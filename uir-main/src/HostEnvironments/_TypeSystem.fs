namespace Mephi.Cybernetics.Race.Core.Host.Definitions.TypeSystem

     

open System.Collections.Generic;

/// Version No 2


// In Lambda cube: *
type 
    IDomain = interface
    
        abstract Name : string
        abstract Namespace : string
        abstract Source : string
        abstract FullName : string
        abstract FullyQualifiedName : string
    end

and IConstructor<'T> = interface
    abstract Name : string
    abstract Arity : int
    end

///  In Lambda cube: 'alpha -> 'alpha, where 'alpha in *
and IDomainConstructor = interface
    inherit IConstructor<IDomain>
    //abstract Name : string
    //abstract Arity : int
    end

and ISubtypeInfo = interface
    abstract Parents : IList<IDomain>
    abstract Supertypes : IList<IDomain>
    abstract IsSubtypeOf : IDomain -> bool
    end

//
and IStructureInfo<'T, 'T2 when 'T2 :> IConstructor<'T>> = interface
    //Зачем это свойство, если интерфейс уже для сл. т.
    abstract IsComplex : bool
    abstract Constructor : 'T2
    abstract ConstructorArguments : IList<'T>
    end 

and IDomainStructureInfo = interface
    inherit IStructureInfo<IDomain, IDomainConstructor>
    end

and IFunctionInfo = interface
    abstract ReflectedType : IDomain
    abstract Arity : int
    abstract FormalArguments : IList<IDomain>
    abstract Returns : IList<IDomain>
    abstract Apply : IList<obj> -> obj // тут нужно сразу вычислять значение; построение объекта-аппликации -- это задача конструирования объекта, она решается другими средствами (Interfaces.fs --> ApplicationOperation)
    end

and IAttribute = interface
    abstract Name : string
    abstract Domain : IDomain
    abstract Range : IDomain
    abstract IsComputable : bool // ?
    abstract IsReadOnly : bool
    abstract GetValue : obj -> obj
    abstract GetValue<'T> : obj -> 'T
    //abstract GetValue1<'T> : obj -> IList<obj> -> 'T
    abstract  SetValue : obj -> unit
    end


// Attribute construction -- aka complex frames
//and IAttributeConstructor = interface
//    inherit IConstructor<IAttribute>
//    end
//
//and IAttributeStructureInfo = interface
//    inherit IStructureInfo<IAttribute, IAttributeConstructor>
//    end

and IAttributeCollection = interface
    inherit IEnumerable<IAttribute>
    abstract  Item : string -> IAttribute with get
    abstract  Item : int -> IAttribute with get
    end

and IAttributesInfo<'T when 'T :> IAttributeCollection> = interface
    abstract Attributes : 'T
    end


//type IRelation = interface
//    abstract Domain : IDomain
//    abstract Where : IAttribute * obj -> IRelation
//    abstract Where : IList<IAttribute * IList<obj>> -> IRelation
//    abstract Select : IDomain -> (IDomain -> IDomain) -> IRelation // projection
//    abstract Union : IRelation -> IRelation
//    abstract Intersect : IRelation -> IRelation
//    abstract Except : IRelation -> IRelation
//    end

type IDataContext = interface
    //abstract Select : IDomain -> IRelation
    //abstract Select : 
    end

type C = class

    member this.DoSmth (x : System.Func<int,int>) =
        x.Invoke 1

    member this.Call() =
        let f = fun x -> x + 1
        this.DoSmth (System.Func<int,int> f)

    end
(*
type IEntity = interface
    abstract Name : string
    abstract Attributes : IList<IEntityAttribute>
    abstract Associations : IList<IAssociation>
    end

and IRelatioship = interface
    abstract Name : string
    abstract Entities : IList<IEntity>
    abstract Arity : int
     abstract Cardinality : ... ?
    end
    *)