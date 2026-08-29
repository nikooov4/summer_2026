using System;
using System.Collections.Generic;
using System.Reflection;
using Mephi.Cybernetics.Race.Core.TypeSystem;

namespace Mephi.Cybernetics.Race.Core
{
    public class PropertyDescriptionCollectionBase<T> : IDomainAttributeCollection where T : IDomainAttribute
    {
        protected readonly Dictionary<string, T> _byName = new Dictionary<string, T>();
        protected readonly Dictionary<int, T> _byIndex = new Dictionary<int, T>();
        protected readonly Dictionary<PropertyInfo, T> _byPropInfo = new Dictionary<PropertyInfo, T>();
        protected readonly List<T> _items = new List<T>();

        //TODO: reset to internal
        public PropertyDescriptionCollectionBase()
        {
        }
        
        //TODO: reset to internal
        public virtual void AddProperty(T property)
        {
            _byName.Add(property.Name, property);
            _items.Add(property);
        }

        /// <summary>
        /// Replace an already existing property with the given one, provided they share the same name and index
        /// </summary>
        /// <param name="property"></param>
        //TODO: reset to internal
        public virtual void Replace(T property)
        {
            var oldProperty = _byName[property.Name];
            _items.Remove(oldProperty);
            _items.Add(property);
            _byName[property.Name] = property;
        }

        public virtual T this[string name] => _byName[name];
        IDomainAttribute IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.this[string name] => _byName[name];

        public IDomain ReflectedDomain => throw new NotImplementedException();

        public virtual T this[int index] => _byIndex[index];
        IDomainAttribute IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.this[int index] => _byIndex[index];

        public virtual T this[PropertyInfo prop] => _byPropInfo[prop];

        public virtual int Count => _items.Count;

        public virtual bool Contains(PropertyInfo propertyInfo)
        {
            return _byPropInfo.ContainsKey(propertyInfo);
        }

        public virtual bool Contains(string name)
        {
            return _byName.ContainsKey(name);
        }

        public virtual bool Contains(T property)
        {
            return _items.Contains(property);
        }

        IEnumerator<IDomainAttribute> IEnumerable<IDomainAttribute>.GetEnumerator()
        {
            return new PropertyDescriptionCollectionEnumerator(this) as IEnumerator<IDomainAttribute>;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return new PropertyDescriptionCollectionEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new PropertyDescriptionCollectionEnumerator(this);
        }

        public class PropertyDescriptionCollectionEnumerator : IEnumerator<T>
        {
            private readonly PropertyDescriptionCollectionBase<T> _owner;
            private int _index = -1;

            internal PropertyDescriptionCollectionEnumerator(PropertyDescriptionCollectionBase<T> owner)
            {
                _owner = owner;
            }

            public T Current
            {
                get
                {
                    if ( _index < 0 )
                        return _owner._items[0];
                    return _owner._items[_index];
                }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if ( _index < 0 )
                        return _owner._items[0];
                    return _owner._items[_index];
                }
            }

            public bool MoveNext()
            {
                if ( _index < _owner.Count - 1 )
                    _index++;
                else
                    return false;
                return true;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }

    public class PropertyDescriptionCollection : PropertyDescriptionCollectionBase<PropertyDescription>
    {

        internal PropertyDescriptionCollection()
        {
        }

        public override void AddProperty(PropertyDescription property)
        {
            base.AddProperty(property);
            _byIndex.Add(property.Index, property);
            _byPropInfo.Add(property.Property, property);
        }

        public override void Replace(PropertyDescription property)
        {
            var oldProperty = _byName[property.Name];
            if (!(oldProperty.Index == property.Index && oldProperty.Name == property.Name))
                throw new ArgumentException("The replacing property must have exactly the same name and index as the property being replaced");
            _items.Remove(oldProperty);
            _items.Add(property);
            _byName[property.Name] = property;
            _byPropInfo.Remove(oldProperty.Property);
            _byPropInfo.Add(property.Property, property);
            _byIndex[property.Index] = property;
        }
    }

//    public class RelationshipDescriptionCollection : PropertyDescriptionCollectionBase<EntityAttributeDescription>
//    {
//        internal RelationshipDescriptionCollection(ObjectDescription owner)
//            : base(owner)
//        {
//        }
//    }


}
