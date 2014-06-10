﻿namespace YieldMap.Tests.Unit

open System

open NUnit.Framework
open FsUnit

module Language = 
    open System.Collections.Generic

    open YieldMap.Language
    open YieldMap.Language.Analyzer
    open YieldMap.Tools.Logging
   
    let logger = LogFactory.create "UnitTests.Language"

    let lexCatch f = 
        try 
            f ()
            failwith "No error"
        with :? LexicalException as e ->
            e.Data0.position

    [<Test>]
    let ``Parsing values: numbers (int and double)`` () =
        Lexem.parse "1" |> should be (equal [0, Lexem.Value <| Value.Integer 1L])
        Lexem.parse "   1   " |> should be (equal [3, Lexem.Value <| Value.Integer 1L])
        Lexem.parse "1   " |> should be (equal [0, Lexem.Value <| Value.Integer 1L])
        Lexem.parse "1 2" |> should be (equal 
            [
                0, Lexem.Value <| Value.Integer 1L
                2, Lexem.Value <| Value.Integer 2L
            ])
        Lexem.parse "   1   2 " |> should be (equal 
            [
                3, Lexem.Value <| Value.Integer 1L
                7, Lexem.Value <| Value.Integer 2L
            ])
        Lexem.parse "1.0 2" |> should be (equal 
            [
                0, Lexem.Value <| Value.Double 1.0
                4, Lexem.Value <| Value.Integer 2L
            ])
        Lexem.parse "1.0 2.4" |> should be (equal 
            [
                0, Lexem.Value <| Value.Double 1.0
                4, Lexem.Value <| Value.Double 2.4
            ])

    [<Test>]
    let ``Parsing values: dates`` () =
        Lexem.parse "#1/2/2013#" |> should be (equal [0, Lexem.Value <| Value.Date (DateTime(2013,2,1))])
        Lexem.parse "  #21/12/2013#  " |> should be (equal [2, Lexem.Value <| Value.Date (DateTime(2013,12,21))])

    [<Test>]
    let ``Parsing values: booleans`` () =
        Lexem.parse "true" |> should be (equal [0, Lexem.Value <| Value.Bool true])
        Lexem.parse "false" |> should be (equal [0, Lexem.Value <| Value.Bool false])
        Lexem.parse "  False  FALSE   TRUE   tRuE" |> should be (equal 
            [
                2, Lexem.Value <| Value.Bool false
                9, Lexem.Value <| Value.Bool false
                17, Lexem.Value <| Value.Bool true
                24, Lexem.Value <| Value.Bool true
            ])

    [<Test>]
    let ``Parsing values: ratings`` () =
        Lexem.parse "[BBB+]" |> should be (equal [0, Lexem.Value <| Value.Rating "BBB+"])
        Lexem.parse "[]" |> should be (equal [0, Lexem.Value <| Value.Rating ""])
        Lexem.parse "  [sdf]  [AAA]   [wow!]   [-+-+]" |> should be (equal 
            [
                2, Lexem.Value <| Value.Rating "sdf"
                9, Lexem.Value <| Value.Rating "AAA"
                17, Lexem.Value <| Value.Rating "wow!"
                26, Lexem.Value <| Value.Rating "-+-+"
            ])

    [<Test>]
    let ``Parsing values: strings`` () =
        Lexem.parse "\"hello world!\"" |> should be (equal [0, Lexem.Value <| Value.String "hello world!"])
        Lexem.parse "\"Hello World!\"" |> should be (equal [0, Lexem.Value <| Value.String "Hello World!"])
        Lexem.parse "\"Hello    World!\"" |> should be (equal [0, Lexem.Value <| Value.String "Hello    World!"])
        Lexem.parse "\"hello world!\"     \"hello world!\"            \"Hello    World!\" \"Hello World!\"       " |> should be (equal 
            [
                0, Lexem.Value <| Value.String "hello world!"
                19, Lexem.Value <| Value.String "hello world!"
                45, Lexem.Value <| Value.String "Hello    World!"
                63, Lexem.Value <| Value.String "Hello World!"
            ])
        Lexem.parse "\"hello world!\"\"hello world!\""|> should be (equal 
            [
                0, Lexem.Value <| Value.String "hello world!"
                14, Lexem.Value <| Value.String "hello world!"
            ])
    
    [<Test>]
    let ``Parsing lexems: brackets`` () =
        Lexem.parse "(() ) (  " |> should be (equal 
            [
                0, Lexem.OpenBracket
                1, Lexem.OpenBracket
                2, Lexem.CloseBracket
                4, Lexem.CloseBracket
                6, Lexem.OpenBracket
            ])

    [<Test>]
    let ``Parsing lexems: variables`` () =
        // valid variable names
        Lexem.parse "$a" |> should be (equal [0, Lexem.Variable <| Variable.Global "A"])
        Lexem.parse "$a.b" |> should be (equal [0, Lexem.Variable <| Variable.Object ("A", "B")])
        Lexem.parse "$a.b $F $eee_11.a23" |> should be (equal 
            [
                0, Lexem.Variable <| Variable.Object ("A", "B")
                5, Lexem.Variable <| Variable.Global "F"
                8, Lexem.Variable <| Variable.Object ("EEE_11", "A23")
            ])
        
        // invalid variable names
        lexCatch (fun () -> Lexem.parse "$a.b $c $DDDD.312r4wefr3" |> ignore) |> should be (equal 13)
        lexCatch (fun () -> Lexem.parse "$1" |> ignore) |> should be (equal 0)
        lexCatch (fun () -> Lexem.parse "$a $_" |> ignore) |> should be (equal 3)

    [<Test>]
    let ``Parsing lexems: function calls`` () =
        Lexem.parse "Hello()" |> should be (equal [0, Lexem.FunctionCall <| { name = "HELLO"; parameters = [] }])
        Lexem.parse "Hello(12)" |> should be (equal 
            [
                0, Lexem.FunctionCall  
                    { 
                        name = "HELLO"; parameters = 
                            [
                                6, Lexem.Value <| Value.Integer 12L
                            ] 
                    }
            ])
        Lexem.parse "OhMyGod(12, 23, $a, true)" |> should be (equal 
            [
                0, Lexem.FunctionCall  
                    { 
                        name = "OHMYGOD"; parameters = 
                            [
                                8, Lexem.Value <| Value.Integer 12L
                                10, Lexem.Comma
                                12, Lexem.Value <| Value.Integer 23L
                                14, Lexem.Comma
                                16, Lexem.Variable <| Variable.Global "A"
                                18, Lexem.Comma
                                20, Lexem.Value <| Value.Bool true
                            ] 
                    }
            ])
        Lexem.parse "OhMyGod(Hello(12), Bye(23, $alpha.beta_11), $a, false)" |> should be (equal 
            [ 0, Lexem.FunctionCall  
                { name = "OHMYGOD"; parameters = 
                    [   8, Lexem.FunctionCall { name = "HELLO"; parameters = [14, Lexem.Value <| Value.Integer 12L] }
                        17, Lexem.Comma
                        19, Lexem.FunctionCall 
                            { name = "BYE"; parameters = 
                                [
                                    23, Lexem.Value <| Value.Integer 23L
                                    25, Lexem.Comma
                                    27, Lexem.Variable <| Variable.Object ("ALPHA", "BETA_11")
                                ]}
                        42, Lexem.Comma
                        44, Lexem.Variable <| Variable.Global "A"
                        46, Lexem.Comma
                        48, Lexem.Value <| Value.Bool false
                    ]}])

    [<Test>]
    let ``Parsing lexems: operations`` () = 
        Lexem.parse "+" |> should be (equal [0, Lexem.Operation "+"])
        Lexem.parse "-" |> should be (equal [0, Lexem.Operation "-"])
        Lexem.parse "*" |> should be (equal [0, Lexem.Operation "*"])
        Lexem.parse "/" |> should be (equal [0, Lexem.Operation "/"])
        Lexem.parse "and" |> should be (equal [0, Lexem.Operation "and"])
        Lexem.parse "or" |> should be (equal [0, Lexem.Operation "or"])
        Lexem.parse "not" |> should be (equal [0, Lexem.Operation "not"])
        Lexem.parse "=" |> should be (equal [0, Lexem.Operation "="])
        Lexem.parse "<>" |> should be (equal [0, Lexem.Operation "<>"])
        Lexem.parse ">" |> should be (equal [0, Lexem.Operation ">"])
        Lexem.parse "<" |> should be (equal [0, Lexem.Operation "<"])
        Lexem.parse ">=" |> should be (equal [0, Lexem.Operation ">="])
        Lexem.parse "<=" |> should be (equal [0, Lexem.Operation "<="])

    [<Test>]
    let ``Parsing lexems: all together`` () = 
        Lexem.parse "1+2=4" |> should be (equal 
            [0, Lexem.Value <| Value.Integer 1L
             1, Lexem.Operation "+"
             2, Lexem.Value <| Value.Integer 2L
             3, Lexem.Operation "="
             4, Lexem.Value <| Value.Integer 4L])

        Lexem.parse "If($a = \"hello\", $x.alpha + 1, 0)" |> should be (equal 
            [0, Lexem.FunctionCall 
                { name = "IF"; parameters = 
                    [3, Lexem.Variable <| Variable.Global "A"
                     6, Lexem.Operation "="
                     8, Lexem.Value <| Value.String "hello"
                     15, Lexem.Comma
                     17, Lexem.Variable <| Variable.Object ("X", "ALPHA")
                     26, Lexem.Operation "+"
                     28, Lexem.Value <| Value.Integer 1L
                     29, Lexem.Comma
                     31, Lexem.Value <| Value.Integer 0L]}])

module Parser = 
    open System.Collections.Generic

    open YieldMap.Parser
    open YieldMap.Tools.Logging
   
    let logger = LogFactory.create "UnitTests.Parser"

    let error expr l = 
        let p = Parser()
        try
            let grammar = p.Parse expr
            grammar.Count |> should be (equal l)
            None
        with :? Exceptions.ParserException as e ->
            logger.ErrorEx "" e
            Some e.ErrorPos


    let eval expr = 
        let p = Parser()
        try
            let grammar = p.Parse expr
            let i = Interpreter grammar
            Some (i.Evaluate <| Dictionary<_,_>())
            
        with :? Exceptions.ParserException as e ->
            logger.ErrorEx "" e
            None

    [<Test>]
    let ``Single-term expressions, spaces and brackets`` () =
        error "$a = 2" 3 |> should be (equal None) 
        error "$a= 2" 3 |> should be (equal None) 
        error "$a =2" 3 |> should be (equal None)
        error "$a=2" 3 |> should be (equal None)
        error "($a = 2)" 3 |> should be (equal None)
        error "($a= 2)" 3 |> should be (equal None)
        error "($a =2)" 3 |> should be (equal None)
        error "($a=2)" 3 |> should be (equal None)
        error "( $a = 2)" 3 |> should be (equal None)
        error "( $a= 2)" 3 |> should be (equal None)
        error "( $a =2)" 3 |> should be (equal None)
        error "( $a=2)" 3 |> should be (equal None)
        error "($a = 2 )" 3 |> should be (equal None)
        error "($a= 2 )" 3 |> should be (equal None)
        error "($a =2 )" 3 |> should be (equal None)
        error "($a=2 )" 3 |> should be (equal None)
        error "($a = 2) " 3 |> should be (equal None)
        error "($a= 2) " 3 |> should be (equal None)
        error "($a =2) " 3 |> should be (equal None)
        error "($a=2) " 3 |> should be (equal None)