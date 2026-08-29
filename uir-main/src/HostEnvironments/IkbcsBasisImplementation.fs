module IKBCS

open Mephi.Cybernetics.Race.HostEnvironments

type BasisI = 
    I
    interface ITerm with

    end

type IKBCS = 
    I | K | B | C | S

    interface ITerm with
        member x.Arity 
            with get () : int = 
                match x with
                    I -> 1
                    | K -> 2
                    | S -> 3
        member x.VariableOccurs (variable : IVariable) : bool =
            false
        member x.Substitute (variable : IVariable, term : ITerm) : ITerm = 
            x :> ITerm
    interface IAtomic with 
        member x.IsReducibleWith (args : seq<ITerm>) : bool =
                Seq.length args = (x :> ITerm).Arity
        member x.ReduceWithArguments (args : seq<ITerm>, application : IApplicationOperation) : ITerm =
            if (x :> IAtomic).IsReducibleWith args then
                match x with 
                    I -> Seq.head args
                    | K -> Seq.head args
                    | S -> 
                        let a = Seq.nth 0 args in
                        let b = Seq.nth 1 args in
                        let c = Seq.nth 2 args in
                        application.Apply (a, [|b; application.Apply (b,c)|])
            else
                raise (new Exception ("Cannot reduce a constant when the number of arguments differs from the arity of the term"))
    end