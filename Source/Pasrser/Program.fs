﻿//Григорьев Семён. Рекурсивно всходящий анализатор. Работает  с арифмитическими выражениями
// E->E+E
// E->E*E
// E->(E)
// E-> a

//Как видно, грамматика неоднознаная.

#light "off"

open Set
open System
open Log

// debug = true - печатается трасса. Иначе нет.
let debug = true

//interacive = true - ввод строки с консоли. иначе - явная подстановка тестовой строки
let interacive = false

let start_time = ref System.DateTime.Now                                   

type Symb = 
     |STerm of char
     |SNTerm of char
     
type ISymb  = 
     |Term of char
     |NTerm of char
     |Dot   of char*int // 0 -> NTrem; 1-> Term
     
type rule = Rule of  char *(Symb list)

type item = Item of char*(ISymb list)

let lex_list = ['E';'a';'+';'*';' ';'S';')';'(']

let rules = [Rule('S',[SNTerm('E')]);Rule('E',[SNTerm('E');STerm('+');SNTerm('E')]);Rule('E',[SNTerm('E');STerm('*');SNTerm('E')]);
             Rule('E',[STerm('a')]);Rule('E',[STerm('(');SNTerm('E');STerm('(')])] 
             
// item = (item_num,dot_pos,left_hand_part)             
let _items = 
        let i,j =ref 0 ,ref 0 in    
        List.map (fun (Rule(e,lst))->i:=!i+1;j:=0;(!i,!j,e)::(List.map (fun x -> j:=!j+1;(!i,!j,e))) lst)rules
        
let rules_map = let i = ref 0 in List.map (fun x -> i:=!i+1;(i,x)) rules

let next_set =                
        List.map (fun lst -> (List.map (fun (i,j,s)->(if j+1 = (List.length lst) then ((i,j),(i,-1)) else ((i,j),(i,j+1))))) lst) _items
    
let get,_next,_print = 
    let _lex_list = ref   (Seq.to_list "a+a*a**(a+a)$")
                         //(Seq.to_list "a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))$")
                                      //"a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))+a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))$")
    in
    let get () =  List.hd !_lex_list
    in
    let next () = _lex_list := if !_lex_list = [] 
                              then []
                              else List.tl !_lex_list                               
    in    
    let _print ()= !_lex_list
    in
    get,next,_print           
     
do print_any  (_items);
   print_any  (next_set)

let items = 
    [Set.of_list[Item('S',[Dot('E',0)])]
    ;Set.of_list[Item('E',[Dot('E',0);Term('+');NTerm('E')])]
    ;Set.of_list[Item('E',[NTerm('E');Dot('+',1);NTerm('E')])]
    ;Set.of_list[Item('E',[NTerm('E');Term('+');Dot('E',0)])]
    ;Set.of_list[Item('E',[NTerm('E');Term('+');NTerm('E');Dot(' ',1)])]
    ;Set.of_list[Item('E',[Dot('E',0);Term('*');NTerm('E')])]
    ;Set.of_list[Item('E',[NTerm('E');Dot('*',1);NTerm('E')])]
    ;Set.of_list[Item('E',[NTerm('E');Term('*');Dot('E',0)])]
    ;Set.of_list[Item('E',[NTerm('E');Term('*');NTerm('E');Dot(' ',1)])]
    ;Set.of_list[Item('E',[Dot('a',1)])]
    ;Set.of_list[Item('E',[Term('a');Dot(' ',1)])]
    ;Set.of_list[Item('E',[Dot('(',1);NTerm('E');Term(')')])]
    ;Set.of_list[Item('E',[Term('(');Dot('E',0);Term(')')])]
    ;Set.of_list[Item('E',[Term('(');NTerm('E');Dot(')',1)])]
    ;Set.of_list[Item('E',[Term('(');NTerm('E');Term(')');Dot(' ',1)])]
    ]

let Q = Set.union_all (Set.of_list[Item('S',[NTerm('E');Dot(' ',1)])]::items)
let Q1 = Set.of_list (List.concat _items)
           
let getText a (x,y,z)= 
    match a 
    with 
    |Term(t) -> if x = 0 then ' ' else t 
    |NTerm(t)-> if y = 0 then ' ' else t
    |Dot(t,i)  -> if z = 0 then ' ' else t 
    
//запоминалка. Используем для запоминания результатов ф-ий parse и climb   
let memoize (f: 'a ->'b) =
let t = new System.Collections.Generic.Dictionary<'a,'b>() 
in
fun a ->    
    let l = List.length (_print())
    in
    if t.ContainsKey(a)
    then t.[(a)]
    else 
    let res = f a 
    in
    t.Add((a),res);
    res    
       
do start_time := System.DateTime.Now;
   printfn "Closure and goto calculation.\nStart time: %A" System.DateTime.Now
    
let closure = 
    memoize(fun q -> 
    let ex = new System.Collections.Generic.KeyNotFoundException()
    in
    let rec cl i q = 
        if i = Set.count q 
        then q
        else
         let next_cl f = cl (i+1) (union_all [q;filter f Q])
         in 
         next_cl (fun x -> 
                      let el_for_cl = (List.nth (Set.to_list q) i)
                      in  
                      let getDot lst = List.find (function Dot(_,0)->true| _ -> false ) lst
                      in                             
                      match x 
                      with 
                      | Item (b,hd1::tl1) -> (fun (Item(a,lst)) -> 
                                                  try(b = (getText (getDot lst) (0,0,1)) && 
                                                      (function Dot(_)->true | _ -> false) hd1)
                                                  with ex -> false)                                                           
                                              el_for_cl
                      |_ -> false                     
                  )
    in
    cl 0 q)

let rec next dlst = 
    match dlst
    with
    | Dot(a,0)::NTerm(x)::tl -> NTerm(a)::Dot(x,0)::tl
    | Dot(a,1)::NTerm(x)::tl -> Term(a)::Dot(x,0)::tl
    | Dot(a,0)::Term(x)::tl -> NTerm(a)::Dot(x,1)::tl
    | Dot(a,1)::Term(x)::tl -> Term(a)::Dot(x,1)::tl
    | Dot(a,1)::[] -> Term(a)::Dot(' ',1)::[]
    | Dot(a,0)::[] -> NTerm(a)::Dot(' ',1)::[]
    | hd::tl -> hd::(next tl)
    | [] -> []
    
let rec prev dlst = 
    match dlst
    with
    | Term(x)::Dot(' ',1)::tl -> Dot(x,1)::tl
    | NTerm(x)::Dot(' ',1)::tl-> Dot(x,0)::tl
    | NTerm(x)::Dot(a,0)::tl  -> Dot(x,0)::NTerm(a)::tl
    | NTerm(x)::Dot(a,1)::tl  -> Dot(x,0)::Term(a)::tl
    | Term(x)::Dot(a,0)::tl   -> Dot(x,1)::NTerm(a)::tl
    | Term(x)::Dot(a,1)::tl   -> Dot(x,1)::Term(a)::tl
    | (Dot(a,_)::[])as x      -> x                            
    | hd::tl                  -> hd::(prev tl)
    | []                      -> []                             
                           
  
let closure_set = 
    let t = System.Collections.Generic.Dictionary<item,Set<item>>()
    in
    Set.iter (fun x -> t.Add(x,closure (Set.add  x empty)))Q;
    t

let goto_set = 
    let make_goto q x =     
    let cl = union_all (Set.map (fun x -> closure_set.[x]) q)
    in 
    Set.map (fun (Item(a,lst)) -> Item(a,next lst))
            (Set.filter (fun (Item(a,lst)) -> (List.exists (fun y -> (x = getText y (0,0,1))) lst) ) cl)
    in
    let t = System.Collections.Generic.Dictionary<(item*char),Set<item>>() 
    in
    List.iter (fun x-> (Set.iter (fun y-> t.Add((y,x),(make_goto (add y empty)) x)))Q) lex_list;
    t  
//это предпросчёт goto. сам анализатор тогда работает быстрее. (closure - очень дорогая операция)           
let goto (q,x) = try union_all (Set.map (fun y -> goto_set.[(y,x)])q ) with _ -> empty
                       
let ItNext (Item(a,lst)) = Item(a,next lst)
let ItPrev (Item(a,lst)) = Item(a,prev lst)
   
let union_from_Some set = set |> List.filter Option.is_some |> List.map Option.get |> Set.of_list                              
   
do printfn "End time: %A Total: %A" System.DateTime.Now (System.DateTime.Now - (!start_time))

let rec climb =
    memoize (fun (q,x,i) -> 
    if debug then print_climb_1 (_print()) x q;
    if q = empty
    then empty
    else
    let gt =  goto (q,x)
    in
    if debug then print_climb_2 gt;
    let new_q = parse (gt,i)
    in 
    if debug then print_climb_3 new_q;
    if Set.exists (fun x->x = (Item('S',[NTerm('E');Dot(' ',1)])))new_q
    then new_q
    else    
    Set.union_all                            
    [Set.filter (fun x1-> 
                   Set.exists (fun (Item(ch,lst) as y) -> 
                                   (ItNext y) = x1
                                   &&(function Dot(_)::tl-> false
                                              |[]        -> false
                                              | _        -> true)lst
                               )q)new_q
     |>Set.map (fun x1->((ItPrev (x1))))                      
    
    ;
    Set.union_all(
    union_from_Some[for (Item(a,hd::tl)) in new_q -> 
                        if getText hd (1,1,1)=x && (a<>'S')&&(function Dot(_)::tl->true | _ -> false)tl
                        then Some(climb (q,a,i))
                        else None])
    ])                

and parse = 
    memoize (fun (q,i) -> 
    if debug  then print_parse q (_print());    
    union_all
        [map (fun x -> x )(Set.filter (fun(Item(a,lst)) -> (List.hd (List.rev lst)) = Dot(' ',1))q)
         ;if (let p = get()in p = '$') then empty else (let x = get () in _next();climb (q,  (*(List.hd i)*)x,i-1))
         ;union_from_Some[for (Item(a,lst)) in Q -> match lst 
                                                    with
                                                    | NTerm('`')::[]->Some (climb (q, a,i))
                                                    | _             ->None]|> union_all 
         ])
                 
let res str = 
    start_time:=System.DateTime.Now;
    printfn "Start time: %A" System.DateTime.Now;
    not(parse (items.Head,( List.length (_print ())))=empty)
 
let test_str1 = "a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))"

let test_str2 = "a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))+a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))"

let test_str3 = "a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))+a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))+a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))+a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))*a+a*a*(a+a)*a+a*a*(a+a)+a*a*(a+a)*a+a*a*(a+a)+a+a*a*(a+a)*a+a*a*(a+a)+(a*a*(a+a)*a+a*a*(a+a))"
     
do let str = 
       if interacive
       then 
        (Console.WriteLine("Insert string:"); 
        Console.ReadLine())
       else
        test_str1 
   in     
   let r = res(str) in
   printfn "Result : %A" r;
   Console.WriteLine();
   printfn "End time: %A Total: %A" System.DateTime.Now (System.DateTime.Now - (!start_time));   
   Console.ReadLine()|>ignore