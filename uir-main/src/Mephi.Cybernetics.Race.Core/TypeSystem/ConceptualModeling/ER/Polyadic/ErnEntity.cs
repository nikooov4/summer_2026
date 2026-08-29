using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Polyadic
{
    public class ErnEntity : IEntity
    {
        private readonly ErnDomainModel OwningModel;

        public ErnEntity(ErnDomainModel owningModel, string name, string ns, string source,
            IList<PropertyDescription> attributes,
            IList<EntityKey> potentialKeys, IReadOnlyCollection<ErnRelationship> relations)
        {
            OwningModel = owningModel;
            Name = name;
            Namespace = ns;
            Source = source;
            Relations = new List<ErnRelationship>(relations);
            Attributes = new EntityAttributeCollection(attributes.Select(a =>
                new EntityAttribute(a.Name, this, a.Range, a.IsComputable, a.IsReadOnly)), this);
            PotentialKeys = new ReadOnlyCollection<EntityKey>(potentialKeys);
        }

        public ReadOnlyCollection<EntityKey> PotentialKeys { get; }

        IList<IEntityKey> IEntity.PotentialKeys => PotentialKeys.Cast<IEntityKey>().ToList();

        public EntityAttributeCollection Attributes { get; }

        public IReadOnlyCollection<ErnRelationship> Relations { get; }

        public string Name { get; }

        public string Namespace { get; }

        public string Source { get; }

        public string FullName => $"{Namespace}{(!string.IsNullOrEmpty(Namespace) ? "." : "")}{Name}";

        public string FullyQualifiedName => $"[{Source}::{FullName}]";
    }
}