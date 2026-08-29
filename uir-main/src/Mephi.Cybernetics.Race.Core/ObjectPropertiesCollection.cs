using System;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core
{


    //TODO: add a read-only collection interface
    public class ObjectPropertiesCollection : PropertyDescriptionCollection, IEnumerable<PropertyDescription>
    {
        private readonly PropertyDescriptionCollection _computableProperties;
        private readonly PropertyDescriptionCollection _storedProperies;
//        private readonly PropertyDescriptionCollection _internalProperties;
        bool _isFrozen = false;

        internal ObjectPropertiesCollection()
        {
            _computableProperties = new PropertyDescriptionCollection();
            _storedProperies = new PropertyDescriptionCollection();
//            _internalProperties =  new PropertyDescriptionCollection();
        }



        public PropertyDescriptionCollection ComputableProperties => _computableProperties;

        public PropertyDescriptionCollection StoredProperies => _storedProperies;


        //        public IEnumerator<PropertyDescription> GetEnumerator()
//        {
//            return new SwitchableEnumerator<PropertyDescription>(_simpleProperties, _oneToManyRelationProperties, 
//                _oneToManyRelationProperties, _manyToManyRelationProperties);
//        }
//
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

        public override void AddProperty(PropertyDescription property)
        {
            if (_isFrozen)
                throw new Exception("Invalid alteration access to a read-only collection");

            base.AddProperty(property);

            if (property.IsComputable)
                _computableProperties.AddProperty(property);
            else
                _storedProperies.AddProperty(property);

//            if (!property.IsInternal)
//                _internalProperties.AddProperty(property);

//            if (property.IsMapped)
//                _mappedProperties.AddProperty(property);

        }

        internal void EndInit()
        {
            _isFrozen = true;
        }

//        public int Count
//        {
//            get
//            {
//                return _simpleProperties.Count + _oneToOneRelationProperties.Count + _oneToManyRelationProperties.Count +
//                       _manyToManyRelationProperties.Count;
//            }
//        }
//
//        public bool Contains(PropertyDescription property)
//        {
//            return _simpleProperties.Contains(property) || _oneToOneRelationProperties.Contains(property) ||
//                   _oneToManyRelationProperties.Contains(property) || _manyToManyRelationProperties.Contains(property);
//        }
    }


}