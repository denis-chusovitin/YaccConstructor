﻿//  ILCheckers.fs contains checkers for IL
//
//  Copyright 2011, 2012 Semen Grigorev <rsdpisuy@gmail.com>
//            2011, 2012 Ilia Shenbin <ilya.shenbin@gmail.com>
//  This file is part of YaccConctructor.
//
//  YaccConstructor is free software:you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

module Yard.Core.Checkers

open Yard.Core.IL.Production
open Yard.Core.IL
open System.Collections.Generic
open System.Linq
open Yard.Core.Helpers

let private startRulesCount (def:Yard.Core.IL.Definition.t<_,_>) =
    def.grammar |> List.sumBy (fun module' -> module'.rules.Count (fun r -> r.isStart))

let IsStartRuleExists def =
    startRulesCount def > 0

let IsSingleStartRule def =
    startRulesCount def = 1

let IsChomskyNormalForm (def:Yard.Core.IL.Definition.t<_,_>) =
    def.grammar.All (fun module' ->
        module'.rules.All (fun r ->
                match r.body with
                | PSeq([{rule = PToken _; omit =_ ; binding =_; checker = _}],_,_)
                | PSeq([{rule = PRef _; omit =_ ; binding =_; checker = _}
                       ;{rule = PRef _; omit =_ ; binding =_; checker = _}],_,_) -> true 
                | _ -> false
            )
        )

let private getAllModuleNames (grammar : Grammar.t<_,_>) =
    grammar
    |> List.map (fun m -> getModuleName m)
    |> List.sort

let GetCoincideModuleNames (def : Yard.Core.IL.Definition.t<Source.t, Source.t>) =
    getAllModuleNames def.grammar
    |> (function
        | [] -> []
        | (x :: xs) -> xs |> List.fold
                              (fun (prev,acc) cur -> (cur, if prev = cur then cur::acc else acc))
                              (x,[])
                       |> snd
       )

let GetInvalidOpenings (def : Yard.Core.IL.Definition.t<Source.t, Source.t>) =
    let existsModule searched =
        def.grammar
        |> List.exists (fun m -> getModuleName m = searched)
    def.grammar
    |> List.choose
        (fun m ->
            let invalidOpenings =
                m.openings
                |> List.filter (fun op -> op.text = getModuleName m || not (existsModule op.text))
            match invalidOpenings with
            | [] -> None
            | _ -> Some (m, invalidOpenings)
        )

let checkModuleRules (publicRules : IDictionary<_,_>) (module' : Grammar.Module<Source.t, Source.t>) = 
    let declaredInnerRules =
        module'.rules |> List.map (fun r -> r.name.text)
    let declaredRules = new HashSet<_>(declaredInnerRules)
    let declaredExportRules =
        module'.openings
        |> List.map (fun op ->
            let rules : Rule.t<_,_> list =
                if publicRules.ContainsKey op.text then publicRules.[op.text]
                else
                    eprintf "Undeclared module %s (%s:%d) " op.text op.file op.startPos.line
                    []
            op.text, rules
        )
    declaredExportRules
    |> List.iter (snd >> List.iter (fun r -> declaredRules.Add r.name.text |> ignore))

    let repeatedInnerRules =
        let rules = new HashSet<_>()
        let repeated = new HashSet<_>()
        module'.rules
        |> List.iter (fun r -> if not <| rules.Add r.name.text then
                                    ignore <| repeated.Add r.name.text)
        repeated |> List.ofSeq

    let repeatedExportRules =
        let ruleToModule = new Dictionary<_,HashSet<_>>()
        let repeated = new HashSet<_>()
        ((getModuleName module', module'.rules) :: declaredExportRules)
        |> List.iter (fun (mName, rules) ->
                rules |> List.iter (fun r ->
                    let rName = r.name.text
                    if ruleToModule.ContainsKey rName then
                        if not <| ruleToModule.[rName].Add mName then
                            repeated.Add rName |> ignore
                    else
                        ruleToModule.[rName] <- new HashSet<_>([mName])
                )
            )
        //module'.rules
        //|> List.iter (fun r -> if not <| rules.Add r.name.text then
        //                            ignore <| repeated.Add r.name.text)
        repeated |> List.ofSeq |> List.map (fun r -> r, List.ofSeq ruleToModule.[r])

    let undeclaredRules = new HashSet<_>()
    let addUndeclaredRule (name : Source.t) additionRules = 
        if not (declaredRules.Contains name.text
                || Seq.exists ((=) name.text) additionRules) && name.text <> errorToken
        then
            undeclaredRules.Add name |> ignore

    let rec getUndeclaredRules additionRules body =
        let getUndeclaredRulesCurried body = getUndeclaredRules additionRules body
        match body with
        | PRef (name,_) -> addUndeclaredRule name additionRules
        | PMetaRef (name,_,exprList) ->  
            addUndeclaredRule name additionRules
            exprList |> List.iter getUndeclaredRulesCurried 
        | PSeq (exprList,_,_) -> exprList |> List.iter (fun r -> getUndeclaredRulesCurried r.rule)
        | PPerm exprList    -> exprList |> List.iter getUndeclaredRulesCurried 
        | PRepet (expr,_,_)
        | PMany expr
        | PSome expr
        | POpt  expr -> getUndeclaredRulesCurried expr
        | PAlt (lExpr,rExpr) -> 
            getUndeclaredRulesCurried lExpr
            getUndeclaredRulesCurried rExpr
        | PLiteral _ 
        | PToken _  -> ()
        | PNegat expr -> getUndeclaredRulesCurried expr
        | PConjuct (lExpr, rExpr) ->
            getUndeclaredRulesCurried lExpr
            getUndeclaredRulesCurried rExpr

    module'.rules
    |> List.iter
        (fun r -> 
            let additionRules = new HashSet<_>()
            r.metaArgs |> List.iter (fun i -> additionRules.Add i.text |> ignore)
            getUndeclaredRules additionRules r.body)

    repeatedInnerRules, repeatedExportRules, List.ofSeq undeclaredRules

let GetUndeclaredNonterminalsList (def : Yard.Core.IL.Definition.t<Source.t, Source.t>) =
    let grammar = def.grammar
    let publicRules = getPublicRules grammar
    let filterEmpty (x : ('a * 'b list)  list) =
        x |> List.filter
            (function
             | (_,[]) -> false
             | _ -> true)
    grammar
    |> List.map (fun m -> m, checkModuleRules publicRules m)
    |> List.map (fun (m,(l1,l2,l3)) ->  (m,l1), (m,l2), (m,l3))
    |> List.unzip3
    |> (fun (x,y,z) -> filterEmpty x, filterEmpty y, filterEmpty z)

// returns a list of rule's names which are reachead from start rule in the grammar
let reachableRulesInfo_of_grammar (grammar: Grammar.t<_,_>) =
    let rulesMap = getRulesMap grammar
    let reachedRules = new HashSet<_>()
    
    let getAdditionRules (rule : Rule.t<Source.t,Source.t>) =
        rule.metaArgs |> List.map (fun i -> i.text)
        |> fun x -> new HashSet<_>(x)

    let rec getReachableRules module' additionRules body : unit =
        let inline getReachableRulesCurried body = getReachableRules module' additionRules body
        match body with
        | PRef (name,_) -> addReachedRule module' name.text additionRules
        | PMetaRef (name,_,exprList) ->  
            addReachedRule module' name.text additionRules
            exprList |> List.iter getReachableRulesCurried 
        | PSeq (exprList,_,_) -> exprList |> List.iter (fun r -> getReachableRulesCurried r.rule)
        | PPerm exprList    -> exprList |> List.iter getReachableRulesCurried
        | PRepet (expr,_,_)
        | PMany expr
        | PSome expr
        | POpt expr -> getReachableRulesCurried expr
        | PAlt (lExpr,rExpr) -> 
            getReachableRulesCurried lExpr
            getReachableRulesCurried rExpr
        | PConjuct (lExpr,rExpr) -> 
            getReachableRulesCurried lExpr
            getReachableRulesCurried rExpr
        | PNegat (expr) -> getReachableRulesCurried expr
        | PLiteral _ 
        | PToken _  -> ()

    and addReachedRule (module' : string) (name : string) (additionRules : HashSet<_>) : unit =  
        let key = module', name
        if not (additionRules.Contains name || reachedRules.Contains key)
        then
            reachedRules.Add key |> ignore
            let newModule = rulesMap.[module'].[name]
            grammar
            |> List.pick (fun m -> if getModuleName m = newModule then Some m.rules else None)
            |> List.find (fun r -> r.name.text = name)
            |> fun rule -> getReachableRules newModule (getAdditionRules rule) rule.body

    let startModule, startRule =
        grammar |> List.pick (fun m ->
            m.rules
            |> List.tryFind (fun r -> r.isStart)
            |> Option.map (fun r -> getModuleName m, r)
        )
    (startModule, startRule.name.text) |> reachedRules.Add |> ignore
    getReachableRules startModule (getAdditionRules startRule) startRule.body
    reachedRules

let reachableRulesInfo (def: Definition.t<_,_>) =
  reachableRulesInfo_of_grammar def.grammar

let IsUnusedRulesExists(def:Yard.Core.IL.Definition.t<_,_>) =
  let reachedRules = reachableRulesInfo def
  def.grammar |> List.exists (fun m ->
        m.rules |> List.exists (fun r -> let v = (getModuleName m, r.name.text) in not <| reachedRules.Contains v)
  )

/// Usage example: check after conversion, that we didn't lose any binding to source (e.g. position)
let sourcesWithoutFileNames (def:Yard.Core.IL.Definition.t<Source.t,Source.t>) =
    let inline check (src : Source.t) = src.file = ""
    let collectName (src : Source.t) = if check src then [src] else []
    let collectOpt (src : Source.t option) =
        match src with
        | Some src when check src -> [src]
        | _ -> []
    let rec processBody = function
        | PRef (name,args) -> collectName name @ collectOpt args
        | PMetaRef (name,args,metas) -> collectName name @ collectOpt args @ List.collect processBody metas
        | PSeq (s,ac,lab) -> collectOpt ac @ (s |> List.collect (fun e -> processBody e.rule))
        | PToken tok | PLiteral tok -> collectName tok
        | PAlt (l,r) -> processBody l @ processBody r
        | PMany e | PSome e | POpt e -> processBody e
        | PPerm p -> List.collect processBody p
        | PRepet (p,_,_) -> processBody p
        | PConjuct (l,r) -> processBody l @ processBody r
        | PNegat expr -> processBody expr

    def.grammar |> List.collect (fun m ->
        m.rules |> List.collect (fun r ->
            List.filter check r.args
            @ collectName r.name
            @ List.filter check r.metaArgs
            @ processBody r.body
        )
    )