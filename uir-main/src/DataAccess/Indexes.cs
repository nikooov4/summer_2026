using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Collections.Specialized;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public class Indexes
	{
		private readonly HybridDictionary _indexes = new HybridDictionary();
		private readonly List<object> _properties = new List<object>();

		public Indexes()
		{
		}

		public object[] Keys
		{
			get
			{
				return _properties.ToArray();
			}
		}

        public IDictionary<object, List<IEntity>> this[string propertyName]
		{
			get
			{
				if ( !_indexes.Contains(propertyName) )
				{
                    IDictionary<object, List<IEntity>> index = new DictionaryWithNullKey<object, List<IEntity>>();
					_indexes.Add(propertyName, index);
					_properties.Add(propertyName);
				}
                return (IDictionary<object, List<IEntity>>)_indexes[propertyName];
			}
		}

        public List<IEntity> GetEntities(string propertyName, object key)
		{
			return this[propertyName][key];
		}
	}

	public class IndexSet<T> where T : IEntity
	{
		//private Dictionary<string, Dictionary<object, List<T>>> _indexes = new Dictionary<string, Dictionary<object, List<T>>>();
        private Dictionary<string, DictionaryWithNullKey<object, List<T>>> _indexes = new Dictionary<string, DictionaryWithNullKey<object, List<T>>>();
        private string[] _indexedFiledsNames;

		public IndexSet(string[] fieldsNames)
		{
			_indexedFiledsNames = fieldsNames;
			if ( _indexedFiledsNames == null || _indexedFiledsNames.Length == 0 )
				return;
			foreach ( string s in _indexedFiledsNames )
				_indexes.Add(s, new DictionaryWithNullKey<object, List<T>>());
		}

		public void Clear()
		{
			foreach ( string s in _indexedFiledsNames )
				_indexes[s].Clear();
		}

		public void Add(T e)
		{
            foreach (string s in _indexedFiledsNames)
            {
                
                //if (e[s] != null)
                {
                    if (_indexes[s].ContainsKey(e[s]))
                        _indexes[s][e[s]].Add(e);
                    else
                    {
                        _indexes[s][e[s]] = new List<T>();
                        _indexes[s][e[s]].Add(e);
                    }
                }
            }
		}

		public void Delete(T e)
		{
			foreach ( string s in _indexedFiledsNames )
				if ( _indexes[s].ContainsKey(e[s]) )
					if ( _indexes[s][e[s]].Contains(e) )
						_indexes[s][e[s]].Remove(e);
		}

		public IDictionary<object, List<T>> this[string fieldName]
		{
			get
			{
				return _indexes[fieldName];
			}
		}
	}
}
