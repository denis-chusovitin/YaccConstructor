//this file was generated by GNESCC
//source grammar:../../../Tests/GNESCC/claret/braces_2/test_simple_braces_2.yrd
//date:10/13/2011 18:52:14

module GNESCC.Actions_simple_braces_2

open Yard.Generators.GNESCCGenerator

let getUnmatched x expectedType =
    "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\n" + expectedType + " was expected." |> failwith
let start0 expr = 
    let inner  = 
        match expr with
        | RESeq [x0] -> 
            let (cntList) =
                let yardElemAction expr = 
                    match expr with
                    | REClosure(lst) -> 
                        let yardClsAction expr = 
                            match expr with
                            | RESeq [_; gnescc_x0; _] -> 

                                let (gnescc_x0) =
                                    let yardElemAction expr = 
                                        match expr with
                                        | RELeaf start -> (start :?> _ ) 
                                        | x -> getUnmatched x "RELeaf"

                                    yardElemAction(gnescc_x0)

                                (gnescc_x0 )
                            | x -> getUnmatched x "RESeq"

                        List.map yardClsAction lst 
                    | x -> getUnmatched x "REClosure"

                yardElemAction(x0)
            ( cntList.Length + List.sum cntList )
        | x -> getUnmatched x "RESeq"
    box (inner)

let ruleToAction = dict [|(1,start0)|]

