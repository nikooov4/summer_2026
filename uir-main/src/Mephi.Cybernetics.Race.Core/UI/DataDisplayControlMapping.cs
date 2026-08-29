using System;
using System.Collections;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.TypeSystem;

namespace Mephi.Cybernetics.Race.Core.UI
{
    public class DataDisplayControlMapping : ICollection<KeyValuePair<IDomain, Type>>, IReadOnlyCollection<KeyValuePair<IDomain, Type>>
    {
        private readonly Dictionary<IDomain,Type> _mapping = new Dictionary<IDomain, Type>();//где IDomain должно быть IEntity
        private IStrictTypeSystem _typeProvider;

        public DataDisplayControlMapping(IStrictTypeSystem typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public DataDisplayControlMapping(IStrictTypeSystem typeProvider, Dictionary<IDomain,Type> mapping)
        {
            _typeProvider = typeProvider;
            _mapping = new Dictionary<IDomain, Type>(mapping);
        }

        // Пока с явным кастом к ObjectDescription. Это костыль.
        // По идее, может быть другая реализация IDomen-а, и в момент вызова мы должны точно знать, какая
        public bool HasControlFor(IDomain domain)
        {
            foreach (var storedDomain in _mapping.Keys)
            {
                if (((ObjectDescription)domain).IsSubtypeOf(ObjectDescription.Get(storedDomain)))
                    return true;
            }
            return false;
        }

        public bool HasControlFor<T>()
        {
            foreach(var storedDomain in _mapping.Keys)
            {
                if (ObjectDescription.Get(typeof(T)).IsSubtypeOf((ObjectDescription)storedDomain))
                    return true;
            }
            return false;
        }
  
        public bool HasControlFor(Type objectType)
        {
            foreach (var storedDomain in _mapping.Keys)
            {
                if (ObjectDescription.Get(objectType).IsSubtypeOf((ObjectDescription)storedDomain))
                    return true;
            }
            return false;
        }

        public object GetControlFor(IDomain domain)
        //TODO добавить возможность учитывать ApplicationArea при выборе control
        {
            var type = _mapping[domain];
            var control =  Activator.CreateInstance(type);
            return control;
        } 

        /// <summary>
        /// Добавление в словарь контрола для типа T
        /// </summary>
        public DataDisplayControlMapping For<T>(Type ctrlType)
        {
            var newDictionary = new Dictionary<IDomain,Type>(_mapping);
            newDictionary[ObjectDescription.Get<T>()] = ctrlType;
            return new DataDisplayControlMapping(_typeProvider, newDictionary);
        }

        public IEnumerator<KeyValuePair<IDomain, Type>> GetEnumerator() => _mapping.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<IDomain, Type> item) => _mapping.Add(item.Key, item.Value);

        public void Clear() => _mapping.Clear();

        public bool Contains(KeyValuePair<IDomain, Type> item) => _mapping.ContainsKey(item.Key);

        public void CopyTo(KeyValuePair<IDomain, Type>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array is null");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex is negative");
            if (_mapping.Count > array.Length - arrayIndex)
                throw new ArgumentException("not enough space from arrayIndex to put all elements");

            foreach (KeyValuePair<IDomain, Type> entry in _mapping)
                array[arrayIndex++] = entry;
        }

        public bool Remove(KeyValuePair<IDomain, Type> item) => _mapping.Remove(item.Key);

        public int Count => _mapping.Count;

        // Нужен ли?
        public bool IsReadOnly => throw new NotImplementedException();
    }
}