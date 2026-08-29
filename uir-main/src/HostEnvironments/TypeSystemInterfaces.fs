namespace Mephi.Cybernetics.Race.Core.Host.Definitions

type 
    ITypeSystem = interface
        //abstract IsValidTerm : ITerm -> bool
        abstract GetTermType : ITerm * ?context:ITypeContext -> IType
        //possible rename
        abstract IsInstance : ITerm * ITerm -> bool
        //abstract SetVariableType : IVariable * seq<IType> * ITypeContext -> unit
    end

and
    IHasSubtypeInfo = interface
        abstract GetSubtypeInfo : ISubtypeInfo
    end

and 
    ITypedReduction = interface
    inherit IReduction
        abstract IsTermValid : ITerm -> bool
    end

and 
    IComplexType = interface
    inherit ICompositeTerm
    inherit IType
        abstract Name : string
    end

and 
    IDependentType = interface
    inherit IFunction
    inherit IComplexType
        abstract GetKind : IKind
    end

and 
    IBasicType = interface
    inherit IType
        abstract GetName : string
    end

and 
    IType = interface
    inherit ITerm
    end

and 
    IFunctional = interface
    inherit ICompositeTerm
        //abstract Function : ITerm
        //sabstract Argument : ITerm
    end

and IKind = interface
    inherit IType
    end

and
    IFunctionalKind = interface
    inherit IKind
    inherit IFunctional
    end

and
    ITypeContext = interface
        abstract Items : seq<ITerm>
        abstract IsContain : ITerm -> bool
        abstract GetTermType : ITerm -> IType
        abstract AddTypeBinding : (ITerm * IType) -> unit
        abstract Copy : ITypeContext
    end

and
    IContextProvider = interface
        abstract GetTypeContext : ITypeContext
    end

and
    IFunctionalType = interface
    inherit IType
    inherit IFunctional
    end

and
    IFunctionalTypeTff = interface
    inherit ITff
        abstract CreateTerm : seq<ITerm> -> IFunctionalType
    end

and
    ITypeVariable = interface
        inherit IType
        abstract GetName : string
    end

and
    ITypeVariableProvider = interface
        abstract CreateVariable : ITypeVariable
    end

and 
    IStar = interface
    inherit IKind
    end

and
    ITyped = interface
        abstract GetTermType : ITypeSystem * ITypeContext -> IType
    end

and 
    ISubtypeInfo = interface
//        abstract Parents : IList<IDomain>
//        abstract Supertypes : IList<IDomain>
        abstract IsSubtypeOf : IType -> bool
    end
