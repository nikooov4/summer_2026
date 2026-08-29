using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Polyadic
{
    public class ErnDomainModel : IStrictTypeSystem
    {
        public IRelationship GetRelationship(PropertyDescription property, ObjectDescription od)
        {
            IList<IRelationshipArgumentInfo> relationshipArgumentInfo;
            IEntity ernEntity = GetEntity(od);
            string name;
            ISemanticRoleset roleset;
            int arity;
            IRelationshipArgumentInfo[] arguments;
            IRelationship relationship = null;
            
                /*new ErnRelationship(
                relationshipArgumentInfo,
                ernEntity,
                name,
                roleset,
                arity,
                arguments
                );*/
            return relationship;  
        }

        public ErnEntity GetEntity(ObjectDescription od)
        {
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
            IReadOnlyCollection<ErnRelationship> relations = new List<ErnRelationship>();
            var entity = new ErnEntity(this, od.Name, od.Namespace, od.Source,
                (od.Properties as IEnumerable<PropertyDescription>).Where(IsNotRelation).ToList(), potentialKeys, relations);
          return  entity;
        }
        
        private bool IsNotRelation(PropertyDescription property)
        {
            return !(property.Range.HasClrAttribute<IsEntityAttribute>()
                     || (property.RangeType.IsGenericType && property.RangeType.GetGenericArguments()[0]
                         .GetCustomAttribute<IsEntityAttribute>() != null));
        }
        
        public static int GetPotentialKey(PropertyDescription propertyDescription)
        {
            return propertyDescription.GetAttribute(typeof(IdFieldAttribute)) != null
                ? ((IdFieldAttribute)propertyDescription.GetAttribute(typeof(IdFieldAttribute))).PotentialKeyId
                : 0;
        }

        public bool IsInstanceOf(object obj, IDomain domain)
        {
            throw new NotImplementedException();//-
        }

        public bool IsSubtypingSupported { get; }
        public bool IsSubtypeOf(ErnEntity entity1, ErnEntity entity2)
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

        bool ITypeSystem.IsSubtypeOf(IDomain domain1, IDomain domain2)
        {
            if (domain1 is ErnEntity entity1 && domain2 is ErnEntity entity2)
            {
                return IsSubtypeOf(entity1, entity2);
            }
            else
                throw new ArgumentException("Both domain1 and domain2 must be Ern   Entity");
        }

        IDomainSubtypingInfo ITypeSystem.GetSubtypingInfo(IDomain domain)
        {
            if (domain is ErnEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErnEntity");
        }

        public bool IsAttributesSupported { get; }
        public IDomainAttributeCollection GetDomainAttributes(IDomain domain)
        {
            if (domain is ErnEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErnEntity");
        }

        public bool IsFunction(IDomain domain)
        {
            if (domain is ErnEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErnEntity");
        }

        public IFunctionTypeInfo GetFunctionTypeInfo(IDomain domain)
        {
            if (domain is ErnEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErnEntity");
        }

        public bool IsComposite(IDomain domain)
        {
            if (domain is ErnEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErnEntity");
        }

        public IDomainStructureInfo GetStructureInfo(IDomain domain)
        {
            if (domain is ErnEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErnEntity");
        }

        public bool IsCollection(IDomain domain)
        {
            if (domain is ErnEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErnEntity");
        }

        public ICollectionDomain GetCollectionInfo(IDomain domain)
        {
            if (domain is ErnEntity entity)
            {
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException("Domain must be ErnEntity");
        }

        public IDomain GetTypeInfo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}