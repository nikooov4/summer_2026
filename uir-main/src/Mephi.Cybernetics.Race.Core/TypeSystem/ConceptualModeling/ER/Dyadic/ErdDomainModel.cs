using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Polyadic;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic
{
    public class ErdDomainModel : /* IDomainModel<ErdEntity,EntityAssociation>,*/ IStrictTypeSystem
    {
        public ErdDomainModel GetFrom(IEnumerable<Type> types) 
        {
            //Пример вызова:
            var allEntitiesInAssembly = Assembly.GetExecutingAssembly().GetExportedTypes()
                //.Select(type => ObjectDescription.Get(type)) //map
                .Select(ObjectDescription.Get) //map
                .Where(od => od.HasClrAttribute<IsEntityAttribute>());
            return GetFrom(allEntitiesInAssembly);
            throw new NotImplementedException();
        }

        public ErdDomainModel GetFrom(IEnumerable<IDomain> types, ITypeSystem typeSystem)
        {
            throw new NotImplementedException();
        }

        public ErdDomainModel GetFrom<TTypeInfo>(IEnumerable<TTypeInfo> types) where TTypeInfo : IDomain, IDomainAttributesInfo
        {
            throw new NotImplementedException();
        }

        public static bool IsEntity(ObjectDescription objectDescription)
        {
            return objectDescription.GetClrAttribute<IsEntityAttribute>() != null;
        }

        public static bool IsRelationship(ObjectDescription objectDescription, PropertyDescription propertyDescription)

        {
            return objectDescription.GetClrAttribute<IsRelationshipAttribute>() != null
                   || propertyDescription.HasAttribute(typeof(IsManyToManyAttribute));
        }

        public static Cardinality GetSourceCardinality(ObjectDescription objectDescription)
        {
            return objectDescription.GetClrAttribute<IsOneToOneAttribute>() != null
                ? objectDescription.GetClrAttribute<IsOneToOneAttribute>().SourceCardinality
                : objectDescription.GetClrAttribute<IsOneToManyAttribute>() != null
                    ? objectDescription.GetClrAttribute<IsOneToManyAttribute>().SourceCardinality
                    : null;
        }

        public static Cardinality GetTargetCardinality(ObjectDescription objectDescription)
        {
            return objectDescription.GetClrAttribute<IsOneToOneAttribute>() != null
                ? objectDescription.GetClrAttribute<IsOneToOneAttribute>().TargetCardinality
                : objectDescription.GetClrAttribute<IsOneToManyAttribute>() != null
                    ? objectDescription.GetClrAttribute<IsOneToManyAttribute>().TargetCardinality
                    : null;
        }

        public static int GetPotentialKey(PropertyDescription propertyDescription)
        {
            return propertyDescription.GetAttribute(typeof(IdFieldAttribute)) != null
                ? ((IdFieldAttribute)propertyDescription.GetAttribute(typeof(IdFieldAttribute))).PotentialKeyId
                : 0;
        }

        private Dictionary<ObjectDescription, ErdEntity> _byObjectDescription =
            new Dictionary<ObjectDescription, ErdEntity>();

        private List<ErdEntity> _entities = new List<ErdEntity>();
        private List<IEntityAssociation> _associations = new List<IEntityAssociation>();

        public IReadOnlyCollection<ErdEntity> StoredEntities => _entities;
        public IReadOnlyCollection<IEntityAssociation> StoredRelationships => _associations;

        public bool IsSubtypingSupported => throw new NotImplementedException();

        public bool IsAttributesSupported => throw new NotImplementedException();


        public ErdEntity GetEntity(IDomain domain, ITypeSystem typeSystem)
        {
            throw new NotImplementedException();
        }

        public ErdEntity GetEntity<TTypeInfo, TTypeSystem>(TTypeInfo entityDescription, TTypeSystem typeSystem) // mb не обязательно TTypeSystem
            where TTypeInfo : IDomain, IDomainAttributesInfo
            where TTypeSystem : ITypeSystem
        {
            throw new NotImplementedException();
        }

        public ErdEntity GetEntity(ObjectDescription od)
        {
            if (!od.HasClrAttribute<IsEntityAttribute>())
            {
                throw new ArgumentException("Domain must be an entity");
            }

            if (_byObjectDescription.ContainsKey(od))
                return _byObjectDescription[od];

            var potentialKeys = od.Properties.StoredProperies
                .Where(item => ((PropertyDescription)item).HasAttribute(typeof(IdFieldAttribute)))
                .GroupBy(attribute => GetPotentialKey(attribute as PropertyDescription))
                .ToDictionary(
                    a => a.Key,
                    b => b.Select(attribute =>
                            new EntityAttribute(attribute.Name, ObjectDescription.Get(attribute), false, false))
                        .ToList())
                .ToDictionary(pair => pair.Key, pair => new EntityKey(pair.Value))
                .Values.ToList();

            var entity = new ErdEntity(this, od.Name, od.Namespace, od.Source,
                (od.Properties as IEnumerable<PropertyDescription>).Where(IsNotAssociation).ToList(), potentialKeys);
            _entities.Add(entity);
            _byObjectDescription[od] = entity;
            entity.Init((od.Properties as IEnumerable<PropertyDescription>)
                .Where(IsAssociation)
                .Select(property => GetAssociation(property, od))
                .ToList());
            return entity;
        }

        public IEntityAssociation GetAssociation(PropertyDescription property, ObjectDescription od)
        {
            var entity = _byObjectDescription[od];

            var propertyDescriptionRange = property.Range;

            var isCollection = false; //ObjectDescription.TypeSystemDescription.IsCollection(propertyDescriptionRange); 
            var isEntity = IsEntity(propertyDescriptionRange);

            if (IsAssociation(property) || isEntity || isCollection) // см EntityAttributeDescription
            {
                //o2o
                var objectDescriptionRange = propertyDescriptionRange.ClrType.IsGenericType
                    ? ObjectDescription.Get(propertyDescriptionRange.ClrType.GetGenericArguments()[0])
                    : property.Range;
                Cardinality sourceCardinality = GetSourceCardinality(objectDescriptionRange) ?? Cardinality.One;
                Cardinality targetCardinality = GetTargetCardinality(objectDescriptionRange) ?? Cardinality.One;

                if (IsRelationship(objectDescriptionRange, property))
                {
                    //m2m
                    sourceCardinality = Cardinality.OneOrMore;
                    targetCardinality = Cardinality.OneOrMore;
                }
                else if (propertyDescriptionRange.ClrType.IsGenericType)
                {
                    //o2m
                    sourceCardinality = GetSourceCardinality(objectDescriptionRange) ?? Cardinality.One;
                    targetCardinality = GetTargetCardinality(objectDescriptionRange) ?? Cardinality.OneOrMore;
                }

                var assoc = new EntityAssociation(
                    property,
                    entity,
                    GetEntity(objectDescriptionRange),
                    sourceCardinality,
                    targetCardinality
                );
                _associations.Add(assoc);
                return assoc;
            }

            throw new NotImplementedException();
        }

        public bool IsInstanceOf(object obj, ErdEntity domain)
        {
            throw new NotImplementedException();
        }

        public bool IsSubtypeOf(ErdEntity entity1, ErdEntity entity2)
        {
            var isSubtypeOf = true;

            foreach (var computableAttribute in entity2.Attributes.ComputableAttributes)
            {
                isSubtypeOf = isSubtypeOf && entity1.Attributes.ComputableAttributes.Contains(computableAttribute);
            }

            foreach (var storedAttribute in entity2.Attributes.StoredAttributes)
            {
                isSubtypeOf = isSubtypeOf && entity1.Attributes.StoredAttributes.Contains(storedAttribute);
            }

            return isSubtypeOf;
        }

        public IEntity GetTypeInfo(object obj)
        {
            return GetEntity(ObjectDescription.Get(obj));
        }

        IDomain IStrictTypeSystem.GetTypeInfo(object obj)
        {
            return GetTypeInfo(obj);
        }

        bool ITypeSystem.IsInstanceOf(object obj, IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                return IsInstanceOf(obj, entity);
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        bool ITypeSystem.IsSubtypeOf(IDomain domain1, IDomain domain2)
        {
            if (domain1 is ErdEntity entity1 && domain2 is ErdEntity entity2)
            {
                return IsSubtypeOf(entity1, entity2);
            }
            else
                throw new ArgumentException("Both domain1 and domain2 must be ErdEntity");
        }

        IDomainSubtypingInfo ITypeSystem.GetSubtypingInfo(IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        IDomainAttributeCollection ITypeSystem.GetDomainAttributes(IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                return entity.Attributes;
                throw new NotImplementedException();//получить ObjectDescription и взять GetValue сделать промежуточный комит
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        bool ITypeSystem.IsFunction(IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        IFunctionTypeInfo ITypeSystem.GetFunctionTypeInfo(IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        bool ITypeSystem.IsComposite(IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        IDomainStructureInfo ITypeSystem.GetStructureInfo(IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        bool ITypeSystem.IsCollection(IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        ICollectionDomain ITypeSystem.GetCollectionInfo(IDomain domain)
        {
            if (domain is ErdEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErdEntity");
        }

        private bool IsNotAssociation(PropertyDescription property)
        {
            return !(property.Range.HasClrAttribute<IsEntityAttribute>()
                     || (property.RangeType.IsGenericType && property.RangeType.GetGenericArguments()[0]
                         .GetCustomAttribute<IsEntityAttribute>() != null));
        }

        private bool IsAssociation(PropertyDescription property)
        {
            // должно быть: IsEntity(property.Range) ||
            return property.Range.HasClrAttribute<IsEntityAttribute>()
                   || (property.RangeType.IsGenericType && //property.RangeType.IsCollection // property.RangeType.IsCollectionOf(...)
                       property.RangeType.GetGenericArguments()[0].GetCustomAttribute<IsEntityAttribute>() != null);
        }
    }
}