﻿// TableInterpretator.fs
//
// Copyright 2009 Semen Grigorev
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation.
 
namespace Yard.Core

open Grammar.Item
open AST
open Utils
open Lexeme 

type TableInterpretator (tables: Tables,getLexeme) = class

  let m_end = {name = "$";value = "$"}
             
  let is_start symbol_name = List.exists ((=) symbol_name) tables.StartNterms

  let memoize f =
     let t = new System.Collections.Generic.Dictionary<_,_>()
     fun (parserState:ParserState<_,_,_>) ->        
         let id = hash(parserState)
         let key = parserState
         if t.ContainsKey(key)       
         then t.[key] 
         else 
            let res = f(parserState) 
            t.Add(key,res)
            res                     

  let goto (states,symbol) = 
      Set.unionMany 
        <| seq { for (state:State<_,_,_>) in states 
                 -> set <| seq {for z in (tables.GotoSet.[hash (state.item,symbol)]) 
                                -> State(z,state.trees)}}             
                                     
  let rec climb = 
    let inline calculate states symbol position=    
      let gt = goto (states,symbol)     
      let new_states = parse (ParserState(gt,symbol,position))
      #if DEBUG      
      Log.print_climb_info position symbol states gt new_states        
      #endif     
      
      let inline checker (parserResult:ParserResult<_,_,_>) =
        let item = parserResult.state.item 
        is_start item.prod_name && item.next_num=None && position=1
                
      if Set.exists checker new_states     
      then set <|seq {for state in states do
                        if state.item.next_num = None 
                        then yield ParserResult(state,1)}
      else
        seq {for (parserResult:ParserResult<_,_,_>) in new_states do
               let item,trees = parserResult.state.item, parserResult.state.trees                             
               let prev_itms = prevItem item tables.Items
               let inline checker item = Option.get item.symb = symbol && item.item_num=item.s
               if Set.exists checker prev_itms && not(is_start item.prod_name)  
               then 
                  let create_new_item (state:State<_,_,_>) =
                    let newNode = 
                      Node(state.trees@trees
                           ,item.prod_name
                           ,{prodNum = item.prod_num;
                             seqNum = item.seq_number;                                                          
                             varNum = 1;
                             value = Value.NodeV(null:obj)})
                    State(state.item,[newNode]) 
                  let newStates = climb(ParserState(Set.map create_new_item states, item.prod_name, position))
                  let inline filter (parserResult:ParserResult<_,_,_>) = parserResult.state.item.item_num > 0
                  yield Set.filter filter newStates
               let inline checker (state:State<_,_,_>) = Set.exists ((=)item) (nextItem state.item tables.Items)
               if Set.exists checker states
               then 
                  let inline createResult item = 
                    ParserResult(State(item,states.MinimumElement.trees@trees), position)
                  yield Set.map createResult prev_itms
              }  |> Set.unionMany  
               
    let climbFunction (parserState:ParserState<_,_,_>) =
      let states,symbol,position = parserState.states, parserState.symbol, parserState.position                      
      if Set.isEmpty states
      then Set.empty
      else calculate states symbol position  
 
    memoize(fun parserState -> climbFunction parserState)

  and parse =
      memoize         
       (fun (parserState) -> 
            let states = parserState.states
            let position = parserState.position
            #if DEBUG 
            Log.print_parse states position;
            #endif
            let value = (getLexeme position)
            let text = (getLexeme position).name
            let leaf_tree item = 
                  [Leaf(text,{prodNum = item.prod_num;
                              seqNum = item.seq_number;
                              varNum = 1;
                              value = Value.LeafV(value)})]
                  
            let new_states = Set.filter (fun (state:State<_,_,_>) -> state.item.next_num = None) states          
            let result_states states create_tree =
                  set <| seq{ for (state:State<_,_,_>) in states -> State(state.item,create_tree state.item)}                  
            Set.map (fun state -> ParserResult(state,position))(result_states new_states (fun _ -> []))
            + 
            if (getLexeme position = m_end) 
            then Set.empty 
            else climb(ParserState(result_states states leaf_tree,text,position-1))
      )
        
  let run inputLength =      
      let startItems = Set.filter (fun item ->is_start item.prod_name) tables.Items
      parse (ParserState(Set.map (fun item -> State(item,[])) startItems,"",inputLength))
      
  member self.Run inputLength = run inputLength
end