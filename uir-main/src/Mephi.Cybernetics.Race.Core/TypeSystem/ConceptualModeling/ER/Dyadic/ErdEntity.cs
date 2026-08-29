using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic
{
    public class ErdEntity : IEntity
    {
        private readonly ErdDomainModel OwningModel;

        private static bool IsEntity(ObjectDescription objectDescription)
        {
            return objectDescription.GetClrAttribute<IsEntityAttribute>() != null;
        }

        public ErdEntity(ErdDomainModel owningModel, string name, string ns, string source,
            IList<PropertyDescription> attributes,IList<EntityKey>  potentialKeys)
        {
            OwningModel = owningModel;
            Name = name;
            Namespace = ns;
            Source = source;
            Attributes = new EntityAttributeCollection(attributes.Select(a =>
                new EntityAttribute(a.Name, this, a.Range, a.IsComputable, a.IsReadOnly)), this);
            PotentialKeys = new ReadOnlyCollection<EntityKey>(potentialKeys);
        }

        internal void Init(List<IEntityAssociation> associations)
        {
            IList<IEntityAssociation> o2o = new List<IEntityAssociation>();
            IList<IEntityAssociation> o2m = new List<IEntityAssociation>();
            IList<IEntityAssociation> m2m = new List<IEntityAssociation>();

            foreach (var association in associations)
            {
                if ((association.SourceCardinality == Cardinality.One ||
                     association.SourceCardinality == Cardinality.ZeroOrOne) &&
                    (association.TargetCardinality == Cardinality.One ||
                     association.TargetCardinality == Cardinality.ZeroOrOne))
                {
                    o2o.Add(association);
                }
                else if ((association.SourceCardinality == Cardinality.One ||
                          association.SourceCardinality == Cardinality.ZeroOrOne) &&
                         (association.TargetCardinality == Cardinality.OneOrMore ||
                          association.TargetCardinality == Cardinality.ZeroOrMore))
                {
                    o2m.Add(association);
                }
                else if ((association.SourceCardinality == Cardinality.OneOrMore ||
                          association.SourceCardinality == Cardinality.ZeroOrMore) &&
                         (association.TargetCardinality == Cardinality.OneOrMore ||
                          association.TargetCardinality == Cardinality.ZeroOrMore))
                {
                    m2m.Add(association);
                }
                else if ((association.SourceCardinality == Cardinality.OneOrMore ||
                          association.SourceCardinality == Cardinality.ZeroOrMore) &&
                         (association.TargetCardinality == Cardinality.One ||
                          association.TargetCardinality == Cardinality.ZeroOrOne))
                {
                    throw new NotImplementedException();// maybe o2o
                }
            }

            Associations = new EntityAssociationCollection(o2o, o2m, m2m);
        }


        public ReadOnlyCollection<EntityKey> PotentialKeys { get; }
        IList<IEntityKey> IEntity.PotentialKeys => PotentialKeys.Cast<IEntityKey>().ToList();
            //throw new NotImplementedException();???

        public EntityAttributeCollection Attributes { get; }

        public EntityAssociationCollection Associations { get; private set; }

        public string Name { get; }

        public string Namespace { get; }

        public string Source { get; }

        public string FullName => $"{Namespace}{(!string.IsNullOrEmpty(Namespace) ? "." : "")}{Name}";

        public string FullyQualifiedName => $"[{Source}::{FullName}]";
    }
}