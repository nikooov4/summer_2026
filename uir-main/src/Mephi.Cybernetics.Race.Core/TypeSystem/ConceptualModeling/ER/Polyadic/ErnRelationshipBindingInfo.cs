namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Polyadic
{
    public class ErnRelationshipBindingInfo
    {
        public string Name { get; set; }

        public ISemanticRoleset Roleset { get; set; }

        public IRelationshipArgumentInfo[] Arguments { get; set; }

        public int Arity { get; set; }
    }
}