namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic
{
    public class EntityAssociationCollectionBindingInfo
    {
        public IEntityOneToOneAssociationCollection OneToOne { get; set; }

        public IEntityOneToManyAssociationCollection OneToMany { get; set; }

        public IEntityManyToManyAssociationCollection ManyToMany { get; set; }

        public IEntity ReflectedDomain { get; set; }
    }
}