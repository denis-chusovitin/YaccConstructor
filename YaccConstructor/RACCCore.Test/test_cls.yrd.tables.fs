//this tables was generated by RACC
//source grammar:..\Tests\RACC\test_cls\\test_cls.yrd
//date:2/3/2011 14:02:13

#light "off"
module Yard.Generators.RACCGenerator.Tables_Cls

open Yard.Generators.RACCGenerator

let autumataDict = 
dict [|("raccStart",{ 
   DIDToStateMap = dict [|(0,(State 0));(1,(State 1));(2,DummyState)|];
   DStartState   = 0;
   DFinaleStates = Set.ofArray [|1|];
   DRules        = Set.ofArray [|{ 
   FromStateID = 0;
   Symbol      = (DSymbol "s");
   Label       = Set.ofArray [|List.ofArray [|(FATrace (TSmbS 0))|]|];
   ToStateID   = 1;
}
;{ 
   FromStateID = 1;
   Symbol      = Dummy;
   Label       = Set.ofArray [|List.ofArray [|(FATrace (TSmbE 0))|]|];
   ToStateID   = 2;
}
|];
}
);("s",{ 
   DIDToStateMap = dict [|(0,(State 0));(1,(State 1));(2,DummyState);(3,DummyState)|];
   DStartState   = 1;
   DFinaleStates = Set.ofArray [|0;1|];
   DRules        = Set.ofArray [|{ 
   FromStateID = 0;
   Symbol      = (DSymbol "MULT");
   Label       = Set.ofArray [|List.ofArray [|(FATrace (TSmbE 2));(FATrace (TSeqE 3));(FATrace (TClsE 1));(FATrace (TSeqE 4))|];List.ofArray [|(FATrace (TSmbE 2));(FATrace (TSeqE 3));(FATrace (TSeqS 3));(FATrace (TSmbS 2))|];List.ofArray [|(FATrace (TSmbE 2));(FATrace (TSeqE 3))|]|];
   ToStateID   = 0;
}
;{ 
   FromStateID = 0;
   Symbol      = Dummy;
   Label       = Set.ofArray [|List.ofArray [|(FATrace (TSmbE 2));(FATrace (TSeqE 3));(FATrace (TClsE 1));(FATrace (TSeqE 4))|];List.ofArray [|(FATrace (TSmbE 2));(FATrace (TSeqE 3));(FATrace (TSeqS 3));(FATrace (TSmbS 2))|];List.ofArray [|(FATrace (TSmbE 2));(FATrace (TSeqE 3))|]|];
   ToStateID   = 2;
}
;{ 
   FromStateID = 1;
   Symbol      = (DSymbol "MULT");
   Label       = Set.ofArray [|List.ofArray [|(FATrace (TSeqS 4));(FATrace (TClsS 1));(FATrace (TClsE 1));(FATrace (TSeqE 4))|];List.ofArray [|(FATrace (TSeqS 4));(FATrace (TClsS 1));(FATrace (TSeqS 3));(FATrace (TSmbS 2))|];List.ofArray [|(FATrace (TSeqS 4));(FATrace (TClsS 1))|]|];
   ToStateID   = 0;
}
;{ 
   FromStateID = 1;
   Symbol      = Dummy;
   Label       = Set.ofArray [|List.ofArray [|(FATrace (TSeqS 4));(FATrace (TClsS 1));(FATrace (TClsE 1));(FATrace (TSeqE 4))|];List.ofArray [|(FATrace (TSeqS 4));(FATrace (TClsS 1));(FATrace (TSeqS 3));(FATrace (TSmbS 2))|];List.ofArray [|(FATrace (TSeqS 4));(FATrace (TClsS 1))|]|];
   ToStateID   = 3;
}
|];
}
)|]

let gotoSet = 
    Set.ofArray [|(-1239003080,Set.ofArray [|("raccStart",2)|]);(-1239003111,Set.ofArray [|("s",2)|]);(-285614526,Set.ofArray [|("s",0)|]);(-635149922,Set.ofArray [|("raccStart",1)|]);(1800920813,Set.ofArray [|("s",3)|]);(1800920844,Set.ofArray [|("s",2)|]);(864571574,Set.ofArray [|("s",0)|]);(864571735,Set.ofArray [|("s",0)|])|]
    |> dict
