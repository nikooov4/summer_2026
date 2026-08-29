using System;
using System.Collections;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER
{
    public class EntityAttributeCollectionBase : IEntityAttributeCollection, IEnumerable<EntityAttribute>
    {
        private readonly List<EntityAttribute> _attributes;
        private readonly Dictionary<String, EntityAttribute> _attributesByName;
        protected readonly IEntity _reflectedDomain;


        protected internal EntityAttributeCollectionBase()
        {
            _attributes = new List<EntityAttribute>();
            _attributesByName = new Dictionary<string, EntityAttribute>();
        }

        protected EntityAttributeCollectionBase(IList<EntityAttribute> attributes, IEntity reflectedDomain) : this(
            attributes as IEnumerable<EntityAttribute>, reflectedDomain)
        {
        }

        protected EntityAttributeCollectionBase(IEnumerable<EntityAttribute> attributes, IEntity reflectedDomain)
        {
            _reflectedDomain = reflectedDomain;
            _attributes = new List<EntityAttribute>(attributes);
            _attributesByName = new Dictionary<string, EntityAttribute>();
            foreach (var attribute in _attributes)
            {
                _attributesByName.Add(attribute.Name, attribute);
            }
        }

        public EntityAttribute this[int index] => _attributes[index];
        public EntityAttribute this[string name] => _attributesByName[name];

        IEntityAttribute IAttributeCollection2<IEntity, IDomain, IEntityAttribute>.this[int index] =>
            _attributes[index];

        IEntityAttribute IAttributeCollection2<IEntity, IDomain, IEntityAttribute>.this[string name] =>
            _attributesByName[name];

        IEntity IAttributeCollection2<IEntity, IDomain, IEntityAttribute>.ReflectedDomain => _reflectedDomain;


        public bool Contains(string name)
        {
            return _attributesByName.ContainsKey(name);
        }

        public IEnumerator<EntityAttribute> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator<IEntityAttribute> IEnumerable<IEntityAttribute>.GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        protected internal void AddAttribute(EntityAttribute attribute)
        {
            _attributes.Add(attribute);
            _attributesByName.Add(attribute.Name, attribute);
        }
    }

    public class EntityAttributeCollection : EntityAttributeCollectionBase, IEntityAttributeCollectionExt,
        IDomainAttributeCollection
    {
        private readonly EntityAttributeCollectionBase _storedAttributes;
        private readonly EntityAttributeCollectionBase _сomputableAttributes;


        public EntityAttributeCollection(IList<EntityAttribute> attributes, IEntity reflectedDomain) : this(
            attributes as IEnumerable<EntityAttribute>, reflectedDomain)
        {
        }

        public EntityAttributeCollection(IEnumerable<EntityAttribute> attributes, IEntity reflectedDomain) : base(
            attributes, reflectedDomain)
        {
            _storedAttributes = new EntityAttributeCollectionBase();
            _сomputableAttributes = new EntityAttributeCollectionBase();
            foreach (var attr in attributes)
            {
                if (attr.IsComputable)
                {
                    _сomputableAttributes.AddAttribute(attr);
                }
                else
                {
                    _storedAttributes.AddAttribute(attr);
                }
            }
        }

        public EntityAttributeCollectionBase StoredAttributes => _storedAttributes;
        public EntityAttributeCollectionBase ComputableAttributes => _сomputableAttributes;

        IEntityAttributeCollection IEntityAttributeCollectionExt.StoredAttributes => _storedAttributes;

        IEntityAttributeCollection IEntityAttributeCollectionExt.ComputableAttributes =>
            _сomputableAttributes;

        IEntity IAttributeCollection2<IEntity, IDomain, IEntityAttribute>.ReflectedDomain =>
            _reflectedDomain;

        IDomainAttribute IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.this[int index] => base[index];

        IDomainAttribute IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.this[string name] => base[name];

        IEnumerator<IDomainAttribute> IEnumerable<IDomainAttribute>.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        IDomain IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.ReflectedDomain => _reflectedDomain;
    }
}