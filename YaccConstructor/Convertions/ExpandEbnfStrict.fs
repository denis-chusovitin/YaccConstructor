﻿//  Module ExpandEbnf contains:
//  - functions for rules convertion from EBNF to BNF 
//
//  Copyright 2011 by Konstantin Ulitin
//
//  This file is part of YaccConctructor.
//
//  YaccConstructor is free software: you can redistribute it and/or modify
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

module Yard.Core.Convertions.ExpandEbnfStrict

open Yard.Core
open Yard.Core.IL
open Production
open TransformAux

let s2source s = (s, (0,0))
let generatedSomesCount = ref 0
let genSomeName () =
    generatedSomesCount := !generatedSomesCount + 1
    sprintf "yard_some_%d" !generatedSomesCount

let generatedManysCount = ref 0
let genManyName () =
    generatedManysCount := !generatedManysCount + 1
    sprintf "yard_many_%d" !generatedManysCount

let generatedOptsCount = ref 0
let genOptName () =
    generatedOptsCount := !generatedOptsCount + 1
    sprintf "yard_opt_%d" !generatedOptsCount

let default_elem = {omit=false; rule=PRef(s2source "empty", None); binding=None; checker=None}

let convertToBnf (rule:(Rule.t<Source.t,Source.t>)) = 
    let addedBnfRules = ref []
    let sourceIf cond s = if cond then Some(s2source s) else None
    // if production is not binded then don't add semantic action in generated rules
    let rec replaceEbnf production binded attrs metaArgs = 
        let insideMetaArgs =
            metaArgs
            |> List.map (fun x -> PRef (x, None))
        match production with
        | PSeq(elem_list, ac) ->
            PSeq(elem_list
            |> List.map (fun elem -> {elem with rule = replaceEbnf elem.rule (elem.binding.IsSome) attrs metaArgs}), ac) 
        | PAlt(left, right) -> PAlt(replaceEbnf left binded attrs metaArgs, replaceEbnf right binded attrs metaArgs)
        | PSome(p) -> 
            let generatedName = genSomeName()
            let expandedBody = replaceEbnf p binded attrs metaArgs
            let newRule = PMetaRef(s2source generatedName, list2opt attrs, insideMetaArgs)
            addedBnfRules := (
                {new Rule.t<Source.t,Source.t> 
                 with name = generatedName 
                 and args = attrs 
                 and body =
                    PAlt(
                        PSeq([{default_elem with rule = expandedBody; binding=sourceIf binded "elem"}], sourceIf binded "[elem]") ,
                        PSeq([
                                {omit=false;
                                    rule = expandedBody;
                                    binding=sourceIf binded "head";
                                    checker=None};
                                {omit=false;
                                    rule = newRule;
                                    binding=sourceIf binded "tail";
                                    checker=None}
                             ]
                             , sourceIf binded "head::tail")
                    ) 
                 and _public=false
                 and metaArgs = metaArgs
                }) :: !addedBnfRules
            newRule
        | PMany(p) -> 
            let generatedName = genManyName()
            let expandedBody = replaceEbnf p binded attrs metaArgs
            let newRule = PMetaRef(s2source generatedName, list2opt attrs, insideMetaArgs)
            addedBnfRules := (
                {new Rule.t<Source.t,Source.t> 
                 with name=generatedName 
                 and args = attrs
                 and body=
                    PAlt(
                        PSeq([{default_elem with rule=PRef(s2source "empty", None)}], sourceIf binded "[]") ,
                        PSeq([
                                {omit=false;
                                    rule=expandedBody;
                                    binding=sourceIf binded "head";
                                    checker=None};
                                {omit=false;
                                    rule=newRule;
                                    binding=sourceIf binded "tail";
                                    checker=None}
                             ]
                             , sourceIf binded "head::tail")
                    ) 
                 and _public=false
                 and metaArgs = metaArgs
                }) :: !addedBnfRules
            newRule
        | POpt(p) -> 
            let generatedName = genOptName()
            let expandedBody = replaceEbnf p binded attrs metaArgs
            let newRule = PMetaRef(s2source generatedName, list2opt attrs, insideMetaArgs)
            addedBnfRules := (
                {new Rule.t<Source.t,Source.t> 
                 with name=generatedName 
                 and args = attrs
                 and body=
                    PAlt(
                        PSeq([{default_elem with rule=newRule}], sourceIf binded "None"),
                        PSeq([{default_elem with rule=expandedBody; binding=sourceIf binded "elem"}], sourceIf binded "Some(elem)")
                    ) 
                 and _public=false
                 and metaArgs = metaArgs
                }) :: !addedBnfRules
            newRule
        | x -> x
    {rule with body=replaceEbnf rule.body false rule.args rule.metaArgs}::(List.rev !addedBnfRules)

type ExpandEbnfStrict() = 
    inherit Convertion()
        override this.Name = "ExpandEbnfStrict"
        override this.ConvertList ruleList = ruleList |> List.map (convertToBnf) |> List.concat
        override this.EliminatedProductionTypes = ["POpt"; "PSome"; "PMany"]
