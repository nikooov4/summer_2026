namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic
{
    public class ErdEntityBindingInfo
    {
        public EntityAttributeCollection Attributes { get; set; }

        public EntityAssociationCollection Associations { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public string Source { get; set; }
    }
}