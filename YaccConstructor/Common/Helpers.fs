﻿//   Copyright 2013 YaccConstructor Software Foundation
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

[<AutoOpen>]
module Yard.Core.Helpers

open Yard.Core.IL
open Grammar
open Production
open Definition

let errorToken = "error"

let getModuleName (m : Module<_,_>) = 
    match m.name with
    | Some n -> n.text
    | None -> ""

let defaultModules rules : Grammar.Module<_,_> list =
    [{
        rules = rules
        openings = []
        name = None
        allPublic = false
    }]

let defaultDefinition rules = {empty with grammar = defaultModules rules}

let simpleRules nonTerm body : Rule.t<_,_> list =
    [{
        name = Source.t nonTerm
        args = []
        body = body
        isStart = true
        isPublic = false
        metaArgs = []
    }]

let simpleNotStartRules nonTerm body : Rule.t<_,_> list =
    [{
        name = Source.t nonTerm
        args = []
        body = body
        isStart = false
        isPublic = false
        metaArgs = []
    }]

let verySimpleRules nonTerm seq : Rule.t<_,_> list =
    simpleRules nonTerm <| PSeq(seq, None, None)

let verySimpleNotStartRules nonTerm seq : Rule.t<_,_> list =
    simpleNotStartRules nonTerm <| PSeq(seq, None, None)

let inline mapModule mapF (m : Grammar.Module<_,_>) =
    {m with rules = mapF m.rules}

let inline mapGrammar mapF grammar =
    grammar
    |> List.map (mapModule mapF)

let getPublicRules (grammar : Grammar.t<_,_>) =
    grammar
    |> List.map (fun module' ->
            let publics = module'.rules |> List.filter (fun r -> r.isPublic)
            getModuleName module', publics
        )
    |> dict

/// For each module creates map: rule -> module, in which the rule is declared
let getRulesMap (grammar : Grammar.t<_,_>) =
    let publicRules = getPublicRules grammar
    grammar
    |> List.map (fun module' ->
        let rMap = new System.Collections.Generic.Dictionary<_,_>()
        module'.openings
        |> List.iter (fun op ->
            try 
                publicRules.[op.text] |> List.iter (fun r ->
                    try 
                        rMap.[r.name.text] <- op.text
                    with
                    | e -> printfn "Get rules error: rule name: %A;" r.name.text

                        )
            with
            | e -> printfn "Get rules error: open: %A;" op.text  
            
        )
        module'.rules |> List.iter (fun r -> rMap.[r.name.text] <- getModuleName module')
        getModuleName module', rMap
    )
    |> dict

