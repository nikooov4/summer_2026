using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mephi.Cybernetics.Race.Core;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects
{
    public class EntityAttributesCollectionBase : EntityAttributeCollection
    {
        private readonly EntityAttributeCollection _computableProperties;
        private readonly EntityAttributeCollection _storedProperties;

        public EntityAttributesCollectionBase()
        {
            _computableProperties = new EntityAttributeCollection();
            _storedProperties = new EntityAttributeCollection();

        }

        public EntityAttributeCollection ComputableProperties => _computableProperties;

        public EntityAttributeCollection StoredProperties => _storedProperties;

        public override void AddProperty(EntityAttributeDescription property)
        {
            base.AddProperty(property);
            if (property.IsComputable)
                _computableProperties.AddProperty(property);
            else
                _storedProperties.AddProperty(property);
        }
    }

    public class EntityAttributesCollection : EntityAttributesCollectionBase
    {
        private readonly EntityAttributesCollectionBase _internalProperties;
        private readonly EntityAttributesCollectionBase _characteristics;
        private readonly EntityRelationshipsCollection _relationships;
        private readonly Dictionary<PropertyDescription, EntityAttributeDescription> _byPropertyDescription;
        bool _isFrozen = false;

        internal EntityAttributesCollection()
        {
            _internalProperties = new EntityAttributesCollectionBase();
            _characteristics = new EntityAttributesCollectionBase();
            _relationships = new EntityRelationshipsCollection();
            _byPropertyDescription = new Dictionary<PropertyDescription, EntityAttributeDescription>();
        }


        public EntityAttributesCollectionBase InternalProperties => _internalProperties;

        public EntityAttributesCollectionBase Characteristics => _characteristics;

        public EntityRelationshipsCollection Relationships => _relationships;

        public EntityAttributeDescription this[PropertyDescription propertyDescription] => _byPropertyDescription[propertyDescription];

        public override void AddProperty(EntityAttributeDescription property)
        {
            if (_isFrozen)
                throw new Exception("Invalid alteration access to a read-only collection");

            base.AddProperty(property);

            _byPropertyDescription.Add(property.ObjectProperty, property);

            if (property.IsRelation)
                _relationships.AddProperty(property);
            else
                _characteristics.AddProperty(property);

            if (property.IsInternal)
                _internalProperties.AddProperty(property);
        }

        public override void Replace(EntityAttributeDescription property)
        {
            base.Replace(property);

            if (property.IsRelation)
                Relationships.AddProperty(property);
            else
                _characteristics.AddProperty(property);

            if (property.IsInternal)
                _internalProperties.AddProperty(property);
        }
    }

    public class EntityRelationshipsCollection : EntityAttributeCollection
    {
        private readonly EntityAttributesCollectionBase _oneToOneRelationProperties;
        private readonly EntityAttributesCollectionBase _oneToManyRelationProperties;
        private readonly EntityAttributesCollectionBase _manyToManyRelationProperties;

        internal EntityRelationshipsCollection()
        {
            _oneToOneRelationProperties = new EntityAttributesCollectionBase();
            _oneToManyRelationProperties = new EntityAttributesCollectionBase();
            _manyToManyRelationProperties = new EntityAttributesCollectionBase();

        }

        public EntityAttributesCollectionBase OneToOne => _oneToOneRelationProperties;

        public EntityAttributesCollectionBase OneToMany => _oneToManyRelationProperties;

        public EntityAttributesCollectionBase ManyToMany => _manyToManyRelationProperties;

        public override void AddProperty(EntityAttributeDescription property)
        {
            base.AddProperty(property);
            if (property.IsManyToManyRelation || property.IsOneToManyRelation || property.IsOneToOneRelation)
            {
                if (property.IsManyToManyRelation)
                    _manyToManyRelationProperties.AddProperty(property);
                if (property.IsOneToManyRelation)
                    _oneToManyRelationProperties.AddProperty(property);
                if (property.IsOneToOneRelation)
                    _oneToOneRelationProperties.AddProperty(property);
            }

        }
    }

    public class EntityAttributeCollection : PropertyDescriptionCollectionBase<EntityAttributeDescription>
    {
        internal EntityAttributeCollection()
        {
        }

        public override void AddProperty(EntityAttributeDescription property)
        {
            base.AddProperty(property);
            _byIndex.Add(property.ObjectProperty.Index, property);
            _byPropInfo.Add(property.ObjectProperty.Property, property);
        }

        public override void Replace(EntityAttributeDescription property)
        {
            var oldProperty = _byName[property.Name];
            if (!(oldProperty.ObjectProperty.Index == property.ObjectProperty.Index && oldProperty.Name == property.Name))
                throw new ArgumentException("The replacing property must have exactly the same name and index as the property being replaced");
            _items.Remove(oldProperty);
            _items.Add(property);
            _byName[property.Name] = property;
            _byPropInfo.Remove(oldProperty.ObjectProperty.Property);
            _byPropInfo.Add(property.ObjectProperty.Property, property);
            _byIndex[property.ObjectProperty.Index] = property;
        }
    }
}
