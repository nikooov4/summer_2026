using System;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER
{
    public class EntityAttribute : IEntityAttribute, IDomainAttribute
    {
        public EntityAttribute(string name, IEntity entity, ObjectDescription range, bool isComputable, bool isReadOnly)
        {
            Name = name;
            Domain= entity;//Это здесь нужно?
            Range = range;
            IsComputable = isComputable;
            IsReadOnly = isReadOnly;
        }
        
        public EntityAttribute(string name, ObjectDescription range, bool isComputable, bool isReadOnly)
        {
            Name = name;
            Range = range;
            IsComputable = isComputable;
            IsReadOnly = isReadOnly;
        }

        public string Name { get; }
        IDomain IAttributeBase2<IDomain, IDomain>.Domain => Domain;

        public string FullName => $"{Domain.Name}.{Name}";
        
        public IEntity Domain { get; }

        public IDomain Range { get; }

        public bool IsGlobal => throw new NotImplementedException();

        public bool IsComputable { get; }

        public bool IsReadOnly { get; }

        public object GetValue(object o)
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(object o)
        {
            throw new NotImplementedException();
        }

        public void SetValue(object o, object value)
        {
            throw new NotImplementedException();
        }

        public void SetValue<T>(object o, T value)
        {
            throw new NotImplementedException();
        }
    }
}