using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

namespace Mephi.Cybernetics.Race.DataAccess
{
	[Serializable]
    public class Cache<TIdType, TValueType> : ICache<TIdType, TValueType>, IEnumerable<TValueType> where TValueType : IEntity 
	{
        protected HybridDictionary _cache = new HybridDictionary();
		

		public Cache()
		{
		}

		public int Count
		{
			get
			{
				return _cache.Count;
			}
		}

		public TIdType[] Keys
		{
			get
			{
				var arr = new TIdType[_cache.Keys.Count];
				_cache.Keys.CopyTo(arr, 0);
				return arr;
			}
		}

		public TValueType[] Values
		{
			get
			{
				var arr = new TValueType[_cache.Values.Count];
				_cache.Values.CopyTo(arr, 0);
				return arr;
			}
		}

		public TValueType this[TIdType key]
		{
			get
			{
				//if ( _cache.ContainsKey(key) )
					return (TValueType)_cache[key];
				//else
				//    return default(ValueType);
			}
			set
			{
				_cache[key] = value;
			}
		}

		public bool Contains(TIdType id)
		{
			return _cache.Contains(id);
		}

        public bool ContainsElement(TValueType item)
        {
            foreach (var value in _cache.Values)
            {
                if (value.Equals(item))
                    return true;
            }
            return false;
        }

	    public void Add(TIdType id, TValueType item)
		{
            if (!_cache.Contains(id))
			_cache.Add(id, item);
		}

		public void Remove(TIdType id)
		{
			_cache.Remove(id);
		}

        public List<TValueType> GetAll() 
		{
			var l = new List<TValueType>();
			foreach ( TValueType v in _cache.Values )
				l.Add(v);
			return l;
		}

		public void Clear()
		{
			_cache.Clear();
		}

		#region IEnumerable<ValueType> Members

		public IEnumerator<TValueType> GetEnumerator()
		{
			return new CacheEnumerator(this);
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new CacheEnumerator(this);
		}

		#endregion

		public class CacheEnumerator : IEnumerator<TValueType> 
		{
			private readonly Cache<TIdType, TValueType> _theCache;
			private IDictionaryEnumerator _enumerator;

			public CacheEnumerator(Cache<TIdType, TValueType> c)
			{
				_theCache = c;
			}

			#region IEnumerator<ValueType> Members

			public TValueType Current
			{
				get
				{
					return (TValueType)_enumerator.Current;
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				_enumerator = null;
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return _enumerator.Current;
				}
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator = _theCache._cache.GetEnumerator();
			}

			#endregion
		}
	}
}
