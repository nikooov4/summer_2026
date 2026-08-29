namespace Mephi.Cybernetics.Race.Core.Host.Computing

open System
open Mephi.Cybernetics.Race.Core.Host.Definitions

//type SingleApplicationOperation = 
//    | SingleApplicationOperation of ITerm * ITerm
//    static member Create (func : ITerm, arg : ITerm) : ITerm = 
//        SingleApplicationOperation (func,arg) :> ITerm
//    static member Create (func : ITerm, args : seq<ITerm>) : ITerm = 
//        if Seq.length args < 1 then
//            raise (new Exception ("Multiple application requires at least one argument"))
//        else
//            args
//            |> Seq.fold (fun term argument -> SingleApplicationOperation.Create (term, argument)) func 
//        
//    member x.Function
//        with get () : ITerm = 
//            match x with SingleApplicationOperation (func, _) -> func
//    member x.Argument
//        with get () : ITerm = 
//            match x with SingleApplicationOperation (_, argument) -> argument
//    interface ITerm with
//        member x.Arity 
//            with get () : int = 
//                if x.Function.Arity > 0 then
//                    x.Function.Arity - 1
//                else 
//                    0
//        member x.Substitute (variable : IVariable, term : ITerm) : ITerm =
//            SingleApplicationOperation (x.Function.Substitute (variable,term), x.Argument.Substitute (variable,term)) :> ITerm
//        member x.VariableOccurs (variable : IVariable) : bool =
//            x.Function.VariableOccurs variable || x.Argument.VariableOccurs variable
//    end
//
//
//type MultiApplicationOperation = 
//    | MultiApplicationOperation of seq<ITerm>
//    static member Create (func : ITerm, argument : ITerm) : ITerm =
//        MultiApplicationOperation [|func;argument|] :> ITerm
//    static member Create (func : ITerm, args : seq<ITerm>) : ITerm =
//        if Seq.length args < 1 then
//            raise (new Exception ("Multi-application requires at least one argument"))
//        else
//            (args
//             |> Seq.append [|func|]
//             |> MultiApplicationOperation 
//            ) :> ITerm
//    member x.Function
//        with get () : ITerm =
//            match x with MultiApplicationOperation l -> Seq.head l
//    member x.Arguments
//        with get () : seq<ITerm> =
//            match x with MultiApplicationOperation l -> Seq.skip 1 l
//    interface ITerm with
//        member x.Arity 
//            with get () : int = 
//                let length = Seq.length x.Arguments
//                if x.Function.Arity >= length then
//                    x.Function.Arity - length
//                else 
//                    0
//        member x.Substitute (variable : IVariable, term : ITerm) : ITerm =
//            (let (MultiApplicationOperation l) = x in
//             Seq.map (fun (t : ITerm) -> t.Substitute (variable,term)) l
//             |> MultiApplicationOperation
//            ) :> ITerm
//        member x.VariableOccurs (variable : IVariable) : bool =
//            let (MultiApplicationOperation l) = x in
//            Seq.exists (fun (term : ITerm) -> term.VariableOccurs variable) l
//        
//        
//type ApplicationOperationsProvider = 
//    SingleApplication
//    | MultiApplication
////    | TypedSingleApplication
////    | TypedMultiApplication
//
//    interface IApplicationOperation with
//        member x.Apply (func : ITerm, args : seq<ITerm>) : ITerm =
//            match x with
//                SingleApplication -> SingleApplicationOperation.Create (func, args)
//                | MultiApplication -> MultiApplicationOperation.Create (func, args)
//        member x.Apply (func : ITerm, arg : ITerm) : ITerm =
//            match x with
//                SingleApplication -> SingleApplicationOperation.Create (func, arg)
//                | MultiApplication -> MultiApplicationOperation.Create (func, arg)


type 
    SingleApplicationTerm( leftTerm : ITerm, rightTerm : ITerm ) =

        static member Create(func, arg) =
                SingleApplicationTerm(func, arg)

        static member Create(func, args) =
            if Seq.length args >= 1 then
                let firstApplication = SingleApplicationTerm(func, Seq.head args) in
                Seq.fold (fun application term -> SingleApplicationTerm( application, term )) firstApplication (Seq.skip 1 args)
            else
                failwith "Expected at least one argument to apply to function term"

        static member Create(terms) =
            if Seq.length terms >= 2 then
                SingleApplicationTerm.Create(Seq.head terms, Seq.skip 1 terms)
            else
                failwith "Expected at least two terms to create application"

        static member Create(func, args) =
            MultiApplicationTerm( Seq.append (Seq.singleton func) args )

        member this.Apply(applyingTerms) =
            Seq.fold (fun application term -> SingleApplicationTerm( application, term )) this applyingTerms
    
        override this.ToString() =
            "(" + leftTerm.ToString() + " " + rightTerm.ToString() + ")"

        interface IApplicationTerm with
            member this.Arguments =
                match leftTerm with
                    | :? IApplicationTerm ->
                        Seq.append (leftTerm :?> IApplicationTerm).Arguments (Seq.singleton rightTerm)
                    | _ -> Seq.singleton rightTerm

            member this.Function =
                match leftTerm with
                    | :? IApplicationTerm ->
                        (leftTerm :?> IApplicationTerm).Function
                    | _ -> leftTerm

            member this.ToSeq =
                match leftTerm with
                    | :? IApplicationTerm ->
                        Seq.append (leftTerm :?> IApplicationTerm).Arguments (Seq.singleton rightTerm)
                    | _ -> Seq.append (Seq.singleton leftTerm) (Seq.singleton rightTerm)

            member this.LeftTerm
                with get () : ITerm = 
                    leftTerm

            member this.RightTerm
                with get () : ITerm = 
                    rightTerm

        interface ICompositeTerm with
            member this.GetBoundVariables =
                Seq.empty

            member this.GetTff
                with get () : ITff =
                    SingleApplicationTff :> ITff

            member this.SubTerms =
                Seq.append (Seq.singleton leftTerm) (Seq.singleton rightTerm)

and 
    MultiApplicationTerm( terms : seq<ITerm> ) =
        do
            if Seq.length terms < 2 then
                failwith "Expected at least two terms to create application"

        static member Create(terms) =
            MultiApplicationTerm(terms)

        static member Create(func, args) =
            MultiApplicationTerm( Seq.append (Seq.singleton func) args )

        static member Create(func, arg) =
            MultiApplicationTerm(seq [func; arg])

        member this.Apply(applyingTerms) =
            MultiApplicationTerm( Seq.append terms applyingTerms)

        override this.ToString() =
            Seq.reduce (fun acc el -> "(" + acc + " " + el + ")") (Seq.map (fun el -> el.ToString()) terms)

        interface IApplicationTerm with
            member this.Arguments =
                Seq.skip 1 terms

            member this.Function =
                Seq.head terms

            member this.ToSeq =
                terms

            member this.LeftTerm
                with get () : ITerm = 
                    if Seq.length terms = 2 then
                        Seq.head terms
                    else
                        MultiApplicationTerm( Seq.take (Seq.length terms - 1) terms ) :> ITerm
                
            member this.RightTerm
                with get () : ITerm = 
                    Seq.last terms

        interface ICompositeTerm with
            member this.GetBoundVariables =
                Seq.empty

            member this.GetTff
                with get () : ITff =
                    MultiApplicationTff :> ITff

            member this.SubTerms =
                terms
        
and 
    SingleApplicationTff =
        | SingleApplicationTff

        interface IApplicationTff with
            member this.CreateTerm( terms : seq<ITerm> ) =
                SingleApplicationTerm.Create(terms) :> IApplicationTerm

            member this.CreateTerm( terms : seq<ITerm> ) =
                SingleApplicationTerm.Create(terms) :> ICompositeTerm

and
    MultiApplicationTff =
        | MultiApplicationTff

        interface IApplicationTff with
            member this.CreateTerm( terms : seq<ITerm> ) =
                MultiApplicationTerm.Create(terms) :> IApplicationTerm

            member this.CreateTerm( terms : seq<ITerm> ) =
                MultiApplicationTerm.Create(terms) :> ICompositeTerm

type ApplicationOperations = 
    SingleApplication
    | MultiApplication
//    | TypedSingleApplication
//    | TypedMultiApplication

    //interface IApplicationOperation with
    member x.Apply (func : ITerm, args : seq<ITerm>) : IApplicationTerm =
        match x with
            SingleApplication -> (SingleApplicationTff.SingleApplicationTff :> IApplicationTff).CreateTerm (Seq.concat [|Seq.singleton func; args|])
            | MultiApplication -> (MultiApplicationTff.MultiApplicationTff :> IApplicationTff).CreateTerm (Seq.concat [|Seq.singleton func; args|])
    member x.Apply (func : ITerm, arg : ITerm) : IApplicationTerm =
        match x with
            SingleApplication -> (SingleApplicationTff.SingleApplicationTff :> IApplicationTff).CreateTerm [|func; arg|]
            | MultiApplication -> (MultiApplicationTff.MultiApplicationTff :> IApplicationTff).CreateTerm  [|func; arg|]

    static member Uncurry2 f (a1,a2) = f a1 a2
    static member Uncurry3 f (a1,a2,a3) = f a1 a2 a3
    static member Uncurry4 f (a1,a2,a3,a4) = f a1 a2 a3 a4
    static member Uncurry5 f (a1,a2,a3,a4,a5) = f a1 a2 a3 a4 a5
    static member Uncurry6 f (a1,a2,a3,a4,a5,a6) = f a1 a2 a3 a4 a5 a6
    static member Uncurry7 f (a1,a2,a3,a4,a5,a6,a7) = f a1 a2 a3 a4 a5 a6 a7
    static member Uncurry8 f (a1,a2,a3,a4,a5,a6,a7,a8) = f a1 a2 a3 a4 a5 a6 a7 a8
    static member Uncurry9 f (a1,a2,a3,a4,a5,a6,a7,a8,a9) = f a1 a2 a3 a4 a5 a6 a7 a8 a9

    static member Curry2 f a1 a2 = f (a1,a2)
    static member Curry3 f a1 a2 a3 = f (a1,a2,a3)
    static member Curry4 f a1 a2 a3 a4 = f (a1,a2,a3,a4)
    static member Curry5 f a1 a2 a3 a4 a5 = f (a1,a2,a3,a4,a5)
    static member Curry6 f a1 a2 a3 a4 a5 a6 = f (a1,a2,a3,a4,a5,a6)
    static member Curry7 f a1 a2 a3 a4 a5 a6 a7 = f (a1,a2,a3,a4,a5,a6,a7)
    static member Curry8 f a1 a2 a3 a4 a5 a6 a7 a8 = f (a1,a2,a3,a4,a5,a6,a7,a8)
    static member Curry9 f a1 a2 a3 a4 a5 a6 a7 a8 a9 = f (a1,a2,a3,a4,a5,a6,a7,a8,a9)
