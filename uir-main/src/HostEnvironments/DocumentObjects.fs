namespace Mephi.Cybernetics.Race.Objects

open System
open Mephi.Cybernetics.Race.Core.Host.Definitions
open Mephi.Cybernetics.Race.Core.Host.Computing.TypeSystem

type Document(tuple : TupleTerm) =
    do
        if tuple.GetArity <> 5 then
            failwith "Five elements were expected in the tuple"

    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            DocumentTff :> ITff

        member this.SubTerms =
            Seq.singleton (tuple :> ITerm)
    end 

and DocumentTff =
    | DocumentTff
    interface ITff with
        member this.CreateTerm terms =
            raise (new NotImplementedException())
    end

and ListTerm(subterms : seq<ITerm>) =
    do
        if Seq.length subterms = 0 then
            failwith "Subterms sequence must be not empty"
    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            ListTff :> ITff

        member this.SubTerms =
            subterms
    end            

and ListTff =
    | ListTff
    interface ITff with
        member this.CreateTerm terms =
            ListTerm(terms) :> ICompositeTerm
    end

and NamedListTerm(subterms, name:string) =
    inherit ListTerm(subterms)
    
    member this.Name 
        with get() = name

and PersonTerm(firstName:StringTerm, lastName:StringTerm, birthDate:StringTerm, passport:StringTerm) =
    member this.FirstName 
        with get() = firstName
    
    member this.LastName 
        with get() = lastName
   
    member this.BirthDate 
        with get() = birthDate
    
    member this.Passport 
        with get() = passport

    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            PersonTermTff() :> ITff

        member this.SubTerms =
            seq [firstName; lastName; birthDate; passport]
    end 

    interface ITyped with
        member this.GetTermType(typeSystem, context) =
            PersonType() :> IType

and PersonTermTff() =
    interface ITff with
        member this.CreateTerm terms =
            let fst = (Seq.nth 0 terms) :?> StringTerm
            let snd = (Seq.nth 1 terms) :?> StringTerm
            let trd = (Seq.nth 2 terms) :?> StringTerm
            let fth = (Seq.nth 3 terms) :?> StringTerm
            PersonTerm(fst, snd, trd, fth) :> ICompositeTerm
    end

and PersonType() =
    override this.ToString() =
        "PersonType"

    interface IBasicType with
        member this.GetName = "Person"
    
and Recipient(person : ITerm) =
      
    member this.GetPerson 
        with get() = person

    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            RecipientTff() :> ITff

        member this.SubTerms =
            seq [person]
    end 

    interface ITyped with
        member this.GetTermType(typeSystem, context) =
            DependentType("RecipientType", 
                           seq[PersonType()],
                           seq[person], 
                           typeSystem, 
                           context) :> IType

and RecipientTff() =
    interface ITff with
        member this.CreateTerm terms =
            let fst = (Seq.nth 1 terms) 
            
            Recipient(fst) :> ICompositeTerm
    end

and PairTerm(first:ITerm, second:ITerm) =

    member this.First
        with get() = first

    member this.Second
        with get() = second

    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            PairTff :> ITff

        member this.SubTerms =
            seq [first; second]
    end 

and PairTff =
    | PairTff
    interface ITff with
        member this.CreateTerm terms =
            if Seq.length terms = 2 then
                let fst = Seq.head terms
                let snd = Seq.last terms in
                PairTerm(fst, snd) :> ICompositeTerm
            else
                failwith "Number of parameters must be equal to 2"
    end

and TupleTerm(arity:int, elements:seq<ITerm>) =
    //TODO: убрать в тип
    member this.GetArity
        with get() = arity

    member this.nth position = 
        if position < arity && position >= 0 then 
            Seq.nth position elements
        else
            failwith "Wrong position for tuple arity"
    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            TupleTff :> ITff

        member this.SubTerms =
            elements
    end

and TupleTff =
    | TupleTff
    interface ITff with
        member this.CreateTerm terms =
            TupleTerm(Seq.length terms, terms) :> ICompositeTerm
    end


and StringTerm(stringValue:string) =
    member this.StringValue
        with get() = stringValue

    interface ITyped with
        member this.GetTermType(typeSystem, context) =  
            StringType() :> IType

    interface ITerm


and StringType() =
    override this.ToString() =
        "StringType"
    
    interface IBasicType with
        member this.GetName = 
            "StringType"

and NLSituationDescription(name:StringTerm, subterms) =
    member this.SituationName 
        with get() = name

    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            NLSituationTff :> ITff

        member this.SubTerms =
            Seq.append (seq[name]) subterms
    
    interface ITyped with
        member this.GetTermType(typeSystem, context) =
            let variables = Seq.filter (fun (el :ITerm) -> el :? IVariable) subterms
            
            Seq.map (
                        fun var -> context.AddTypeBinding(var, StringType() :> IType)) variables
                                 
            DependentType("NLSituationType", seq[StringType()], seq[name], 
                           typeSystem, 
                           context) :> IType

and NLSituationTff =
    | NLSituationTff

    interface ITff with
        member this.CreateTerm( terms : seq<ITerm> ) =
            NLSituationDescription((Seq.head terms) :?> StringTerm, Seq.skip 1 terms) :> ICompositeTerm
    
and DependentType(typeName, kindTerms : seq<IType>, passedSubterms : seq<ITerm>, typeSystem, context) =
    let mutable subterms = passedSubterms
    
    override this.ToString() =
        typeName + ":[" + FunctionalKind(kindTerms).ToString() + "]"
    interface IDependentType with
        member this.GetKind = 
            FunctionalKind(kindTerms) :> IKind
        
        member this.Name = 
            typeName

        member this.Arity = 
            Seq.length subterms

        member this.IsReducibleWith args =
            Seq.length args > 0

        member this.ReduceWithArguments(args, app) =
            if (Seq.forall2 (fun (kindTerm : IType) (subterm : ITerm) -> 
                                 (kindTerm :? Star && subterm :? IType) ||
                                 (kindTerm :? IType && (subterm :?> ITyped).GetTermType(typeSystem, context) = kindTerm))
                            (Seq.skip (Seq.length subterms) kindTerms)
                            args
                ) then
                subterms <- (Seq.append subterms args)
                this :> ITerm
            else
                failwith "Bad kind"

        member this.SubTerms =
            subterms
        member this.GetTff =
            DependentTypeTff(typeName, typeSystem, context) :> ITff
            
        member this.GetBoundVariables =  
            Seq.empty
    end  

and DependentTypeTff(typeName, typeSystem, context) = 
    interface ITff with
        member this.CreateTerm( terms : seq<ITerm> ) =
            DependentType(typeName, Seq.map (fun (el:ITerm) -> (typeSystem :> ITypeSystem).GetTermType el) terms,  terms, typeSystem, context) :> ICompositeTerm


and NamedTypedTuple(name:StringTerm, elements : seq<ITerm>) =
    let arity = Seq.length elements

    member this.GetArity
        with get() = arity

    member this.nth position = 
        if position < arity && position >= 0 then 
            Seq.nth position elements
        else
            failwith "Wrong position for tuple arity"
    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            NamedTypedTupleTff() :> ITff

        member this.SubTerms =
            Seq.append (seq[name]) elements

    interface ITyped with
        member this.GetTermType(typeSystem, context) =
            DependentType("Named tuple", 
                           Seq.append (seq[StringType()]) (Seq.map (fun (el : ITerm) -> (Star :> IType)) elements),
                           Seq.append (Seq.singleton (name :> ITerm)) elements, 
                           typeSystem, 
                           context) :> IType
     
and NamedTypedTupleTff() =

    interface ITff with
        member this.CreateTerm terms =
            NamedTypedTuple((Seq.head terms) :?> StringTerm, Seq.skip 1 terms) :> ICompositeTerm

and NamedTypedList(name:StringTerm, elements : seq<ITerm>) =
    let arity = Seq.length elements

    member this.nth position = 
        if position < arity && position >= 0 then 
            Seq.nth position elements
        else
            failwith "Wrong position for tuple arity"
    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.empty

        member this.GetTff =
            NamedTypedListTff() :> ITff

        member this.SubTerms =
            Seq.append (seq[name]) elements

    interface ITyped with
        member this.GetTermType(typeSystem, context) =
            DependentType("Named list", 
                           Seq.append (seq[StringType()]) (Seq.map (fun (el : ITerm) -> (Star :> IType)) elements),
                           Seq.append (Seq.singleton (name :> ITerm)) elements, 
                           typeSystem, 
                           context) :> IType
     
and NamedTypedListTff() =

    interface ITff with
        member this.CreateTerm terms =
            NamedTypedTuple((Seq.head terms) :?> StringTerm, Seq.skip 1 terms) :> ICompositeTerm