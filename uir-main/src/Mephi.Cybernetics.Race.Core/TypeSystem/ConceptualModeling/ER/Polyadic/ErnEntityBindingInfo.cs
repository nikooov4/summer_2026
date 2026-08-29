using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Polyadic
{
    public class ErnEntityBindingInfo
    {
        public EntityAttributeCollection Attributes { get; set; }

        public IReadOnlyCollection<ErnRelationship> Relations { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public string Source { get; set; }
    }
}