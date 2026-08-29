using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling;

namespace Mephi.Cybernetics.Race.Core.TypeSystem
{
    public class TypeInfoProvider<TTypeSystem,TType,TValue> where TTypeSystem : IRaceTypeSystem
    {
        public TypeInfoProvider<TTypeSystem, TType, TValue> UseGetType(Func<TType,TValue> getType)
        {
            return this;
        }
        public TypeInfoProvider<TTypeSystem, TType, TValue> UseGetTypeName(Func<TType,string> getTypeName)
        {
            return this;
        }
    }

    public interface IAnyTypeProvider
    {
        IDomain TypeAny { get; }
    }
    public interface IUnitTypeProvider
    {
        IDomain TypeUnit { get; }
    }
    public interface INumericTypeProvider
    {
        IDomain TypeNumeric { get; }
        IDomain TypeInteger { get; }
        IDomain TypeFloat { get; }
        IDomain TypeDouble { get; }
        IDomain TypeDecimal { get; }
        IDomain TypeInt8 { get; }
        IDomain TypeInt16 { get; }
        IDomain TypeInt32 { get; }
        IDomain TypeInt64 { get; }
    }
    public interface IStandardTypesProvider : IAnyTypeProvider, IUnitTypeProvider, INumericTypeProvider
    {

    }
    public interface IRaceTypeSystem
    {
        IList<IDomain> PrimaryTypes { get; }
        IList<IDomainConstructor> PrimaryTypeConstructors { get; }
        bool IsDerivedFromPrimaries(IDomain domain);
        IDomain TypeAny { get; }
        IDomain TypeUnit { get; }

        bool IsRepresentationOf(ObjectDescription objectDescription, IDomain domain);

        bool IsSubtypeOf(IDomain domain1, IDomain domain2);
        IList<IDomain> GetAncestors(IDomain domain);
        IDomainSubtypingInfo GetSubtypingInfo(IDomain domain);

        IDomainAttributeCollection GetDomainAttributes(IDomain domain);

        bool IsFunction(IDomain domain);
        IFunctionTypeInfo GetFunctionTypeInfo(IDomain domain);

        bool IsComplex(IDomain domain);
        IDomainStructureInfo GetStructureInfo(IDomain domain);

        bool IsCollection(IDomain domain);
        ICollectionDomain GetCollectionInfo(IDomain domain);


    }

    public interface ITypeSystem
    {
        bool IsInstanceOf(object obj, IDomain domain);

        bool IsSubtypingSupported { get; }
        bool IsSubtypeOf(IDomain domain1, IDomain domain2);
        IDomainSubtypingInfo GetSubtypingInfo(IDomain domain);

        bool IsAttributesSupported { get; }
        IDomainAttributeCollection GetDomainAttributes(IDomain domain);

        bool IsFunction(IDomain domain);
        IFunctionTypeInfo GetFunctionTypeInfo(IDomain domain);

        bool IsComposite(IDomain domain);
        IDomainStructureInfo GetStructureInfo(IDomain domain);

        bool IsCollection(IDomain domain);
        ICollectionDomain GetCollectionInfo(IDomain domain);
    }

    public interface IStrictTypeSystem : ITypeSystem
    {
        IDomain GetTypeInfo(object obj);
    }
}