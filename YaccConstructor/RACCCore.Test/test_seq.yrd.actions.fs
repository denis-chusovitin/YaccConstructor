//this file was generated by RACC
//source grammar:..\Tests\RACC\test_seq\\test_seq.yrd
//date:2/3/2011 14:02:09

module RACC.Actions_Seq

open Yard.Generators.RACCGenerator

let value x = (x:>Lexeme<string>).value

let s0 expr = 
    let inner  = 
        match expr with
        | RESeq [x0; _; x1] -> 
            let (l) =
                let yardElemAction expr = 
                    match expr with
                    | RELeaf tNUMBER -> tNUMBER :?> 'a
                    | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRELeaf was expected." |> failwith

                yardElemAction(x0)

            let (r) =
                let yardElemAction expr = 
                    match expr with
                    | RELeaf tNUMBER -> tNUMBER :?> 'a
                    | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRELeaf was expected." |> failwith

                yardElemAction(x1)
            ((value l |> float) + (value r |> float))
        | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRESeq was expected." |> failwith
    box (inner)

let ruleToAction = dict [|("s",s0)|]


//test footer

