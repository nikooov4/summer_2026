namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling
{
    public interface IRelationship : IDomain, ISignature<IEntity, IRelationshipArgumentInfo>
    {
        ISemanticRoleset Roleset { get; }
    }

    public interface IRelationshipArgumentInfo : IArgumentInfo<IEntity>
    {
        Cardinality Cardinality { get; }
        SemanticRoleInfo RoleInfo { get; }
    }

    public interface IRelationshipAttribute : IAttributeBase<IRelationship>
    {

    }

    public interface IRelationshipAttributeCollection : IAttributeCollection<IRelationship, IRelationshipAttribute>
    {

    }
}