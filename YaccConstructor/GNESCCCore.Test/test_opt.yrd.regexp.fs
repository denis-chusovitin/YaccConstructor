//this file was generated by GNESCC
//source grammar:../../../Tests/GNESCC/test_opt/test_opt.yrd
//date:10/13/2011 18:52:15

module GNESCC.Regexp_opt

open Yard.Generators.GNESCCGenerator
open System.Text.RegularExpressions

let buildIndexMap kvLst =
    let ks = List.map (fun (x:string,y) -> x.Length + 2,y) kvLst
    List.fold (fun (bl,blst) (l,v) -> bl+l,((bl,v)::blst)) (0,[]) ks
    |> snd
    |> dict

let buildStr kvLst =
    let sep = ";;"
    List.map fst kvLst 
    |> String.concat sep
    |> fun s -> ";" + s + ";"

let s childsLst = 
    let str = buildStr childsLst
    let idxValMap = buildIndexMap childsLst
    let re = new Regex("((;5;)((;6;))?)")
    let elts =
        let res = re.Match(str)
        if Seq.fold (&&) true [for g in res.Groups -> g.Success]
        then res.Groups
        else (new Regex("((;5;)((;6;))?)",RegexOptions.RightToLeft)).Match(str).Groups
    let e1 =
        if elts.[4].Value <> ""
        then
            let e  =
                let e0 =
                    idxValMap.[elts.[4].Captures.[0].Index] |> RELeaf
                RESeq [e0]
            Some (e)
        else None 
        |>REOpt

    let e0 =
        idxValMap.[elts.[2].Captures.[0].Index] |> RELeaf
    RESeq [e0; e1]

let ruleToRegex = dict [|(1,s)|]

