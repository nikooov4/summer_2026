namespace Mephi.Cybernetics.Race.Core.Host.Definitions

open System

type
    /// Repesents reduction strategy
    IReduction = interface
        /// Gets if the term contains any redexes (at least one)
        abstract HasRedex : ITerm -> bool
        /// If the term contains a redex, produces a contracted term; otherwise, returns the same term
        abstract Reduce : ITerm -> ITerm
        /// Reduces the term to its normal form; may end up in an infinite loop
        abstract Evaluate : ITerm -> ITerm
        /// 
        abstract EvaluateWithArguments : ITerm * seq<ITerm> -> ITerm
        /// Checks if a term obtained by application of the current term the specified arguments has redexes
        abstract IsReducibleWith : ITerm * seq<ITerm> -> bool
        /// If the current term is reducible with current arguments (see IsReducibleWith), <produces the application 
        /// of the current term to the specified arguments and then makes one reduction step>
        abstract ReduceWithArguments : ITerm * seq<ITerm> -> ITerm
        /// Get term arity
        abstract GetArity : ITerm -> int
end

and ReductionStrategy =
    | Normal
    | Applicative

and 
    Arity =
    | ExactArity of int
    | MoreOrEqualArity of int
    member this.TryToGetExactValue =
        match this with
            | ExactArity(value) -> value
            | _ -> raise (new Exception("Can't get exact value"))
    member this.GetValue =
        match this with
            | ExactArity(value) -> value
            | MoreOrEqualArity(value) -> value
    member this.IsExactValue =
        match this with
            | ExactArity(value) -> true
            | _ -> false

and 
    /// Represents a function that is either an atom or is obtained via Tff
    IFunction = interface
    inherit ITerm
        /// Gets the arity if the term
        abstract Arity : int
         /// Checks if a term obtained by application of the current term the specified arguments has redexes
        abstract IsReducibleWith : seq<ITerm> -> bool 
        /// If the current term is reducible with current arguments (see IsReducibleWith), <produces the application 
        /// of the current term to the specified arguments and then makes one reduction step>
        abstract ReduceWithArguments : seq<ITerm> * IApplicationTff -> ITerm
end

and IHasSpecificReductionStrategy = interface
    abstract GetReductionStrategy : ReductionStrategy
end

and 
    /// Represents a variable
    //TODO: Add variable range 
    IVariable = interface
        inherit ITerm
        inherit IEquatable<Object>
        abstract Name : string
end

//and
//    IRangedVariable = interface
//        inherit IVariable           //                      ++++++++++++
//        abstract Range : Domain
//end
//
//and Domain =
//        //| ObjectType of IRaceType
//        | Kind of Kind
//
//
//and Kind =
//        | Star
//        | Square


and 
    /// Provides a variannt of the application operation
    IApplicationTerm = interface
    inherit ITerm
    inherit ICompositeTerm
        // Posible renaming
        abstract LeftTerm : ITerm
        abstract RightTerm : ITerm
        abstract Function : ITerm
        abstract Arguments : seq<ITerm>
        abstract ToSeq : seq<ITerm>
end
 
and 
    /// Represents a term of the ({},{}) environment
    ITerm = interface
//        /// Gets the arity if the term
//        abstract Arity : int
//        /// Performs the substitution of the given term instead the specified variable into the current term
//        abstract Substitute : ITerm * ITerm -> ITerm
//        /// Checks if the specified variable has either free or bound occurences in the current term
//        abstract VariableOccurs : IVariable -> bool
    end

//and 
//    /// Represetns an object's property
//    IProperty = interface
//        abstract Name : string
//        abstract Domain : ITerm
//        abstract Range : ITerm
//        abstract GetValue : unit -> ITerm
//    end

and 
    /// Represent a primary TFF, that has not been derived from any environment
    // rename to composite?
    ICompositeTerm = interface // primary TFFs
    inherit ITerm 
        /// Gets a list of variables that the current TFF binds
        //abstract BindsVariables : seq<IVariable>
        /// Subterms which the current TFF combines into the new term
        abstract SubTerms : seq<ITerm>
        abstract GetTff : ITff
        abstract GetBoundVariables : seq<IVariable>
end

and
    ISubstitution = interface
        abstract Substitute : ITerm * seq<IVariable * ITerm> -> ITerm
    end

and
    ITff = interface
        abstract CreateTerm : seq<ITerm> -> ICompositeTerm
    end

and
    IApplicationTff = interface
    inherit ITff
        abstract CreateTerm : seq<ITerm> -> IApplicationTerm
    end

and 
    ITermAlias = interface
    inherit ITerm
        abstract GetTerm : ITerm
    end
and 
    /// Represents primary objects
    IAtomic = interface
    inherit ITerm
    end
    // represents the embedded term
    //abstract InnerTerm : ITerm
    
    
    /// Checks if a term obtained by application of the current term the specified arguments has redexes
    //abstract IsReducibleWith : seq<ITerm> -> bool 
    /// If the current term is reducible with current arguments (see IsReducibleWith), <produces the application 
    /// of the current term to the specified arguments and then makes one reduction step>
    //abstract ReduceWithArguments : seq<ITerm> * IApplicationOperation -> ITerm
and
    Environment (tffs : seq<ITff>, atoms : seq<IAtomic>) = class
         member val Tffs : seq<ITff> = tffs with get
         member val Atoms : seq<IAtomic> = atoms with get

         member x.Includes (e : Environment) =
            false

         member x.IntersectWith (e : Environment) =
            new Environment(Seq.empty, Seq.empty)
        
end



//[<AbstractClass>]
//type EmptyTFF() as this = class
//    abstract Arity : int
//    abstract Substitute : IVariable * ITerm -> ITerm
//    abstract VariableOccurs : IVariable -> bool
//    abstract BindsVariables : seq<IVariable>
//    abstract SubTerms : seq<ITerm>
//    abstract IsReducibleWith : seq<ITerm> -> bool
//    abstract ReduceWithArguments : seq<ITerm> * IApplication -> ITerm
//    interface ITerm with
//        member x.Substitute (v,t) = this.Substitute (v,t)
//        member x.VariableOccurs v = this.VariableOccurs v
//        member x.Arity = this.Arity
//        //abstract Properties : seq<IProperties>
//    interface ITFF with
//        member x.BindsVariables = this.BindsVariables
//        member x.SubTerms = this.SubTerms
//        member x.IsReducibleWith args = this.IsReducibleWith args
//        member x.ReduceWithArguments (args,appl) = this.ReduceWithArguments (args,appl)
//end
//
//[<AbstractClass>]
//type EmptyAtomic() as this = class
//    abstract Arity : int
//    abstract Substitute : IVariable * ITerm -> ITerm
//    abstract VariableOccurs : IVariable -> bool
//    abstract IsReducibleWith : seq<ITerm> -> bool 
//    abstract ReduceWithArguments : seq<ITerm> * IApplication -> ITerm
//    interface ITerm with
//        member x.Substitute (v,t) = this.Substitute (v,t)
//        member x.VariableOccurs v = this.VariableOccurs v
//        member x.Arity = this.Arity
//    interface IAtomic with
//        member x.IsReducibleWith args = this.IsReducibleWith args
//        member x.ReduceWithArguments (args,appl) = this.ReduceWithArguments (args,appl)
//end



//type IKSBasis = 
//    I | K | S
//
//    interface IAtomic 
//    interface ITerm with
//        member x.Arity 
//            with get () : int = 
//                match x with
//                    I -> 1
//                    | K -> 2
//                    | S -> 3
//        member x.VariableOccurs (variable : IVariable) : bool =
//            false
//        member x.Substitute (variable : IVariable, term : ITerm) : ITerm = 
//            x :> ITerm
//        member x.IsReducibleWith (args : seq<ITerm>) : bool =
//                Seq.length args = (x :> ITerm).Arity
//        member x.ReduceWithArguments (args : seq<ITerm>, application : IApplicationOperation) : ITerm =
//            if (x :> ITerm).IsReducibleWith args then
//                match x with 
//                    I -> Seq.head args
//                    | K -> Seq.head args
//                    | S -> 
//                        let a = Seq.nth 0 args in
//                        let b = Seq.nth 1 args in
//                        let c = Seq.nth 2 args in
//                        application.Apply (a, [|b; application.Apply (b,c)|])
//            else
//                raise (new Exception ("Cannot reduce a constant when the number of arguments differs from the arity of the term"))
//    end
