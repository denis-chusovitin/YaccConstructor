s0 <- module()
      {
        a <- 0;
        sm1 <- module()
               [foreign
                 language : "Java";
                 filename : "sm1.java";
               ]
               [interface
                 in  : [a, b];
                 out : [a];
               ];

        sm1.a <~ a + 1;
      };