﻿namespace YieldMap.Language

open System
open System.Collections.Generic
open System.Text
open System.Text.RegularExpressions

open YieldMap.Tools.Logging
open YieldMap.Tools.Ratings
open YieldMap.Tools.Aux
open YieldMap.Tools.Response

open Lexan
open Syntan
open Exceptions

#nowarn "62"
module Interpreter = 
    let logger = LogFactory.create "YieldMap.Language.Interpreter"
    // LAZY FUNCTIONS POSSIBLE ONLY IF I FIRST CREATE A TREE FROM A STACK
    // FUNCTIONS WITH VARIABLE NUMBER OF ARGS (OR DIFFERING NUMBER OF ARGS)
    //   ARE NOT IMPLEMENTABLE UNLESS I ADD SOME CALL DELIMITER TO SYNTAX PARSER OUTPUT

    module private Operations = 
        let applyMath op v1 v2 =
            logger.TraceF "applyMath %s %A %A" op v1 v2
            if op = "+" then
                match v1, v2 with
                | Value.Integer i1, Value.Integer i2 -> 
                    Value.Integer (i1 + i2)
                | Value.Double d1, Value.Double d2 -> 
                    Value.Double (d1 + d2)
                | Value.Integer i, Value.Double d | Value.Double d, Value.Integer i -> 
                    Value.Double (double i + d)
                | Value.String s1, Value.String s2 -> 
                    Value.String (s1 + s2)
                | Value.String s, Value.Nothing | Value.Nothing, Value.String s ->
                    Value.String s
                | Value.Rating n, Value.Integer i | Value.Integer i, Value.Rating n -> 
                    match n |> Notch.elevate (int i) with
                    | Answer notch -> notch |> Value.Rating 
                    | Failure f -> raise <| InterpreterException (sprintf "Rating failure %s" (f.ToString()))
                | _, Value.Nothing | Value.Nothing, _ -> 
                    Value.Nothing
                | _ -> raise <| InterpreterException (sprintf "Operation + is not applicable to args %A %A" v1 v2)
            else 
                match v1, v2 with
                | Value.Integer n, Value.Integer m -> 
                    Value.Integer (if op = "*" then n*m elif op = "/" then n/m else n-m)
                | Value.Double n, Value.Double m -> 
                    Value.Double (if op = "*" then n*m elif op = "/" then n/m else n-m)
                | Value.Integer i, Value.Double d ->
                    let n, m = double i, d
                    Value.Double (if op = "*" then n*m elif op = "/" then n/m else n-m)
                | Value.Double d, Value.Integer i -> 
                    let n, m = d, double i
                    Value.Double (if op = "*" then n*m elif op = "/" then n/m else n-m)
                | _, Value.Nothing | Value.Nothing, _ -> 
                    Value.Nothing
                | Value.Rating n, Value.Integer i | Value.Integer i, Value.Rating n when op = "-" -> 
                    match n |> Notch.elevate (int -i) with
                    | Answer notch -> notch |> Value.Rating 
                    | Failure f -> raise <| InterpreterException (sprintf "Rating failure %s" (f.ToString()))
                | _ -> raise <| InterpreterException (sprintf "Operation %s is not applicable to args %A %A" op v1 v2)

        let private getLogicalOperation op = 
            match op with
            | ">"  -> (>)   | "<"  -> (<)
            | ">=" -> (>=)  | "<=" -> (<=)
            | "="  -> (=)   | "<>" -> (<>)
            | _ -> raise <| InterpreterException (sprintf "Unknown logical operation %s" op)

        let (|C|_|) v =
            match v with
            | Value.Integer i -> Some (i :> IComparable)
            | Value.Double d -> Some (d :> IComparable)
            | Value.Date d -> Some (d :> IComparable)
            | Value.Rating n -> Some (n :> IComparable)
            | Value.Bool b -> Some (b :> IComparable)
            | _ -> None

        let applyLogical op v1 v2 =
            logger.TraceF "applyLogical %s %A %A" op v1 v2
            match v1, v2 with
            | C n, C m -> 
                Value.Bool <| (getLogicalOperation op) n m

            | Value.Nothing, _ | _, Value.Nothing -> 
                Value.Nothing

            | _ -> raise <| InterpreterException (sprintf "Operation %s is not applicable to args %A %A" op v1 v2)
                
        let apply op stack = 
            logger.TraceF "apply %s" op
            if op = "_" then
                match stack with
                | (Syntel.Value (Value.Integer i)) :: tail -> 
                    (Syntel.Value (Value.Integer -i)) :: tail 

                | (Syntel.Value (Value.Double d)) :: tail -> 
                    (Syntel.Value (Value.Double -d)) :: tail 

                | head :: _ -> 
                    raise <| InterpreterException (sprintf "Operation - is not applicable to %A" head)

                | _ -> raise <| InterpreterException "Nothing to negate"

            elif op = "not" then
                match stack with
                | (Syntel.Value (Value.Bool b)) :: tail -> 
                    (Syntel.Value (Value.Bool (not b))) :: tail 

                | head :: _ -> 
                    raise <| InterpreterException (sprintf "Operation 'not' is not applicable to %A" head)
                | _ -> 
                    raise <| InterpreterException "Nothing to negate"

            elif op |- set ["+"; "-"; "*"; "/"] then
                match stack with
                | (Syntel.Value v1) :: (Syntel.Value v2) :: tail -> 
                    (Syntel.Value <| applyMath op v2 v1) :: tail 

                | _ -> raise <| InterpreterException "Need two values to apply mathematical operation"

            elif op |- set [">"; "<"; ">="; "<="; "="; "<>"] then
                match stack with // TODO =, <> are not directly applicable to doubles!
                | (Syntel.Value v1) :: (Syntel.Value v2) :: tail -> 
                    (Syntel.Value <| applyLogical op v2 v1) :: tail 

                | _ -> raise <| InterpreterException "Need two values to apply logical operation"

            elif op |- set ["and"; "or"] then
                match stack with
                | (Syntel.Value (Value.Bool b1)) :: (Syntel.Value (Value.Bool b2)) :: tail -> 
                    let o = if op = "and" then (&&) 
                            elif op = "or" then (||) 
                            else raise <| InterpreterException (sprintf "Unknown boolean operation %s" op)
                    (Syntel.Value <| Value.Bool (o b1 b2)) :: tail 

                | _ -> raise <| InterpreterException "Need two values to apply boolean operation"

            else raise <| InterpreterException (sprintf "Unknown operation %s" op)

    module internal Functions = 
        let apply name stack = 
            logger.TraceF "apply function %s" name
            // functions: 
            //  - control : iif
            //  - math : round, floor, ceil, abs
            //  - strings : format, like, contains
            //  - date : AddDays, AddMonths, AddYears
            //  - rating

            // todo any function with nothing is nothing
            if name = "IIF" then
                match stack with 
                | (Syntel.Value ``if-false``) :: (Syntel.Value ``if-true``) :: (Syntel.Value condition) :: tail -> 
                    match condition with
                    | Value.Bool cond -> Syntel.Value (if cond then ``if-true`` else ``if-false``) :: tail
                    | Value.Nothing -> Syntel.Value condition :: tail
                    | _ -> raise <| InterpreterException (sprintf "Invalid Iif function call. Use Iif(bool, value-if-true, value-if-false)")
                | _ -> raise <| InterpreterException (sprintf "Invalid Iif function call. Use Iif(bool, value-if-true, value-if-false)")
            
            elif name = "ROUND" then
                match stack with 
                | (Syntel.Value (Value.Double dbl)) :: tail -> 
                    let value = dbl |> Math.Round
                                    |> int64
                                    |> Value.Integer
                                    |> Syntel.Value
                    value :: tail

                | (Syntel.Value (Value.Integer _)) :: _ | (Syntel.Value (Value.Nothing)) :: _ -> 
                    stack
                                        
                | _ -> raise <| InterpreterException (sprintf "Invalid Round function call. Use Round(double)")

            elif name |- set ["FLOOR"; "CEIL"; "ABS"] then
                match stack with 
                | (Syntel.Value Value.Nothing) :: tail -> 
                    stack

                | (Syntel.Value (Value.Double dbl)) :: tail -> 
                    let math : float -> float = 
                        match name with
                        | "FLOOR" -> Math.Floor
                        | "CEIL" -> Math.Ceiling
                        | "ABS" -> Math.Abs
                        | _ -> failwith ""

                    let value = dbl |> math
                                    |> int64
                                    |> Value.Integer
                                    |> Syntel.Value

                    value :: tail

                | (Syntel.Value (Value.Integer _)) :: _ -> 
                    stack
                                        
                | _ -> raise <| InterpreterException (sprintf "Invalid %s function call. Use %s(double)" name name)

            elif name = "FORMAT" then
                match stack with 
                | (Syntel.Value value) :: (Syntel.Value (Value.String format)) :: tail -> 
                    let res = 
                        match value with
                        | Value.Bool b -> Value.String <| String.Format("{0:" + format + "}", b)
                        | Value.Date d -> Value.String <| String.Format("{0:" + format + "}", d)
                        | Value.Double d -> Value.String <| String.Format("{0:" + format + "}", d)
                        | Value.Integer i -> Value.String <| String.Format("{0:" + format + "}", i)
                        | other -> other
                    Syntel.Value res :: tail

                | Syntel.Value Value.Nothing :: _ :: _ 
                | _ :: Syntel.Value Value.Nothing :: _ -> 
                    stack

                | _ -> raise <| InterpreterException "Invalid Format function call. Use Format(format, value)" 

            elif name = "LIKE" then
                match stack with 
                | (Syntel.Value (Value.String str)) :: (Syntel.Value (Value.String regex)) :: tail -> 
                    let m = Regex.Match(str, regex)
                    (Syntel.Value <| Value.Bool m.Success) :: tail

                | Syntel.Value Value.Nothing :: _ :: _ 
                | _ :: Syntel.Value Value.Nothing :: _ -> 
                    stack

                | _ -> raise <| InterpreterException "Invalid Like function call. Use Like(str, regex-str)" 

            elif name = "ISNOTHING" then
                match stack with 
                | (Syntel.Value Value.Nothing) :: tail -> (Syntel.Value (Value.Bool true)) :: tail 
                | _ :: tail -> (Syntel.Value (Value.Bool false)) :: tail 
                | _ -> raise <| InterpreterException "Invalid IsNothing function call. Use Contains(some-value-or-variable))" 

            elif name = "FORMAT" then
                match stack with 
                | (Syntel.Value (Value.String format)) :: (Syntel.Value value) :: tail -> 
                    try
                        let res = String.Format(format, Value.boxify value)
                        (Syntel.Value (Value.String res)) :: tail 
                    with :? FormatException as e ->
                        raise <| InterpreterException(sprintf "Invalid format: %s" e.Message)
                | _ -> raise <| InterpreterException "Invalid Format function call. Use Foramt(format, some-value-or-variable))" 

            elif name = "CONTAINS" then
                match stack with 
                | (Syntel.Value (Value.String needle)) :: (Syntel.Value (Value.String haystack)) :: tail -> 
                    (Syntel.Value <| Value.Bool (haystack.Contains(needle))) :: tail

                | Syntel.Value Value.Nothing :: _ :: _ 
                | _ :: Syntel.Value Value.Nothing :: _ -> 
                    stack

                | _ -> raise <| InterpreterException "Invalid Contains function call. Use Contains(haystack-string, needle-string)" 
                
            elif name |- set ["ADDDAYS"; "ADDMONTHS"; "ADDYEARS"] then
                match stack with 
                | Syntel.Value Value.Nothing :: _ :: _ 
                | _ :: Syntel.Value Value.Nothing :: _ -> 
                    stack

                | (Syntel.Value (Value.Integer num)) :: (Syntel.Value (Value.Date dt)) :: tail -> 
                    let add = 
                        match name with
                        | "ADDDAYS" -> dt.AddDays << float
                        | "ADDMONTHS" -> dt.AddMonths
                        | "ADDYEARS" -> dt.AddYears
                        | _ -> raise <| InterpreterException (sprintf "Unexpected call to %s" name)

                    let newDate = num |> int 
                                      |> add 
                                      |> Value.Date 
                                      |> Syntel.Value

                    newDate :: tail

                | _ -> raise <| InterpreterException (sprintf "Invalid %s function call. Use %s(date, int)" name name)

            else failwith <| sprintf "Unknown function %s" name
            
    let private getVariable (vars : (string, obj) Map) var = 
        match var with
        | Variable.Global name -> 
            if vars |> Map.containsKey name then
                Value.interpret vars.[name]
            else Value.Nothing

        | Variable.Object (name, field) -> 
            if vars |> Map.containsKey name then
                let map = vars.[name] :?> (string, obj) Map
                if map |> Map.containsKey field then
                    Value.interpret map.[field]
                else Value.Nothing
            else Value.Nothing

    let evaluate (grammar, expr, vars : (string, obj) Dictionary) = 
        let vars = vars |> Map.fromDict
        
        let items = 
            ([], grammar)
            ||> Seq.fold (fun progress i -> 
                match snd i with
                | Syntel.Value _ & output -> 
                    output :: progress

                | Syntel.Variable var -> 
                    (Syntel.Value <| getVariable vars var) :: progress

                | Syntel.Operation op -> 
                    try
                        Operations.apply op progress
                    with :? InterpreterException as e ->
                        raise <| GrammarException { str = expr; message = e.Data0; position = fst i }

                | Syntel.Function name -> 
                    try
                        Functions.apply name progress
                    with :? InterpreterException as e ->
                        raise <| GrammarException { str = expr; message = e.Data0; position = fst i })
        
        match items with 
        | (Syntel.Value v) :: [] -> v
        | _ ->  raise <| GrammarException { str = expr; message = "Function not fully evaluated"; position = 0 }