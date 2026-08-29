using System;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling
{
    public interface IEntity : IDomain
    {
        IList<IEntityKey> PotentialKeys { get; }

    }

    //public interface IEntityProperty : IAttributeBase<IDomain>
    //{

    //}

    //TODO: реализовать структуру, описывающую функциональные зависимости между атрибутами
    public interface IEntityAttribute : IAttributeBase2<IEntity, IDomain>
    {
        string FullName { get; }
        bool IsGlobal { get; }
        bool IsComputable { get; }
    }


    public interface  IEntityKey : IEnumerable<IEntityAttribute>, IEquatable<IEntityKey> //, IImmutableSet<IEntityAttribute>
    {
        IEntityAttribute this[string name] { get; }
        IEntityAttribute this[int index] { get; }
        bool Contains(IEntityAttribute attribute);
        bool Contains(string name);
        bool ContainsFullName(string name);
    }

    //TODO: подумать над (частичным) упорядочением см Algorithms
    public class Cardinality
    {
        public static Cardinality ZeroOrOne { get; }
        public static Cardinality ZeroOrMore { get; }
        public static Cardinality One { get; }
        public static Cardinality OneOrMore { get; }

        static Cardinality()
        {
            Cardinality.ZeroOrOne = new Cardinality(0, 1);
            Cardinality.ZeroOrMore = new Cardinality(0, int.MaxValue);
            Cardinality.One = new Cardinality(1, 1);
            Cardinality.OneOrMore = new Cardinality(1, int.MaxValue);
        }

        public Cardinality(int minimal, int maximal)
        {
            Minimal = minimal;
            Maximal = maximal;
        }

        public int Minimal { get; }
        public int Maximal { get; }

        protected bool Equals(Cardinality other)
        {
            return Minimal == other.Minimal && Maximal == other.Maximal;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Cardinality)obj);
        }

        public override int GetHashCode()
        {
            return (Minimal * 397) ^ Maximal;

        }
    }

    public interface IEntityAssociation : IAttributeBase2<IEntity, IEntity>
    {
        IEntity OwningEntity { get; }
        IEntity AssociatedEntity { get; }
        Cardinality SourceCardinality { get; }
        Cardinality TargetCardinality { get; }
    }

    //public interface IEntityPropertyCollection : IAttributeCollection<IEntity, IEntityProperty>
    //{
    //    IEntityAttributeCollection Attributes { get; }
    //    IEntityAssociationCollection AsscCollections { get; }
    //}

    public interface IEntityAttributeCollection : IAttributeCollection2<IEntity, IDomain, IEntityAttribute>
    {
    }

    public interface IEntityAttributeCollectionExt : IAttributeCollection2<IEntity, IDomain, IEntityAttribute>
    {
        IEntityAttributeCollection StoredAttributes { get; }
        IEntityAttributeCollection ComputableAttributes { get; }
    }

    public interface IEntityAssociationCollection : IAttributeCollection2<IEntity, IEntity, IEntityAssociation>
    {
        IEntityOneToOneAssociationCollection OneToOne { get; }
        IEntityOneToManyAssociationCollection OneToMany { get; }
        IEntityManyToManyAssociationCollection ManyToMany { get; }
    }

    public interface IEntityOneToOneAssociationCollection : IAttributeCollection2<IEntity, IEntity, IEntityAssociation>
    {
    }

    public interface IEntityOneToManyAssociationCollection : IAttributeCollection2<IEntity, IEntity, IEntityAssociation>
    {
    }

    public interface
        IEntityManyToManyAssociationCollection : IAttributeCollection2<IEntity, IEntity, IEntityAssociation>
    {
    }
}