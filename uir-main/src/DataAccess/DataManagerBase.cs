using System;
using System.Collections.Generic;
using System.Reflection;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using Mephi.Cybernetics.Race.DataAccess.Predicates;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public abstract class DataManagerBase : MarshalByRefObject, IDataManager
    {
        protected StorageAccessMediator _storageAccessMediator;
        protected bool _loadAllOnGetById = true;
        protected FullyRecursiveDataSaveStrategy _dataSaveStrategy = null;
        protected EntityFactory _entityFactory;
        protected List<EntityEditingContext> _contexts = new List<EntityEditingContext>();

        //protected abstract IMephi.Cybernetics.Race.DataAccessProvider Mephi.Cybernetics.Race.DataAccessProvider { get; }

        [Obsolete("Такого свойства вообще не должно быть у DataManager")]
        public abstract CachesContainer Caches { get; }

        public abstract IList<IEntity> GetEntities(Type type);

        public EntityList<T> GetEntities<T>(Predicate<T> where) where T : IEntity
        {
            var list = new EntityList<T>();
            foreach ( T element in GetEntities<T>() )
                if ( where(element) )
                    list.Add(element);
            return list;
        }

        public abstract List<T> GetEntities<T>() where T : IEntity;

        public IEntity GetOneEntityByPredicate(Type type, UnaryOperation<IEntity> op)
        {
            var list = GetEntitiesByPredicate(type, op);
            if (list.Count == 0)
                return null;
            else
                return list[0];
        }

        public T GetOneEntityByPredicate<T>(UnaryOperation<T> op) where T : IEntity
        {
            var list = GetEntitiesByPredicate(op);
            if (list.Count == 0)
                return default(T);
            else
                return list[0];
        }

        public List<IEntity> GetEntitiesByPredicate(Type type, UnaryOperation<IEntity> op)
        {
            return Functions.Select<IEntity>(GetEntities(type), op);
        }

        public List<T> GetEntitiesByPredicate<T>(UnaryOperation<T> op) where T : IEntity
        {
            return Functions.Select<T>(GetEntities<T>(), op);
        }

        public List<IEntity> GetEntitiesByFieldValue(Type type, string fieldName, object value)
        {
            UnaryOperation<IEntity> op = delegate(IEntity e)
                                                {
                                                    if (e[fieldName] != null)
                                                    {
                                                        return e[fieldName].Equals(value);
						
                                                    }
                                                    else
                                                        return value == null;
                                                };
            return Functions.Select<IEntity>(GetEntities(type), op);
        }

        public List<T> GetEntitiesByFieldValue<T>(string fieldName, object value) where T : IEntity
        {
            //UnaryOperation<T> op = e => e != null && e[fieldName] != null ? e[fieldName].Equals(value) : value == null;
            var dt1 = DateTime.Now;
            var list = GetEntities<T>();
            var dt2 = DateTime.Now;
            //var result = Functions.Select<T>(list, op);
            var result = new List<T>(list.Count / 4);
            var property = EntityDescription.GetEntityDescription(typeof (T)).EntityAttributes[fieldName];
            foreach ( var element in list )
            {
                if ( element != null && property.GetValue(element) != null ? property.GetValue(element).Equals(value) : value == null) 
                    result.Add(element);
            }
            var dt3 = DateTime.Now;
            var ts1 = (dt2 - dt1).TotalMilliseconds;
            var ts2 = (dt3 - dt2).TotalMilliseconds;
            return result;
        }

        public T GetOneEntityByFieldValue<T>(string fieldName, object value) where T : IEntity
        {
            var property = EntityDescription.GetEntityDescription(typeof(T)).EntityAttributes[fieldName];
            T el =	Functions.SelectFirst(GetEntities<T>(), e => e != null && property.GetValue(e) != null && property.GetValue(e).Equals(value));
            return el;
        }

        public T GetEntityById<T>(object id) where T : IEntity
        {
            var en = GetEntityById(typeof(T), id);
            return (T)en;
        }

        public abstract IEntity GetEntityById(Type type, object id);

        public List<IEntity> GetReferencingEntities(IEntity entity)
        {
            throw new NotImplementedException("Method is not implemented");
        }

        public List<IEntity> GetReferencingEntites(IEntity entity, Type restriction)
        {
            throw new NotImplementedException("Method is not implemented");
        }

        public List<IEntity> GetReferencedEntities(IEntity entity)
        {
            return GetReferencedEntites(entity, null);
        }

        public List<IEntity> GetReferencedEntites(IEntity entity, Type restriction)
        {
            var od = EntityDescription.GetEntityDescription(entity.GetType());
            var result = new List<IEntity>();
            foreach (var p in od.EntityAttributes.Relationships)
            {
                var value = p.GetValue(entity);
                if (p.IsOneToOneRelation)
                {
                    var valueOd = _entityFactory.GetObjectDescription(value.GetType());
                    if (restriction != null && valueOd.Object.IsSubtypeOf(restriction))
                        result.Add(value as EntityBase);
                }
                else
                    foreach (var e in p.GetValue(entity) as System.Collections.IEnumerable)
                    {
                        EntityDescription eOd = _entityFactory.GetObjectDescription(e.GetType());
                        if (restriction != null && eOd.Object.IsSubtypeOf(restriction))
                            result.Add(e as EntityBase);
                    }
            }
            return result;
        }

        public List<T> GetReferencedEntities<T>(IEntity entity) where T : IEntity
        {
            var od = EntityDescription.GetEntityDescription(entity.GetType());
            var result = new List<T>();
            foreach (var p in od.EntityAttributes.Relationships)
            {
                object value = p.GetValue(entity);
                if (value == null)
                    continue;
                if (p.IsOneToOneRelation)
                {
                    EntityDescription valueOd = _entityFactory.GetObjectDescription(value.GetType());
                    if (valueOd.Object.IsSubtypeOf(typeof(T)))
                        result.Add((T)value);
                }
                else
                    foreach (IEntity e in p.GetValue(entity) as System.Collections.IEnumerable)
                    {
                        EntityDescription eOd = _entityFactory.GetObjectDescription(e.GetType());
                        if (eOd.Object.IsSubtypeOf(typeof(T)))
                            result.Add((T)e);
                    }
            }
            return result;
        }

        public void ForAll(UnaryOperation<IEntity> predicate, PersistentStorage.EntityProc proc)
        {
            foreach (var t in Caches.Keys)
            {
                foreach (EntityBase e in Caches[t].Values)
                    if ( predicate(e) )
                        proc(e);
            }
        }

        public List<IEntity> Select(UnaryOperation<IEntity> predicate)
        {
            var list = new List<IEntity>();
            foreach (var t in Caches.Keys)
            {
                foreach (EntityBase e in Caches[t].Values)
                    if (predicate(e))
                        list.Add(e);
            }
            return list;
        }

        public abstract void Save(IEntity entity);

        public abstract void Save(List<IEntity> entities);

        [Obsolete]
        public abstract void AddEntity(IEntity entity);

        [Obsolete("По-видимому, метод необходимо ликвидировать")]
        public abstract void Delete(IEntity e);

    }
}