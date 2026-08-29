using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling
{
    public interface IDomainModel<TEntity, TRelatioship> : ITypeSystem
        where TEntity : IEntity
        where TRelatioship : IRelationship
    {
        IReadOnlyCollection<TEntity> StoredEntities { get; } //KnownEntities ?
        IReadOnlyCollection<TRelatioship> StoredRelationships { get; } // KnownRelationships
    }
}
