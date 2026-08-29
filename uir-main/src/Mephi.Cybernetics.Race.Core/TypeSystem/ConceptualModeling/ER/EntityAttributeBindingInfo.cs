namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER
{
//TODO: class or structure????
    public class EntityAttributeBindingInfo
    {
        public string FullName { get; set; }

        public bool IsGlobal { get; set; }

        public bool IsComputable { get; set; }

        public string Name { get; set; }

        public string Domain { get; set; }

        public string Range { get; set; }

        public bool IsReadOnly { get; set; }
    }
}