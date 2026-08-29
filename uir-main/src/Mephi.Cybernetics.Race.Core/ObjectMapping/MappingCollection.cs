using System.Collections;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.ObjectMapping
{
    public class MappingCollection<TSource> : IEnumerable<PropertyMapping<TSource>>
    {
        private readonly ObjectDescription _targetEntityDescription;
        private PropertyMapping<TSource>[] _properties;
        private Dictionary<string, PropertyMapping<TSource>> _propertiesDic;
        private readonly ObjectDescription _sourceEntityDescription;
        private List<PropertyMapping<TSource>> _foreachHelper;

        public MappingCollection(ObjectDescription targetEntityDescription)
            : this(ObjectDescription.Get(typeof(TSource)), targetEntityDescription)
        {
        }

        public MappingCollection(ObjectDescription sourceEntityDecDescription, ObjectDescription targetEntityDescription)
        {
            _sourceEntityDescription = sourceEntityDecDescription;
            _targetEntityDescription = targetEntityDescription;
            Init();
        }

        private void Init()
        {
            _properties = new PropertyMapping<TSource>[_targetEntityDescription.Properties.Count];
            _propertiesDic = new Dictionary<string, PropertyMapping<TSource>>(_targetEntityDescription.Properties.Count);
        }

        internal ObjectDescription SourceEntityDescription
        {
            get { return _sourceEntityDescription; }
        }

        internal ObjectDescription TargetEntityDescription
        {
            get { return _targetEntityDescription; }
        }

        private PropertyMapping<TSource> this[int key]
        {
            get
            {
                if ( _properties[key] == null )
                {
                    _properties[key] = new PropertyMapping<TSource>(_sourceEntityDescription, TargetEntityDescription.Properties[key]);
                }
                return _properties[key];
            }
        }

        public PropertyMapping<TSource> this[PropertyDescription property]
        {
            get { return this[property.Index]; }
        }


        public PropertyMapping<TSource> this[string key]
        {
            get { return this[TargetEntityDescription.Properties[key].Index]; }
        }

        public IEnumerator<PropertyMapping<TSource>> GetEnumerator()
        {
            if ( _foreachHelper == null )
            {
                _foreachHelper = new List<PropertyMapping<TSource>>();
                foreach ( var mapping in _properties )
                    if ( mapping != null )
                        _foreachHelper.Add(mapping);
            }
            return _foreachHelper.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Clear()
        {
            Init();
        }
    }
}