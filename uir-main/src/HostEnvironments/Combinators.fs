namespace Mephi.Cybernetics.Race.Core.Host.Definitions

open Mephi.Cybernetics.Race.Core.Host.Computing
open System

module internal Functions = 
    let Unwrap2<'T> (items : seq<'T>) : ('T*'T) =
        let a1 = Seq.head items in
        let a2 = items |> Seq.skip 1 |> Seq.head in
        (a1,a2)
    let Unwrap3<'T> (items : seq<'T>) : ('T*'T*'T) =
        let a1 = Seq.head items in
        let s2 = items |> Seq.skip 1
        let a2 = s2 |> Seq.head in
        let s3 = s2 |> Seq.skip 1
        let a3 = s3 |> Seq.skip 1 |> Seq.head in
        (a1,a2,a3)
    let Unwrap4<'T> (items : seq<'T>) : ('T*'T*'T*'T) =
        let a1 = Seq.head items in
        let a2 = items |> Seq.skip 1 |> Seq.head in
        let a3 = items |> Seq.skip 1 |> Seq.head in
        let a4 = items |> Seq.skip 1 |> Seq.head in
        (a1,a2,a3,a4)
    let Unwrap5<'T> (items : seq<'T>) : ('T*'T*'T*'T*'T) =
        let a1 = Seq.head items in
        let a2 = items |> Seq.skip 1 |> Seq.head in
        let a3 = items |> Seq.skip 1 |> Seq.head in
        let a4 = items |> Seq.skip 1 |> Seq.head in
        let a5 = items |> Seq.skip 1 |> Seq.head in
        (a1,a2,a3,a4,a5)
    let Unwrap6<'T> (items : seq<'T>) : ('T*'T*'T*'T*'T*'T) =
        let a1 = Seq.head items in
        let a2 = items |> Seq.skip 1 |> Seq.head in
        let a3 = items |> Seq.skip 1 |> Seq.head in
        let a4 = items |> Seq.skip 1 |> Seq.head in
        let a5 = items |> Seq.skip 1 |> Seq.head in
        let a6 = items |> Seq.skip 1 |> Seq.head in
        (a1,a2,a3,a4,a5,a6)
    let Unwrap7<'T> (items : seq<'T>) : ('T*'T*'T*'T*'T*'T*'T) =
        let a1 = Seq.head items in
        let a2 = items |> Seq.skip 1 |> Seq.head in
        let a3 = items |> Seq.skip 1 |> Seq.head in
        let a4 = items |> Seq.skip 1 |> Seq.head in
        let a5 = items |> Seq.skip 1 |> Seq.head in
        let a6 = items |> Seq.skip 1 |> Seq.head in
        let a7 = items |> Seq.skip 1 |> Seq.head in
        (a1,a2,a3,a4,a5,a6,a7)
    let Unwrap8<'T> (items : seq<'T>) : ('T*'T*'T*'T*'T*'T*'T*'T) =
        let a1 = Seq.head items in
        let a2 = items |> Seq.skip 1 |> Seq.head in
        let a3 = items |> Seq.skip 1 |> Seq.head in
        let a4 = items |> Seq.skip 1 |> Seq.head in
        let a5 = items |> Seq.skip 1 |> Seq.head in
        let a6 = items |> Seq.skip 1 |> Seq.head in
        let a7 = items |> Seq.skip 1 |> Seq.head in
        let a8 = items |> Seq.skip 1 |> Seq.head in
        (a1,a2,a3,a4,a5,a6,a7,a8)
    let Unwrap9<'T> (items : seq<'T>) : ('T*'T*'T*'T*'T*'T*'T*'T*'T) =
        let a1 = Seq.head items in
        let a2 = items |> Seq.skip 1 |> Seq.head in
        let a3 = items |> Seq.skip 1 |> Seq.head in
        let a4 = items |> Seq.skip 1 |> Seq.head in
        let a5 = items |> Seq.skip 1 |> Seq.head in
        let a6 = items |> Seq.skip 1 |> Seq.head in
        let a7 = items |> Seq.skip 1 |> Seq.head in
        let a8 = items |> Seq.skip 1 |> Seq.head in
        let a9 = items |> Seq.skip 1 |> Seq.head in
        (a1,a2,a3,a4,a5,a6,a7,a8,a9)
    let Unwrap10<'T> (items : seq<'T>) : ('T*'T*'T*'T*'T*'T*'T*'T*'T*'T) =
        let a1 = Seq.head items in
        let a2 = items |> Seq.skip 1 |> Seq.head in
        let a3 = items |> Seq.skip 1 |> Seq.head in
        let a4 = items |> Seq.skip 1 |> Seq.head in
        let a5 = items |> Seq.skip 1 |> Seq.head in
        let a6 = items |> Seq.skip 1 |> Seq.head in
        let a7 = items |> Seq.skip 1 |> Seq.head in
        let a8 = items |> Seq.skip 1 |> Seq.head in
        let a9 = items |> Seq.skip 1 |> Seq.head in
        let a10 = items |> Seq.skip 1 |> Seq.head in
        (a1,a2,a3,a4,a5,a6,a7,a8,a9,a10)

type internal FunctionWrap =
    | Comb of seq<IVariable> * ITerm
    | FuncN of (seq<ITerm> -> ITerm)
    | Func1 of (ITerm -> ITerm)
    | Func2 of (ITerm -> ITerm -> ITerm) 
    | Func3 of (ITerm -> ITerm -> ITerm -> ITerm) 
    | Func4 of (ITerm -> ITerm -> ITerm -> ITerm -> ITerm) 
    | Func5 of (ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) 
    | Func6 of (ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) 
    | Func7 of (ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) 
    | Func8 of (ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) 
    | Func9 of (ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) 
and
    Combinator private (internalObject : FunctionWrap, arity : int, check : seq<ITerm> -> bool) = class
        static member FromEquation (args : seq<IVariable>) (result : ITerm) =
            null

        static member From (xC : seq<ITerm> -> ITerm, arity : int) =
            Combinator(FuncN xC, arity, fun args -> Seq.length args >= arity)
        static member From (xC : seq<ITerm> -> ITerm, check : seq<ITerm> -> bool, arity : int) =
            Combinator(FuncN xC, arity, check)
    
        static member From (xC : ITerm -> ITerm) =
            Combinator(Func1 xC, 1, fun args -> Seq.length args = 1)
        static member From (xC : ITerm -> ITerm, check : seq<ITerm> -> bool) =
            Combinator(Func1 xC, 1, check)
        static member From (xC : ITerm -> ITerm, check : ITerm -> bool) =
            Combinator(Func1 xC, 1, fun args -> (Seq.length args >= 1 && check (Seq.head args)))
    
        static member From (xC : ITerm -> ITerm -> ITerm) =
            Combinator(Func2 xC, 2, fun (args : seq<ITerm>) -> Seq.length args >= 2)
        static member From (xC : ITerm -> ITerm -> ITerm, check : seq<ITerm> -> bool) =
            Combinator(Func2 xC, 2, check)
        static member From (xC : ITerm -> ITerm -> ITerm, check : ITerm -> ITerm -> bool) =
            Combinator(Func2 xC, 2, 
                            fun (args : seq<ITerm>) -> 
                                let (a1, a2) = Functions.Unwrap2 args in 
                                check a1 a2)
    
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm) =
            Combinator(Func3 xC, 3, fun (args : seq<ITerm>) -> Seq.length args >= 3)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm, check : seq<ITerm> -> bool) =
            Combinator(Func3 xC, 3, check)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm, check : ITerm -> ITerm -> ITerm -> bool) =
            Combinator(Func3 xC, 2, 
                            fun (args : seq<ITerm>) -> 
                                let (a1, a2, a3) = Functions.Unwrap3 args in 
                                check a1 a2 a3)
    
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm) =
            Combinator(Func4 xC, 4, fun (args : seq<ITerm>) -> Seq.length args >= 4)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : seq<ITerm> -> bool) =
            Combinator(Func4 xC, 4, check)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : ITerm -> ITerm -> ITerm -> ITerm ->bool) =
            Combinator(Func4 xC, 4, 
                            fun (args : seq<ITerm>) -> 
                                let (a1, a2, a3, a4) = Functions.Unwrap4 args in 
                                check a1 a2 a3 a4)
    
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) =
            Combinator(Func5 xC, 5, fun (args : seq<ITerm>) -> Seq.length args >= 5)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : seq<ITerm> -> bool) =
            Combinator(Func5 xC, 5, check)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> bool) =
            Combinator(Func5 xC, 5, 
                        fun (args : seq<ITerm>) -> 
                            let (a1, a2, a3, a4, a5) = Functions.Unwrap5 args in 
                            check a1 a2 a3 a4 a5)
    
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) =
            Combinator(Func6 xC, 6, fun (args : seq<ITerm>) -> Seq.length args >= 6)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : seq<ITerm> ->bool) =
            Combinator(Func6 xC, 6, check)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> bool) =
            Combinator(Func6 xC, 6, 
                        fun (args : seq<ITerm>) -> 
                            let (a1, a2, a3, a4, a5, a6) = Functions.Unwrap6 args in 
                            check a1 a2 a3 a4 a5 a6)
    
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) =
            Combinator(Func7 xC, 7, fun (args : seq<ITerm>) -> Seq.length args >= 7)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : seq<ITerm> -> bool) =
            Combinator(Func7 xC, 7, check)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> bool) =
            Combinator(Func7 xC, 7, 
                        fun (args : seq<ITerm>) -> 
                            let (a1, a2, a3, a4, a5, a6, a7) = Functions.Unwrap7 args in 
                            check a1 a2 a3 a4 a5 a6 a7)
    
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) =
            Combinator(Func8 xC, 8, fun (args : seq<ITerm>) -> Seq.length args >= 8)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : seq<ITerm> -> bool) =
            Combinator(Func8 xC, 8, check)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> bool) =
            Combinator(Func8 xC, 8, 
                        fun (args : seq<ITerm>) -> 
                            let (a1, a2, a3, a4, a5, a6, a7, a8) = Functions.Unwrap8 args in 
                            check a1 a2 a3 a4 a5 a6 a7 a8)
    
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm) =
            Combinator(Func9 xC, 9, fun (args : seq<ITerm>) -> Seq.length args >= 9)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : seq<ITerm> -> bool) =
            Combinator(Func9 xC, 9, check)
        static member From (xC : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm, check : ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> ITerm -> bool) =
            Combinator(Func9 xC, 9, 
                        fun (args : seq<ITerm>) -> 
                            let (a1, a2, a3, a4, a5, a6, a7, a8, a9) = Functions.Unwrap9 args in 
                            check a1 a2 a3 a4 a5 a6 a7 a8 a9)
    
        interface IAtomic with
            member this.Arity  
                with get () : int = 
                    arity
            member this.IsReducibleWith (args : seq<ITerm>) : bool = 
                check args

            member this.ReduceWithArguments (args : seq<ITerm>, app : IApplicationTff) : ITerm = 
                let hasTrainingArgs = Seq.length args > arity in
                let actualArgs = Seq.take arity args in
                let trainingArgs = Seq.skip arity args in
                let result = 
                    match internalObject with 
                    | Comb (variables,result) -> raise (new Exception())
                    | FuncN f -> raise (new Exception())
                    | Func1 f -> 
                        actualArgs |> Seq.head |> f 
                    | Func2 f ->
                        actualArgs
                        |> Functions.Unwrap2
                        |> ApplicationOperations.Uncurry2 f
                    | Func3 f ->
                        actualArgs
                        |> Functions.Unwrap3
                        |> ApplicationOperations.Uncurry3 f
                    | Func4 f ->
                        actualArgs
                        |> Functions.Unwrap4
                        |> ApplicationOperations.Uncurry4 f
                    | Func5 f ->
                        actualArgs
                        |> Functions.Unwrap5
                        |> ApplicationOperations.Uncurry5 f
                    | Func6 f ->
                        actualArgs
                        |> Functions.Unwrap6
                        |> ApplicationOperations.Uncurry6 f
                    | Func7 f ->
                        actualArgs
                        |> Functions.Unwrap7
                        |> ApplicationOperations.Uncurry7 f
                    | Func8 f ->
                        actualArgs
                        |> Functions.Unwrap8
                        |> ApplicationOperations.Uncurry8 f
                    | Func9 f ->
                        actualArgs
                        |> Functions.Unwrap9
                        |> ApplicationOperations.Uncurry9 f
                in
                let application = Seq.concat [|Seq.ofArray[|result|];trainingArgs|] |> app.CreateTerm in
                application :> ITerm

end
module StandardCombinators = 
    let I = Combinator.From (fun x -> x)
    //let S = Combinator.From (fun x y z -> x z (y z))

type CombinatorI =
    |I 
    interface IFunction with
        member this.Arity
            with get () : int = 1

        member this.IsReducibleWith (termList : seq<ITerm>) = 
            Seq.length termList = (this :> IFunction).Arity

        member this.ReduceWithArguments(termList : seq<ITerm>, applicationTff) =
            let arity = (this :> IFunction).Arity
            let listLength = Seq.length termList in

            if arity > listLength then 
                raise (new Exception("Not enougn arguments"))
            else if arity = listLength then
                Seq.head termList
            else 
                raise (new Exception("Too much arguments"))
                

type CombinatorK =
    | K 
    interface IFunction with
        member this.Arity
            with get () : int = 2

        member this.IsReducibleWith (termList : seq<ITerm>) = 
            Seq.length termList = (this :> IFunction).Arity

        member this.ReduceWithArguments(termList : seq<ITerm>, applicationTff) =
            let arity = (this :> IFunction).Arity
            let listLength = Seq.length termList in

            if arity > listLength then 
                raise (new Exception("Not enougn arguments"))
            else if arity = listLength then
                Seq.head termList
            else 
                raise (new Exception("Too much arguments"))

type CombinatorS =
    | S

    interface IFunction with
        member this.Arity
            with get () : int = 3

        member this.IsReducibleWith (termList : seq<ITerm>) = 
            Seq.length termList = (this :> IFunction).Arity

        member this.ReduceWithArguments(termList : seq<ITerm>, applicationTff) =
            let arity = (this :> IFunction).Arity
            let listLength = Seq.length termList in

            if arity > listLength then 
                raise (new Exception("Not enougn arguments"))
            else if arity = listLength then
                let fst = Seq.nth 0 termList
                let trd = Seq.nth 2 termList
                let fstApplcation = Seq.singleton( applicationTff.CreateTerm( Seq.append (Seq.singleton fst) (Seq.singleton trd) ) :> ITerm )
                let sndApplication = Seq.singleton( applicationTff.CreateTerm( Seq.skip 1 termList ) :> ITerm ) in

                applicationTff.CreateTerm( Seq.append fstApplcation sndApplication ) :> ITerm
            else 
                raise (new Exception("Too much arguments"))

type CombinatorB =
    | B

    interface IFunction with
        member this.Arity
            with get () : int = 3

        member this.IsReducibleWith (termList : seq<ITerm>) = 
            Seq.length termList = (this :> IFunction).Arity

        member this.ReduceWithArguments(termList : seq<ITerm>, applicationTff) =
            let arity = (this :> IFunction).Arity
            let listLength = Seq.length termList in

            if arity > listLength then 
                raise (new Exception("Not enougn arguments"))
            else if arity = listLength then
                let fst = Seq.singleton( Seq.nth 0 termList )
                let sndApplication = Seq.singleton( applicationTff.CreateTerm( Seq.skip 1 termList ) :> ITerm ) in

                applicationTff.CreateTerm( Seq.append fst sndApplication ) :> ITerm
            else 
                raise (new Exception("Too much arguments"))

type CombinatorC =
    | C

    interface IFunction with
        member this.Arity
            with get () : int = 3

        member this.IsReducibleWith (termList : seq<ITerm>) = 
            Seq.length termList = (this :> IFunction).Arity

        member this.ReduceWithArguments(termList : seq<ITerm>, applicationTff) =
            let arity = (this :> IFunction).Arity
            let listLength = Seq.length termList in

            if arity > listLength then 
                raise (new Exception("Not enougn arguments"))
            else if arity = listLength then
                let fstApplication = Seq.singleton( applicationTff.CreateTerm( Seq.take 2 termList ) :> ITerm )
                let trd = Seq.singleton( Seq.nth 2 termList ) in

                applicationTff.CreateTerm( Seq.append fstApplication trd ) :> ITerm
            else 
                raise (new Exception("Too much arguments"))