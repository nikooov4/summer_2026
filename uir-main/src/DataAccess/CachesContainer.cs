using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using System.Collections.Specialized;

namespace Mephi.Cybernetics.Race.DataAccess
{
	[Serializable]
	public class CachesContainer
	{
        private static class EntityChacheFactory
        {
            public static IEntityCache CreateEntityCache(CachesContainer container, EntityDescription ed)
            {
                if (ed.IsAggregated)
                {
                    return new AggregatedEntityCache(ed, Array.ConvertAll<EntityDescription, IEntityCache>(ed.AggregatedTypes, ed2 => container[ed2]));
                }
                else
                    return new EntityCache(ed);
            }
        }


        private readonly HybridDictionary _caches = new HybridDictionary();
		private readonly List<EntityDescription> _keys = new List<EntityDescription>();

		public CachesContainer()
		{
		}

		public EntityDescription[] Keys
		{
			get { return _keys.ToArray(); }
		}

		public IEntityCache this[EntityDescription type]
		{
			get
			{
				if ( !_caches.Contains(type) )
				{
					IEntityCache cache = EntityChacheFactory.CreateEntityCache(this, type); //new EntityCache(ed);
					_caches.Add(type, cache);
					_keys.Add(type);
				}
				return (IEntityCache)_caches[type];
			}
		}

        public List<IEntity> GetEntities(EntityDescription ed)
		{
			if ( this[ed] == null )
			{
			    var attr = ed.Object.GetClrAttribute<EntityCacheTypeAttribute>();
			    _caches.Add(ed, EntityChacheFactory.CreateEntityCache(this, ed));
//			        attr == null
//			            ? EntityChacheFactory.CreateEntityCache(this, ed)
//			            : EntityChacheFactory.CreateEntityCache(this, attr.EntityCacheType));
			}
            //List<Entity> list = new List<Entity>();
			return this[ed].GetAll().FindAll(e => !(e.Description.State == EntityState.Deleted || e.Description.State == EntityState.NewDeleted));
			//return list;
		}

		public List<T> GetEntities<T>() where T : IEntity
		{
		    var ed = EntityDescription.GetEntityDescription<T>();
			if ( this[ed] == null )
			{
//                var attr = EntityDescription.GetEntityDescription(type).Object.GetClrAttribute<EntityCacheTypeAttribute>();
//				if ( attr == null )
					_caches.Add(ed, EntityChacheFactory.CreateEntityCache(this, ed) /*new EntityCache(ed)*/);
//				else
//					_caches.Add(type, EntityChacheFactory.CreateEntityCache(this, attr.EntityCacheType)/*new EntityCache(attr.EntityCacheType)*/);
			}
            return this[ed].GetAll<T>().FindAll(e => !(e.Description.State == EntityState.Deleted || e.Description.State == EntityState.NewDeleted));
		}

        public IEntity GetEntityById(Type type, object id)
		{
			return this[EntityDescription.GetEntityDescription(type)].GetById(id);
		}

	    public IEntity GetEntityById(EntityDescription ed, object id)
	    {
	        return this[ed].GetById(id);
	    }

        public void RemoveFromCache(IEntity e)
        {
            (_caches[e.GetType()] as IEntityCache).Delete(e);
        }

        public T GetEntityById<T>(object id) where T : IEntity
        {
            var ed = EntityDescription.GetEntityDescription<T>();
			return this[ed].GetById<T>(id);
		}

//		public void Refresh(ILoader loader)
//		{
//			if ( !_caches.Contains(loader.ReflectedEntityDescription) )
//				_caches.Add(loader.ReflectedEntityDescription, 
//					EntityChacheFactory.CreateEntityCache(this,  loader.ReflectedEntityDescription));
//					/*new EntityCache(loader.ReflectedEntityDescription));*/
//			(_caches[loader.ReflectedEntityDescription] as EntityCache).Refresh(loader);
//		}

		public void Clear()
		{
			_caches.Clear();
		}

	    public bool Contains(IEntity entity)
	    {
            if (_caches.Contains(entity.GetType()))
                return (_caches[entity.GetType()]as EntityCache).Contains(entity);
            else
                return false;
	    }
	}
}
