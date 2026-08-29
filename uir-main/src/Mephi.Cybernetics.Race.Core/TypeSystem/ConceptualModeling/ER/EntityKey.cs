using System;
using System.Collections;
/* Unmerged change from project 'Mephi.Cybernetics.Race.Core (net4.7.2)'
Before:
using System.Collections.Generic;
After:
using System.Collections.Generic;
using Mephi;
using Mephi.Cybernetics;
using Mephi.Cybernetics.Race;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic;
*/
using System.Collections.Generic;
using System.Linq;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER
{
    public class EntityKey : IEntityKey
    {
        private readonly List<EntityAttribute> _attributes;
        private Dictionary<string,EntityAttribute> _attributesByName;

        public EntityKey(ICollection<EntityAttribute> attibutes)
        {
            _attributes = new(attibutes);
            _attributesByName = new Dictionary<string, EntityAttribute>();
            foreach (var atr in attibutes)
            {
                _attributesByName.Add(atr.Name,atr);
            }
        }

        public IEntityAttribute this[string name]=> _attributesByName[name];
        

        public IEntityAttribute this[int index] => _attributes[index];

        public bool Contains(IEntityAttribute attribute)
        {
            return _attributes.Contains(attribute);
        }

        bool IEntityKey.Contains(IEntityAttribute attribute)
        {
            return Contains(attribute);
        }

        public bool Contains(string name)=>_attributesByName.ContainsKey(name);
        public bool ContainsFullName(string name)
        {
            return _attributes.Exists(attribute => attribute.FullName.Equals(name));
        }

        public bool Equals(EntityKey other)
        {
            return _attributes.All(attribute =>
                other._attributes.Any(entityAttribute => entityAttribute.Equals(attribute)));
        }

        bool IEquatable<IEntityKey>.Equals(IEntityKey other)
        {
            return _attributes.All(attribute => other != null && other.Contains(attribute));
        }

        public IEnumerator<IEntityAttribute> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}