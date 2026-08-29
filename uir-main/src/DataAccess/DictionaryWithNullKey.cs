using System;
using System.Collections;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public class DictionaryWithNullKey<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        private Dictionary<TKey, TValue> notNullKeysValues = new Dictionary<TKey, TValue>();
        private TValue _nullKeyValue;
        private bool _nullKeyValueSet = false;

        public DictionaryWithNullKey()
        {
            
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new DictionaryWithNullKeyEnumerator(this);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null)
            {
                if (_nullKeyValueSet)
                    throw new ArgumentException("Key already exists");
                else
                {
                    _nullKeyValueSet = true;
                    _nullKeyValue = item.Value;
                }
            }
            else
                notNullKeysValues.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _nullKeyValueSet = false;
            _nullKeyValue = null;
            notNullKeysValues.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null)
            {
                if (!_nullKeyValueSet)
                    return false;
                else
                    return _nullKeyValue == item.Value;
            }
            else
                return notNullKeysValues.ContainsKey(item.Key) && notNullKeysValues[item.Key] == item.Value;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var index = arrayIndex;
            if (_nullKeyValueSet)
            {
                array[arrayIndex] = new KeyValuePair<TKey, TValue>(null, _nullKeyValue);
                index++;
            }
            foreach ( var value in notNullKeysValues )
            {
                array[index++] = value;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null)
            {
                var result = _nullKeyValueSet;
                _nullKeyValueSet = false;
                _nullKeyValue = null;
                return result;
            }
            else
                return (notNullKeysValues as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }

        public int Count
        {
            get
            {
                if (_nullKeyValueSet)
                    return notNullKeysValues.Count + 1;
                else
                    return notNullKeysValues.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null)
                return _nullKeyValueSet;
            else
                return notNullKeysValues.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                _nullKeyValue = value;
                _nullKeyValueSet = true;
            }
            else
                notNullKeysValues.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                if (!_nullKeyValueSet)
                    return false;
                else
                {
                    _nullKeyValueSet = false;
                    _nullKeyValue = null;
                    return true;
                }
            }
            return notNullKeysValues.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                if (_nullKeyValueSet)
                {
                    value = _nullKeyValue;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
            else
                return notNullKeysValues.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    if (_nullKeyValueSet)
                        return _nullKeyValue;
                    else
                        throw new ArgumentException();
                }
                else
                    return notNullKeysValues[key];
            }
            set
            {
                if (key == null)
                {
                    _nullKeyValueSet = true;
                    _nullKeyValue = value;
                }
                else
                    notNullKeysValues[key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new List<TKey>();
                if (_nullKeyValueSet)
                    keys.Add(null);
                keys.AddRange(notNullKeysValues.Keys);
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>();
                if (_nullKeyValueSet)
                    values.Add(_nullKeyValue);
                values.AddRange(notNullKeysValues.Values);
                return values;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class DictionaryWithNullKeyEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly DictionaryWithNullKey<TKey, TValue> _dictionaryWithNullKey;
            private bool _cuurentNull = true;
            private Dictionary<TKey, TValue>.Enumerator _dicCursor;
            private bool _started = false;

            public DictionaryWithNullKeyEnumerator(DictionaryWithNullKey<TKey, TValue> dictionaryWithNullKey)
            {
                _dictionaryWithNullKey = dictionaryWithNullKey;
                _dicCursor = dictionaryWithNullKey.notNullKeysValues.GetEnumerator();
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (!_started)
                {
                    _started = true;
                    return _cuurentNull && _dictionaryWithNullKey._nullKeyValueSet;
                }
                else
                    return _dicCursor.MoveNext();
            }

            public void Reset()
            {
                _cuurentNull = true;
                _dicCursor = _dictionaryWithNullKey.notNullKeysValues.GetEnumerator();
                _started = false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (_cuurentNull && _dictionaryWithNullKey._nullKeyValueSet)
                        return new KeyValuePair<TKey, TValue>(null, _dictionaryWithNullKey._nullKeyValue);
                    else
                        return _dicCursor.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
        }

    }

}