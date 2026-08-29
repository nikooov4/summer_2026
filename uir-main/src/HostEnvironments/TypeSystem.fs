module Mephi.Cybernetics.Race.Core.Host.Computing.TypeSystem

open System
open System.Collections.Generic
open Mephi.Cybernetics.Race.Core.Host.Definitions

type FunctionalTypeTff() = 
    interface IFunctionalTypeTff with
        member this.CreateTerm( terms : seq<ITerm> ) =
            FunctionalType( Seq.cast<IType> terms  ) :> IFunctionalType
        member this.CreateTerm( terms : seq<ITerm> ) =
            FunctionalType( Seq.cast<IType> terms ) :> ICompositeTerm

and FunctionalType( typeSequence : seq<IType> ) =
    override this.ToString() =
        String.Join(" -> ", Seq.map (fun el -> el.ToString()) typeSequence)
    interface IFunctionalType with
        member this.SubTerms = typeSequence :?> seq<ITerm>
        member this.GetTff = FunctionalTypeTff() :> ITff
        member this.GetBoundVariables = Seq.empty
    end

and Star =
    | Star
    override this.ToString() = "*"
    interface IKind

and FunctionalKindTff = 
    | FunctionalKindTff
    interface ITff with
        member this.CreateTerm( terms : seq<ITerm> ) =
            FunctionalKind(Seq.cast<IType> terms) :> ICompositeTerm

// Нужно переписать без добавления звездочки в конце
// Желательно вообще от нее избавится, и "подразумевать" ее
and FunctionalKind( types : seq<IType> ) =
    let subtypes = Seq.append types (Seq.singleton (Star :> IType))
    override this.ToString() =
        String.Join(" -> ", Seq.map (fun el -> el.ToString()) types)
    interface IFunctionalKind with
        member this.SubTerms = Seq.cast<ITerm> subtypes
        member this.GetTff = FunctionalKindTff :> ITff
        member this.GetBoundVariables = Seq.empty
    end
    
type NoRepeatTypeContext( initialBindings : seq<ITerm * IType> ) =
    let mutable  bindings = initialBindings
    interface ITypeContext with
        member this.AddTypeBinding binding = 
            bindings <- Seq.append (Seq.singleton binding) bindings 

        member this.GetTermType( term ) =
            try
                snd( Seq.find (fst >> ((=) term) ) bindings )
            with
                | :? KeyNotFoundException ->
                    raise (new Exception ("There is no such term in dom"))      
                              
        member this.IsContain( term ) =
            Seq.exists (fun (el : ITerm) -> if el :? IVariable then (el :?> IVariable).Equals term else el.Equals term) (Seq.map fst bindings)

        member this.Items = 
            Seq.map fst bindings

        member this.Copy = 
            NoRepeatTypeContext( bindings ) :> ITypeContext
    
type ContextProvider() =
    interface IContextProvider with
        member this.GetTypeContext =
            NoRepeatTypeContext(Seq.empty) :> ITypeContext

type TypeVariable(name : string) =
    override this.ToString() =
        name
    override this.Equals term = 
        match term with
            | :? ITypeVariable as var ->
                name = var.GetName
            | _ -> false
    interface ITypeVariable with
       member this.GetName = name
    
       
type TypeVariableProvider() =
    let mutable i = -1
    interface ITypeVariableProvider with
        member this.CreateVariable = 
            i <- i + 1
            TypeVariable("a" + i.ToString()) :> ITypeVariable
            
            

type TermTypeDependency =
    | TermType of ITypeVariableProvider * ISubstitution
//  | TermType
    | TypeTerm
    | TypeType

type TypeCompare = 
    | TypesDiffer
    | TypesAreEqual
    | FirstTypeIsSubset
    | FirstTypeIsParent
    | TypesHaveCommonParent

type TypeSystem ( availableDependencies : seq<TermTypeDependency>, reduction : IReduction, 
                  contextProvider : IContextProvider, functionalTypeTff : IFunctionalTypeTff ) as typeSystem =

    let typeTermDependency = Seq.exists ((=) TypeTerm) availableDependencies
    let typeTypeDependency = Seq.exists ((=) TypeType) availableDependencies
    let (typeVariableProvider, substitution) = 
        try
            let checkFunc = Seq.find   (fun item ->
                                        match item with
                                            | TermType(typeVar, substitution) -> true
                                            | _ -> false
                                        )
                                        availableDependencies
            match checkFunc with
                | TermType(typeVariableProvider, substitution) -> (Some typeVariableProvider, Some substitution)
                | _ -> failwith("This has never been raisen")
        with
            | _ -> (None, None)
            
//    let dependencies = Seq.fold (+) 0 (availableDependencies :?> seq<int>)

    let rec arrowElimination( expectedArgs : seq<ITerm>, passedArgs : seq<ITerm>, context : ITypeContext ) =
        let expectedLength = Seq.length expectedArgs - 1
        let argsLength = Seq.length passedArgs
        
        let rec isEqualOrInstance (expected : ITerm) (passed : ITerm) =
            match passed with 
                | :? IType ->
                    if expected :? Star then
                        true
                    else
                        false
                                            
                | :? IApplicationTerm as application ->
                    let func = application.Function
                    let funcArgs = application.Arguments 
                    let applicationType = getType(func, funcArgs, context) in

                    match compareTypes applicationType (expected :?> IType) with
                        | TypesAreEqual ->
                            true
                        | FirstTypeIsSubset ->
                            true
                        | FirstTypeIsParent ->
                            match func with
                                | :? IVariable ->
                                    let varType = context.GetTermType func in
                                    match varType with
                                        | :? IFunctional as functional ->
                                            let expectedAsSeq =
                                                match expected with
                                                    | :? IFunctional as expectedFunctional ->
                                                        expectedFunctional.SubTerms
                                                    | _ ->
                                                        Seq.singleton expected
                                            let newVarType = functionalTypeTff.CreateTerm( Seq.append (Seq.take (Seq.length funcArgs) functional.SubTerms) expectedAsSeq) in
                                            context.AddTypeBinding( func, newVarType :> IType )
                                            true
                                        | _ ->
                                            raise (new Exception("This shouldn't be ever raisen"))
                                | _ ->
                                    false
                        | _ ->
                            false

                | :? IVariable ->
                    if context.IsContain passed then
                        let varType = context.GetTermType passed in
                        match compareTypes varType (expected :?> IType) with
                            | TypesAreEqual ->
                                true
                            | FirstTypeIsSubset ->
                                true
                            | FirstTypeIsParent ->
                                let bindingType =
                                    if expected :? Star && passed :? IVariable then
                                        match typeVariableProvider with
                                            | Some provider ->
                                                provider.CreateVariable :> IType
                                            | None -> failwith "no dependency"
                                    else
                                        expected :?> IType
                                context.AddTypeBinding(passed, bindingType)
                                true
                            | _ ->
                                false
                    else
                        let bindingType = 
                            if expected :? Star && passed :? IVariable then
                                match typeVariableProvider with
                                    | Some provider ->
                                        provider.CreateVariable :> IType
                                    | None -> failwith "no dependency"
                            else
                                expected :?> IType
                        context.AddTypeBinding(passed, bindingType)
                        true

                | _ ->
                    let passedArgType = getType(passed, Seq.empty, context)
                    let compareRes = compareTypes passedArgType (expected :?> IType) in
                    compareRes = TypesAreEqual || compareRes = FirstTypeIsSubset
                     
        and compareTypes (type1 : ITerm) (type2 : ITerm) =
            let seqCompare (seq1 : seq<ITerm>) (seq2  : seq<ITerm>) =
                Seq.reduce 
                    (   fun (acc : TypeCompare) (elem : TypeCompare) ->
                            match acc with
                                | TypesDiffer ->
                                    acc

                                | TypesHaveCommonParent ->
                                    match elem with
                                        | TypesDiffer ->
                                            elem
                                        | _ ->
                                            acc

                                | FirstTypeIsSubset ->
                                    match elem with
                                        | TypesDiffer ->
                                            elem
                                        | TypesHaveCommonParent ->
                                            elem
                                        | FirstTypeIsParent ->
                                            TypesHaveCommonParent
                                        | _ ->
                                            acc
                                | FirstTypeIsParent ->
                                    match elem with
                                        | TypesDiffer ->
                                            elem
                                        | TypesHaveCommonParent ->
                                            elem
                                        | FirstTypeIsSubset ->
                                            TypesHaveCommonParent
                                        | _ ->
                                            acc
                                | _ ->
                                    elem
                    )
                    (Seq.map2 compareTypes seq1 seq2)

            if type1 :? IHasSubtypeInfo && (type1 :?> IHasSubtypeInfo).GetSubtypeInfo.IsSubtypeOf (type2 :?> IType) then
                FirstTypeIsSubset
            else if type2 :? IHasSubtypeInfo && (type2 :?> IHasSubtypeInfo).GetSubtypeInfo.IsSubtypeOf (type1 :?> IType) then
                FirstTypeIsParent
            else
                match type1 with
                    | :? ITypeVariable ->
                        match type2 with
                            //check it!
                            | :? ITypeVariable ->
                                TypesAreEqual
                            | _ ->
                                FirstTypeIsParent

                    | :? IFunctional ->
                        match type2 with
                            | :? IFunctional ->
                                let subterms1 = ((type1 :?> IFunctional).SubTerms)
                                let subterms2 = ((type2 :?> IFunctional).SubTerms)
                                let lengthOfSubterms1 = Seq.length subterms1
                                let lengthOfSubterms2 = Seq.length subterms2 in

                                if lengthOfSubterms1 = lengthOfSubterms2 then
                                    seqCompare subterms1 subterms2
                                else
                                    if  (Seq.last subterms1 :? ITypeVariable && lengthOfSubterms1 < lengthOfSubterms2) ||
                                        (Seq.last subterms2 :? ITypeVariable && lengthOfSubterms1 > lengthOfSubterms2) then

                                        let minLength = if lengthOfSubterms1 < lengthOfSubterms2 then lengthOfSubterms1 else lengthOfSubterms2
                                        let possibleCompareRes = seqCompare (Seq.take (minLength - 1) subterms1) subterms2 in

                                        match possibleCompareRes with
                                            | TypesDiffer ->
                                                possibleCompareRes
                                            | TypesHaveCommonParent ->
                                                possibleCompareRes
                                            | FirstTypeIsParent ->
                                                if lengthOfSubterms1 < lengthOfSubterms2 then
                                                    possibleCompareRes
                                                else
                                                    TypesHaveCommonParent
                                            | FirstTypeIsSubset ->
                                                if lengthOfSubterms1 > lengthOfSubterms2 then
                                                    possibleCompareRes
                                                else
                                                    TypesHaveCommonParent
                                            | _ ->
                                                if lengthOfSubterms1 > lengthOfSubterms2 then
                                                    FirstTypeIsSubset
                                                else
                                                    FirstTypeIsParent
                                    else
                                        TypesDiffer
                            
                            | :? Star ->
                                TypesAreEqual                                                
                            | _ ->
                                TypesDiffer

                    | :? IDependentType as dependent1 ->
                        match type2 with
                            | :? IDependentType as dependent2 ->
                                if  dependent1.Name = dependent2.Name         &&
                                    Seq.length dependent1.SubTerms = Seq.length dependent2.SubTerms   &&
                                    (seqCompare (dependent1.SubTerms) (dependent2.SubTerms)) = TypesAreEqual then

                                    TypesAreEqual
                                else
                                    TypesDiffer

                            | :? Star ->
                                TypesAreEqual

                            | _ ->
                                TypesDiffer

                    | :? IBasicType as basic1 ->
                        if  type2 :? IBasicType &&
                            basic1.GetName = (type2 :?> IBasicType).GetName || type2 :? Star then
                            
                            TypesAreEqual
                        else
                            TypesDiffer
                    
                    | _ ->
                        // Сравнение термов
                        if type1 = type2 || type2 :? Star then
                            TypesAreEqual
                        else
                            TypesDiffer
                        
        if expectedLength < argsLength then
            //Num -> a | Num -> Num -> Num -> b | = Num -> (Num -> Num -> b)
            let last = Seq.last expectedArgs 
            if last :? ITypeVariable then
                match typeVariableProvider with
                    | Some provider ->
                        let funcType = 
                            functionalTypeTff.CreateTerm( 
                                Seq.append 
                                    (Seq.map (fun term -> getType(term, Seq.empty, context) :> ITerm) (Seq.skip expectedLength passedArgs)) 
                                    (Seq.singleton (provider.CreateVariable :> ITerm)) 
                            )
                        if context.IsContain last then
                            let typeInContext = context.GetTermType last in
                            match compareTypes funcType typeInContext with
                                | TypesAreEqual ->
                                    if Seq.forall2 isEqualOrInstance expectedArgs passedArgs then
                                        funcType :> ITerm
                                    else
                                        raise (new Exception("Bad type"))
                                | FirstTypeIsSubset ->
                                    if Seq.forall2 isEqualOrInstance expectedArgs passedArgs then
                                        context.AddTypeBinding(last, funcType :> IType)
                                        funcType :> ITerm
                                    else
                                        raise (new Exception("Bad type"))
                                | _ ->
                                    raise (new Exception("Bad type"))
                        else
                            if Seq.forall2 isEqualOrInstance expectedArgs passedArgs then
                                context.AddTypeBinding (last, funcType :> IType)
                                funcType :> ITerm
                            else
                                raise (new Exception("Bad type"))
                    | None ->
                        raise (new Exception("Term-type dependency is not allowed"))
            else
                raise (new Exception("Bad type"))
        else
            if Seq.forall2 isEqualOrInstance expectedArgs passedArgs then
                if expectedLength > argsLength then
                   functionalTypeTff.CreateTerm( Seq.skip argsLength expectedArgs ) :> ITerm
                else
                    Seq.last expectedArgs
            else
                raise (new Exception ("Bad type"))

    and dependentOrSimpleType (termType : IType, context) =
        if  (termType :? IDependentType && isCorrectTypeKind(termType :?> IDependentType, context)) ||
            (not (termType :? IDependentType)) then
            termType :> ITerm 
        else
            raise (new Exception("Bad kind"))

    and isCorrectTypeKind (dependentType : IDependentType, context) =
        match dependentType.GetKind with
            | :? IFunctional as functional ->
                let subterms = functional.SubTerms in

                if  (typeTermDependency && typeTypeDependency)

                    ||  (   typeTermDependency && 
                            not (Seq.exists 
                                    (fun (item : ITerm) -> item :? Star) 
                                    (Seq.take (Seq.length subterms - 1) subterms))
                        )
                    ||  (   typeTypeDependency && 
                            not (Seq.exists 
                                    (fun (item : ITerm) -> not (item :? Star)) 
                                    (Seq.take (Seq.length subterms - 1) subterms))
                        )
                then
                    //seq empty?
                   // match arrowElimination(subterms, dependentType.SubTerms, contextProvider.GetTypeContext) with
                    match arrowElimination(subterms, dependentType.SubTerms, context) with
                        | :? Star -> true
                        | _ -> false
                else
                    false

            | :? Star -> 
                true 
            | _ -> 
                false

    and getType( term : ITerm, args : seq<ITerm>, context : ITypeContext ) = 
        match term with
            | :? IType ->
                let reducedType = reduction.EvaluateWithArguments( term, args )
                let arity = reduction.GetArity reducedType in
                if arity = 0 then
                    match reducedType with
                        | :? IBasicType ->
                            reducedType :?> IType
                        | :? IDependentType ->
                            let dependentType = (reducedType :?> IDependentType) in
                            match dependentType.GetKind with
                                | :? IFunctional ->
                                    raise (new NotImplementedException())
                else
                    raise (new NotImplementedException())

            | :? IApplicationTerm ->
                let application = term :?> IApplicationTerm in
                getType( application.Function, application.Arguments, context )

            | :? ITyped ->
                // Possible reduce here
                let termType = (term :?> ITyped).GetTermType( typeSystem :> ITypeSystem, context ) in
                match termType with
                    | :? IFunctional as functional ->
                        //Possible compare func type length with arity
                        arrowElimination( functional.SubTerms, args, context ) :?> IType
                    | _ ->
                        if Seq.length args = 0 then
                            (dependentOrSimpleType( termType, context)) :?> IType
                        else
                            raise (new Exception("Functional type was expected"))

            | _ ->
                if context.IsContain term then
                    let termType = context.GetTermType term in
                    match termType with
                        | :? ITypeVariable ->
                            if Seq.length args = 0 then
                                termType
                            else
                                match typeVariableProvider with
                                    | Some provider ->
                                        arrowElimination( Seq.map (fun term -> provider.CreateVariable :> ITerm) args, args, context ) :?> IType
                                    | None ->
                                        raise (new Exception("Term-type dependency is not allowed"))
                        | :? IFunctionalType as functional ->
                            arrowElimination( functional.SubTerms, args, context ) :?> IType
                        | _ ->
                            if Seq.length args = 0 then
                                (dependentOrSimpleType(termType, context)) :?> IType
                            else
                                raise (new Exception("Functional type was expected"))
                else
                    if term :? IVariable then
                        match typeVariableProvider with
                            | Some provider ->
                                let varType = functionalTypeTff.CreateTerm( Seq.take (Seq.length args + 1) (Seq.initInfinite (fun index -> provider.CreateVariable :> ITerm)) ) :> IType in
                                context.AddTypeBinding( term, varType )
                                varType
                            | None ->
                                raise (new Exception("Term-type dependency is not allowed"))
                    else
                        raise (new Exception("Can not get term type"))

    interface ITypeSystem with
        member this.GetTermType(term, passedContext) = 
            let context = 
                match passedContext with
                | Some con -> con
                |None -> contextProvider.GetTypeContext
            let ttype = getType( term, Seq.empty, context )
            ttype
        
        member this.IsInstance(term1, term2) =
            let isInstance( term1 : ITerm, term2 : ITerm ) =
                if (term2 :? Star || term2 :? ITypeVariable) && term1 :? IType then
                    true
                else
                    match term1 with
                        | :? IDependentType ->
                            let type1 = term1 :?> IDependentType in
                            if term2 :? IDependentType && type1.Name = (term2 :?> IDependentType).Name then
                                let type2 = term2 :?> IDependentType
                                let subtermsOfType1 = type1.SubTerms
                                let subtermsOfType2 = type2.SubTerms in

                                    if Seq.length subtermsOfType1 < Seq.length subtermsOfType2 then
                                        false
                                    else
                                        Seq.forall2 (fun term1 term2 -> (this :> ITypeSystem).IsInstance( term1, term2 )) subtermsOfType1 subtermsOfType2
                            else
                                false
                        | :? IBasicType ->
                            if term2 :? IBasicType then
                                (term1 :?> IBasicType).GetName = (term2 :?> IBasicType).GetName
                            else 
                                false
                        | _ ->
                            false
            let isSubtype( term1 : ITerm, term2 : ITerm ) =
                match term1 with
                    | :? IHasSubtypeInfo ->
                        match term2 with
                            | :? IType ->
                                (term1 :?> IHasSubtypeInfo).GetSubtypeInfo.IsSubtypeOf (term2 :?> IType)
                            | _ -> false
                    | _ -> 
                        false
            isInstance(term1, term2) || isSubtype(term1, term2)
        
            


            
        
    

//type SimpleTypedLambda( context : ITypeContext ) =
//    let rec getType( term : ITerm, argsTypes : seq<IType>, context : ITypeContext) =
//        match term with
//            | :? ITyped ->
//                typeSubstitution( (term :?> ITyped).GetType, argsTypes )
//            | :? IApplication ->
//                let application = (term :?> IApplication) in
//                getType( application.Function, Seq.append (Seq.map (fun term -> getType( term, Seq.empty, context )) application.Arguments) argsTypes, context )
//            | :? ITFF ->
//                if (term :?> IReducible).Arity < Seq.length argsTypes then
//                    raise (new Exception ("Application of term with 0 arity and another term can't be built"))
//                else
//                    let tff = (term :?> ITFF)
//                    getType( tff.FormedTerm, Seq.empty, Seq.fold (fun cont elem -> cont.AddTypeBinding elem) context (Seq.zip (tff.BindsVariables :?> seq<ITerm>) argsTypes)) //check if downcast succeed 
//            | :? IVariable ->
//                if context.IsContain term then
//                    typeSubstitution( context.GetTermType term, argsTypes )
//                else
//                    raise (new Exception ("Variable type wasn't assigned"))
//            | _ ->
//                raise (new Exception ("Unhandled type in match expression"))
//
//    interface ITypeSystem with
//        member this.IsValidTerm( term : ITerm ) =
//            try
//                //I think that Copy method is not the best decision
//                let termType =  getType( term, Seq.empty, context.Copy ) in
//                true
//            with
//                | _ ->
//                    false
//
//        member this.GetTermType( term ) =
//            getType( term, Seq.empty, context.Copy )
//
//        member this.AddToContext binding =
//            context.AddTypeBinding binding
//
//        member this.GetContext =
//            context
//    end
//
//type TypeVariable() = 
//    interface ITypeVariable with
//        member this.GetType = this :> IType
//        member this.IsSubtypeOf( parentType ) = false
//        member this.Equals typeTOCheck = true
//    end
//
//type SecondOrderTypedLambda( context : ITypeContext, domain : ITypeDomain ) =
//    let rec getType( term : ITerm, argsTypes : seq<IType>, context : ITypeContext ) =
//        match term with
//            | :? ITyped ->
//                typeSubstitution( (term :?> ITyped).GetType, argsTypes )
//            | :? IApplication ->
//                let application = (term :?> IApplication) in
//                getType( application.Function, Seq.append (Seq.map (fun term -> getType( term, Seq.empty, context )) application.Arguments) argsTypes, context )
//            | :? ITFF ->
//                if (term :?> IReducible).Arity < Seq.length argsTypes then
//                    raise (new Exception ("Application of term with 0 arity and another term can't be built"))
//                else
//                    let tff = (term :?> ITFF)
//                    getType( tff.FormedTerm, Seq.empty, Seq.fold (fun cont elem -> cont.AddTypeBinding elem) context (Seq.zip (tff.BindsVariables :?> seq<ITerm>) argsTypes)) //check if downcast succeed 
//            | :? IVariable ->
//                if context.IsContain term then
//                    typeSubstitution( context.GetTermType term, argsTypes )
//                else
//                    raise (new Exception ("Variable type wasn't assigned"))
//            | _ ->
//                raise (new Exception ("Unhandled type in match expression"))