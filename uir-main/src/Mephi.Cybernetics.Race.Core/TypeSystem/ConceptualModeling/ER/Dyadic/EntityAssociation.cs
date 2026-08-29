using System;
using System.Collections.Generic;
using System.Linq;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic;

public class EntityAssociation : IEntityAssociation
{

    public EntityAssociation(

        PropertyDescription propertyDescription,
        ErdEntity domainEntity,
        ErdEntity rangeEntity,
        Cardinality sourceCardinality,
        Cardinality targetCardinality
    )
    {
        Name = propertyDescription.Name;
        Domain = domainEntity;
        Range = rangeEntity;
        IsReadOnly = propertyDescription.IsReadOnly;
        OwningEntity = domainEntity;
        AssociatedEntity = rangeEntity;
        SourceCardinality = sourceCardinality;
        TargetCardinality = targetCardinality;
    }

    public IEntity OwningEntity { get; }
    public IEntity AssociatedEntity { get; }
    public Cardinality SourceCardinality { get; }
    public Cardinality TargetCardinality { get; }

    public string Name { get; }
    public IEntity Domain { get; }
    public IEntity Range { get; }
    public bool IsReadOnly { get; }

    public object GetValue(object o)
    {
        throw new NotImplementedException();
    }

    public void SetValue(object o, object value)
    {
        throw new NotImplementedException();
    }

    public T GetValue<T>(object o)
    {
        throw new NotImplementedException();
    }

    public void SetValue<T>(object o, T value)
    {
        throw new NotImplementedException();
    }
}

public class BinaryRelationship : IRelationship
{

    IRelationshipArgumentInfo ISignature<IEntity, IRelationshipArgumentInfo>.this[int index] => throw new NotImplementedException();

    IRelationshipArgumentInfo ISignature<IEntity, IRelationshipArgumentInfo>.this[string name] => throw new NotImplementedException();

    public string Namespace => throw new NotImplementedException();

    public string Source => throw new NotImplementedException();

    public string FullName => throw new NotImplementedException();

    public string FullyQualifiedName => throw new NotImplementedException();

    string IDomain.Name => throw new NotImplementedException();

    ISemanticRoleset IRelationship.Roleset => throw new NotImplementedException();

    IRelationshipArgumentInfo[] ISignature<IEntity, IRelationshipArgumentInfo>.Arguments => throw new NotImplementedException();

    int ISignature<IEntity, IRelationshipArgumentInfo>.Arity => throw new NotImplementedException();
}

public struct UnorderedPair<T1,T2>
{
    private HashSet<Either<T1, T2>> _values;//????

    public IReadOnlyCollection<Either<T1,T2>> Items => throw new NotImplementedException();
    public bool Contains(T1 item) => throw new NotImplementedException();
    public bool Contains(T2 item) => throw new NotImplementedException();
    public bool Contains(object item) => throw new NotImplementedException();
    public bool Contains(Either<T1,T2> item) => throw new NotImplementedException();
}