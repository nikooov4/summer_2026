using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
	[Serializable]
    public class EntityCache : Cache<object, IEntity>, IEntityCache
	{
		private readonly EntityDescription _entityDescription;
		//private Dictionary<object, IEntity> _cache = new Dictionary<object, IEntity>();
		private readonly Indexes _rawIndexes = new Indexes();
	    private Mapper _mapper;

	    public EntityCache(EntityDescription entityDescription)
		{
			_entityDescription = entityDescription;
		}

		public EntityDescription Type
		{
			get
			{
				return _entityDescription;
			}
		}

		public Indexes RawIndexes
		{
			get
			{
				return _rawIndexes;
			}
		}

		//public int Count
		//{
		//    get
		//    {
		//        return _cache.Count;
		//    }
		//}

		//public bool Contains(object id)
		//{
		//    return _cache.ContainsKey(id);
		//}

		//public void AddProperty(object id, Entity item)
		//{
		//    _cache.AddProperty(id, item);
		//}

        public void Add(IEntity item)
		{
			_cache.Add(item.ID, item);
		    item.EntityIdChanged += (e, args) =>
		                                {
		                                    if ( _cache.Contains(args.NewId) )
		                                        throw new ArgumentException(string.Format("Object error caching error: item's ID changed from '{1}' to '{0}', an object with id '{0}' already exists", args.NewId, args.OldId));
		                                    _cache.Remove(args.OldId);
		                                    _cache.Add(args.NewId, e);
		                                };
			foreach ( string prop in item.Description.SourceRawValues.Keys )
			{
                if (item.Description.SourceRawValues[prop] != null)
                {
                    if (!_rawIndexes[prop].ContainsKey(item.Description.SourceRawValues[prop]))
                        _rawIndexes[prop][item.Description.SourceRawValues[prop]] = new List<IEntity>();
                    _rawIndexes[prop][item.Description.SourceRawValues[prop]].Add(item);
                }
			}
		}

        public void Delete(IEntity item)
		{
            foreach (string prop in item.Description.SourceRawValues.Keys)
            {
                if (item.Description.SourceRawValues[prop] != null)
                {
                    if (_rawIndexes[prop].ContainsKey(item.Description.SourceRawValues[prop]))
                        if (_rawIndexes[prop][item.Description.SourceRawValues[prop]].Contains(item))
                            _rawIndexes[prop][item.Description.SourceRawValues[prop]].Remove(item);
                }
            }
            _cache.Remove(item.ID);
		}

		public void DeleteById(object id)
		{
            if (_cache.Contains(id))
                Delete(_cache[id] as EntityBase);
		}

        public List<IEntity> GetAll()
		{
            var list = new List<IEntity>();
            foreach (IEntity e in _cache.Values)
				list.Add(e);
			return list;
		}

        public List<T> GetAll<T>() where T : IEntity
		{
			var list = new List<T>();
			foreach ( T e in _cache.Values )
				list.Add(e);
			return list;
		}

        public IEntity GetById(object id)
		{
            return (IEntity)_cache[id];
		}

        public T GetById<T>(object id) where T : IEntity
		{
			return (T)_cache[id];
		}

		public void Refresh(ILoader loader)
		{
			this._cache.Clear();
			foreach ( var entity in loader.LoadAll() )
			{
				//if (!_
				_cache.Add(entity.ID, entity);
			}
		}

        public void Reload(List<IEntity> list)
		{
			_cache.Clear();
            foreach (var e in list)
				_cache.Add(e.ID, e);
		}

		public bool ContainsElements(string propertyName, object value)
		{
		    return RawIndexes[_mapper[propertyName]].ContainsKey(value);
		}

	    public List<IEntity> GetEntitiesByRawFieldValues(string propertyName, object value)
		{
			if ( RawIndexes[_mapper[propertyName]].ContainsKey(value) )
                return new List<IEntity>(RawIndexes[_mapper[propertyName]][value]);
			else
				return null;
		}

	}

	public class AggregatedEntityCache : IEntityCache
	{
		private readonly EntityDescription _type;
		private readonly IEntityCache[] _caches;
		private EntityDescription[] _innerTypes;
		private readonly Dictionary<EntityDescription, IEntityCache> _cachesByType = new Dictionary<EntityDescription, IEntityCache>();

		public AggregatedEntityCache(EntityDescription type, IEntityCache[] caches)
		{
			_type = type;
			_caches = caches;
			_innerTypes = Array.ConvertAll<IEntityCache, EntityDescription>(_caches, c => c.Type);
			foreach ( var c in _caches )
				_cachesByType.Add(c.Type, c);
		}

		#region IEntityCache Members

		public EntityDescription Type
		{
			get
			{
				return _type;
			}
		}

        public void Add(IEntity item)
		{
            if ( _cachesByType.ContainsKey(item.Description.EntityDescriptor) )
				_cachesByType[item.Description.EntityDescriptor].Add(item);
		}

        public void Delete(IEntity item)
		{
			DeleteById(item.ID);
		}

		public void DeleteById(object id)
		{
			foreach ( var c in _caches )
				if ( c.Contains(id) )
					c.DeleteById(id);
		}


        public IEntity GetById(object id)
		{
			foreach ( var c in _caches )
				if ( c.Contains(id) )
					return c[id];
			throw new Exception("Key not fount");
		}

        public T GetById<T>(object id) where T : IEntity
		{
			foreach ( var c in _caches )
				if ( c.Contains(id) )
					return c.GetById<T>(id);
			throw new Exception("Key not fount");
		}

		public void Refresh(ILoader loader)
		{
			foreach ( var c in _caches )
				c.Refresh(loader);
		}

        public void Reload(List<IEntity> list)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region ICache<object,EntityBase> Members

        public IEntity this[object key]
		{
			get
			{
				return GetById(key);
			}
			set
			{
				if ( !_cachesByType.ContainsKey(value.Description.EntityDescriptor) )
					throw new Exception("The value type is incovenient for this cache.");
				if ( Contains(key) )
				{
                    var oldValue = GetById(key);
					if ( oldValue.GetType() == value.GetType() )
                        _cachesByType[value.Description.EntityDescriptor][key] = value;
					else
					{
                        _cachesByType[oldValue.Description.EntityDescriptor].DeleteById(key);
                        _cachesByType[value.Description.EntityDescriptor][key] = value;
					}
				}
				else
                    _cachesByType[value.Description.EntityDescriptor][key] = value;
			}
		}

		public int Count
		{
			get
			{
				int count = 0;
				foreach ( var c in _caches )
					count += c.Count;
				return count;
			}
		}

		public bool Contains(object id)
		{
			foreach ( var c in _caches )
				if ( c.Contains(id) )
					return true;
			return false;
		}

        public void Add(object id, IEntity item)
		{
            if ( _cachesByType.ContainsKey(item.Description.EntityDescriptor) )
				_cachesByType[item.Description.EntityDescriptor].Add(id, item);
		}

        public List<T> GetAll<T>() where T : IEntity
		{
			var list = new List<T>();
			foreach ( var c in _caches )
				foreach ( T e in c )
					list.Add(e);
			return list;
		}


        public List<IEntity> GetAll()
		{
            var list = new List<IEntity>();
			foreach ( var c in _caches )
                foreach (var e in c)
					list.Add(e);
			return list;
		}

		#endregion


		T IEntityCache.GetById<T>(object id)
		{
			return (T)GetById(id);
		}

		public bool ContainsElements(string propertyName, object value)
		{
			foreach(IEntityCache cache in _caches)
				if(cache.ContainsElements(propertyName, value))
					return true;
			return false;
		}

        public List<IEntity> GetEntitiesByRawFieldValues(string propertyName, object value)
		{
            var list = new List<IEntity>();
			foreach ( var cache in _caches )
				if ( cache.ContainsElements(propertyName, value) )
					list.AddRange(cache.GetEntitiesByRawFieldValues(propertyName, value));
			if ( list.Count == 0 )
				return null;
			return list;
		}

		#region ICache<object,EntityBase> Members


		public object[] Keys
		{
			get
			{
				int count = 0;
				foreach ( IEntityCache c in _caches )
					count += c.Keys.Length;
				object[] keys = (object[])Array.CreateInstance(typeof(object), count);
				int counter = 0;
				foreach ( IEntityCache c in _caches )
				{
					c.Keys.CopyTo(keys, counter);
					counter += c.Keys.Length - 1;
				}
				return keys;
			}
		}

        public IEntity[] Values
		{
			get
			{
				int count = 0;
				foreach ( IEntityCache c in _caches )
					count += c.Values.Length;
                IEntity[] values = (IEntity[])Array.CreateInstance(typeof(IEntity), count);
				int counter = 0;
				foreach ( IEntityCache c in _caches )
				{
					c.Values.CopyTo(values, counter);
					counter += c.Values.Length - 1;
				}
				return values;
			}
		}

		#endregion

        public IEnumerator<IEntity> GetEnumerator()
		{
			return new AggregatedEntityCacheEnumerator(this);
		}

        public class AggregatedEntityCacheEnumerator : IEnumerator<IEntity>
		{
			private readonly AggregatedEntityCache _cache;
            private IEnumerator<IEntity> _currentEnumerator;
			private int _currentCacheNumber = -1;

			public AggregatedEntityCacheEnumerator(AggregatedEntityCache cache)
			{
				_cache = cache;
			}

			#region IEnumerator<EntityBase> Members

            public IEntity Current
			{
				get
				{
					return _currentEnumerator.Current;
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get
				{
					return _currentEnumerator.Current;
				}
			}

			public bool MoveNext()
			{
				if ( !_currentEnumerator.MoveNext() )
				{
					if ( ++_currentCacheNumber < _cache._caches.Length )
					{
						_currentEnumerator = _cache._caches[_currentCacheNumber].GetEnumerator();
						_currentEnumerator.Reset();
						return true;
					}
					else
						return false;
				}
				return true;

			}

			public void Reset()
			{
				_currentEnumerator = _cache._caches[0].GetEnumerator();
				_currentEnumerator.Reset();
				_currentCacheNumber = 0;
			}

			#endregion
		}
	}
}
