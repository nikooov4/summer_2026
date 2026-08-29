using System;
using System.Collections;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.TypeSystem
{
    //TODO: инвертировать зависимости: базовые определения перенести сюда, а HostEnvironments сделать расширением
    public interface IDomain //: IType
    {
        string Name { get; }
        string Namespace { get; }
        string Source { get; }
        string FullName { get; }
        string FullyQualifiedName { get; }
    }

    public interface IDomainConstructor : IConstructor<IDomain>
    {
        
    }

    public interface IDomainStructureInfo : IStructureInfo<IDomain, IDomainConstructor>//, IComplexType
    {
        
    }

    public interface IDomainSubtypingInfo : ISubtypingInfo<IDomain>
    {
        
    }

    public interface IDomainAttributesInfo
    {
        IAttributeCollection<IDomain, IDomainAttribute> Properties { get; }
    }

    public interface IFunctionTypeInfo : IFunctionSignature<IDomain, IArgumentInfo<IDomain>>
    {
        /// <summary>
        /// The domain, whose structure is being reflected
        /// </summary>
        IDomain ReflectedDomain { get; }
        object Apply(object function, IList<object> args);
        object Apply(object function, object[] args);
        Delegate AsDelegate { get; }
        bool IsDelegateConformant(Delegate functionDelegate);
    }
    public interface IDomainAttribute : IAttributeBase<IDomain>
    {
        
    }

    public interface IDomainAttributeCollection : IAttributeCollection<IDomain, IDomainAttribute>
    {
        
    }

    public interface ICollectionDomain : IDomain
    {
        //IDomain ReflectedDomain { get; }
        bool IsGeneric { get; }
        IDomain ElementType { get; }
        int GetDimensionCount(object collection);
        int GetElementCount(object collection);
        bool CanBeEmpty { get; }
        bool IsList { get; }
        bool IsArray { get; }
        bool IsResizable { get; }
        bool IsMutable { get; }
        object GetElement(int index);// для остальных размерностей используется 0
        object GetElement(int[] indices);
        bool TryGetElement(int index, out object element); // для остальных размерностей используется 0
        bool TryGetElement(int[] indices, out object element);
        void SetElement(int index); // для остальных размерностей используется 0
        void SetElement(int[] indices);
        IEnumerable CreateEmpty();
        IEnumerable Create(object item, int count = 1);
    }

    public interface ITupleDomain : IDomain
    {
        //IDomain ReflectedDomain { get; }
        bool IsGeneric { get; }
        IReadOnlyCollection<IDomain> ElementTypes { get; }
        int ComponentsCount { get; }
        bool IsMutable { get; }
        IDomain GetComponent(int index);
        object GetComponentValue(object tuple, int index);
        bool TryGetComponentValue(object tuple, int index, out object value);
        void SetComponentValue(object tuple, int index, object value);
        object FromValues(object[] items);
    }

    public interface IRaceRelation
    {
        IList<IRaceRelationAttribute> Attributes { get; }

        bool IsCompatibleWith(IRaceRelation relation);
        bool IsCompatibleWith(IDomain compounDomain);
        bool IsCompatibleWith(IList<IDomain> domains);
        bool IsCompatibleWith(Type compoundDomainType);
        bool IsComatibleWith(IList<Type> domainTypes);

        IRaceRelation Union(IRaceRelation relation);
        IRaceRelation Intersect(IRaceRelation relation);
        IRaceRelation Except(IRaceRelation relation);

        IRaceRelation Select(IList<IRaceRelationAttribute> attributes);
        IRaceRelation Select(IList<int> attributes);
        IRaceRelation Join(Func<IRaceTuple, IRaceTuple, bool> predicate, IRaceRelation relation);
        Func<IRaceRelation, IRaceRelation> Join(Func<IRaceTuple, IRaceTuple, bool> predicate);
        IRaceRelation Where(Func<IRaceTuple, bool> predicate);
        IRaceRelation Where(Predicate<IRaceTuple> predicate);

        bool Contains(IRaceTuple tuple);

    }

    public interface IRaceRelationAttribute
    {
        string Name { get; }
        IDomain Domain { get; }
        int Index { get; }
    }

    public interface IRaceTuple
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        : System.Runtime.CompilerServices.ITuple
#endif
    {
        int Size { get; }
        //object this[int index] { get; } // inherited from ITuple
        object this[string attributeName] { get; }
        object this[IRaceRelationAttribute attribute] { get; }
        T GetValue<T>(int index);
        T GetValue<T>(string attributeName);
        T GetValue<T>(IRaceRelationAttribute attribute);
    }
}
