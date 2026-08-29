using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.AccessControl;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess
{
    //TODO: Убрать поддержку интерфейса IDataSourceManager и ликвидировать соот. методы
    /// <summary>
    /// Отвечает за загрузку данных из источников, в т.ч. множественных, и, при необходимости, кеширование загруженных данных
    /// </summary>
    public class CachableDataProvider : IDataAccessProvider, IDataSourceManager, IDisposable
    {
        private CachesContainer _caches;
        //private Dictionary<Type, ILoader> _loaders = new Dictionary<Type, ILoader>();
        //private Dictionary<Type, ISaver> _savers = new Dictionary<Type, ISaver>();
        private Dictionary<Type, Cache<object, IEntity>> _underLoading = new Dictionary<Type, Cache<object, IEntity>>();
        private readonly List<DataSource> _dataSources = new List<DataSource>();
        private readonly Dictionary<EntityDescription, List<DataSource>> _unreadDataSources = new Dictionary<EntityDescription, List<DataSource>>();
        private readonly List<EntityDescription> _loadedTypes = new List<EntityDescription>();
        private DataSource _defaultDataSource;
        private StorageAccessMediator _accessMediator;
        private bool _loadAllOnGetById;

        //protected EntityFactory _entityFactory;

        public CachableDataProvider()
        {
            Init();
        }

        private void Init()
        {
            _caches = new CachesContainer();
            _accessMediator = new XmlStorageAccessMediator(this);
            _loadAllOnGetById = true;
        }

        public CachesContainer Caches
        {
            get { return _caches; }
        }

        public bool LoadAllOnGetById
        {
            get { return _loadAllOnGetById; }
        }

        #region IMephi.Cybernetics.Race.DataAccessProvider
        public IList<T> GetEntities<T>() where T : IEntity
        {
            var type = typeof(T);
            var objDescr = EntityDescription.GetEntityDescription(type);
            if (!objDescr.IsAggregated)
            {
                if (!_unreadDataSources.ContainsKey(objDescr))
                    _unreadDataSources.Add(objDescr, new List<DataSource>(GetDataSources(objDescr)));
                var list = Caches.GetEntities<T>();
                if ((list == null || list.Count == 0) && !_loadedTypes.Contains(objDescr))
                {
                    //stub-it
                    ////lock (this)
                    //{
                    if (objDescr.IsVirtual)
                    {
                        var l = (List<IEntity>)type.GetMethod(objDescr.GetAllMethodName,
                                           BindingFlags.Static).Invoke(null, new object[] { this });
                        list = l.ConvertAll<T>(e => (T) e);
                    }
                    else
                    {
                        if (list == null)
                            list = new List<T>();
                        foreach (var dataSource in GetDataSources(objDescr))
                        {
                            if (_unreadDataSources.ContainsKey(objDescr) && _unreadDataSources[objDescr].Contains(dataSource))
                                _unreadDataSources[objDescr].Remove(dataSource);
                            list.AddRange(dataSource.GetLoader(objDescr).LoadAll<T>());
                        }
                    }
                    _loadedTypes.Add(objDescr);
                    //}
                }
                if (_unreadDataSources.ContainsKey(objDescr) && _unreadDataSources[objDescr].Count > 0)
                {
                    if (list == null)
                        list = new List<T>();
                    var dataSources = new List<DataSource>(_unreadDataSources[objDescr]);
                    foreach (var dataSource in dataSources)
                    {
                        _unreadDataSources[objDescr].Remove(dataSource);
                        list.AddRange(dataSource.GetLoader(objDescr).LoadAll<T>());
                    }
                }
                if (list == null || list.Count == 0)
                    list = new List<T>();
                return list;
            }
            else
            {
                IList<T> l = new List<T>();
                foreach (var t in objDescr.AggregatedTypes)
                {
                    IList<IEntity> templist = GetEntities(t.Object.ClrType);
                    if (templist != null && templist.Count > 0)
                        foreach (T ee in templist)
                            l.Add(ee);
                }
                return l;
            }
        }

        public IList<T> GetEntities<T>(string propertyName, object value) where T : IEntity
        {
            throw new NotImplementedException();
        }

        public IList<T> GetEntities<T>(string propertyName, object min, object max) where T : IEntity
        {
            throw new NotImplementedException();
        }

        public T GetEntityBiId<T>(object id)
        {
            throw new NotImplementedException();
        }

        public IList<IEntity> GetEntities(Type type)
        {
            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new ArgumentException(string.Format("Type '{0}' is not compatible with entityDescription 'IEntity'", type.Name));
            return GetEntities(EntityDescription.GetEntityDescription(type));
        }

        public IList<IEntity> GetEntities(EntityDescription entityDescription)
        {
            if (!entityDescription.IsAggregated)
            {
                if (!_unreadDataSources.ContainsKey(entityDescription))
                    _unreadDataSources.Add(entityDescription, new List<DataSource>(GetDataSources(entityDescription)));
                //ILoader loader = GetLoader(entityDescription);
                var list = Caches.GetEntities(entityDescription);
                if ((list == null || list.Count == 0) && !_loadedTypes.Contains(entityDescription))
                {
                    // stub-it
                    //lock (this)
                    //{
                    if ( entityDescription.IsVirtual )
                        list =
                            (List<IEntity>)
                            entityDescription.Object.ClrType.GetMethod(entityDescription.GetAllMethodName,
                                           BindingFlags.Static).Invoke(null, new object[] { this });
                    else
                    {
                        if (list == null)
                            list = new List<IEntity>();
                        var unreadDs = new List<DataSource>(_unreadDataSources[entityDescription]);
                        foreach (var dataSource in unreadDs)
                        {
                            if (_unreadDataSources.ContainsKey(entityDescription) && _unreadDataSources[entityDescription].Contains(dataSource))
                                _unreadDataSources[entityDescription].Remove(dataSource);
                            list.AddRange(dataSource.GetLoader(entityDescription).LoadAll());
                        }
                    }
                    _loadedTypes.Add(entityDescription);
                    //}
                }
                if (_unreadDataSources.ContainsKey(entityDescription) && _unreadDataSources[entityDescription].Count > 0)
                {
                    if (list == null)
                        list = new List<IEntity>();
                    var dataSources = new List<DataSource>(_unreadDataSources[entityDescription]);
                    foreach (var dataSource in dataSources)
                    {
                        _unreadDataSources[entityDescription].Remove(dataSource);
                        list.AddRange(dataSource.GetLoader(entityDescription).LoadAll());
                    }
                }
                if (list == null || list.Count == 0)
                    list = new List<IEntity>();
                return list;
            }
            else
            {
                var l = new List<IEntity>();
                foreach (var t in entityDescription.AggregatedTypes)
                {
                    var templist = GetEntities(t.Object.ClrType);
                    if (templist != null && templist.Count > 0)
                        l.AddRange(templist);
                }
                return l;
            }
        }

        public IList<IEntity> GetEntities(Type type, string propertyName, object value)
        {
            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new ArgumentException(string.Format("Type '{0}' is not compatible with entityDescription 'IEntity'", type.Name));
            throw new NotImplementedException();
        }

        public IList<IEntity> GetEntities(Type type, string propertyName, object min, object max)
        {
            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new ArgumentException(string.Format("Type '{0}' is not compatible with entityDescription 'IEntity'", type.Name));
            throw new NotImplementedException();
        }

        public IEntity GetEntityById(Type type, object id)
        {
            if (!EntityDescription.IsEntity(type))
                throw new ArgumentException(string.Format("Type '{0}' is not compatible with entityDescription 'IEntity'", type.Name));
            return GetEntityById(EntityDescription.GetEntityDescription(type), id);
        }

        public IEntity GetEntityById(EntityDescription entityDescription, object id)
        {

            if (id == null)
                return null;
            if (!entityDescription.IsAggregated)
            {
                var e = Caches.GetEntityById(entityDescription.Object.ClrType, id);
                if (e == null)
                {
                    if (LoadAllOnGetById && !entityDescription.IsDenyGetAllOnGetById)
                    {
                        var temp = GetEntities(entityDescription);
                        e = Caches[entityDescription].Contains(id) ? Caches[entityDescription][id] : null;
                    }
                    else
                    {
                        foreach (var dataSource in GetDataSources(entityDescription))
                        {
                            var entity = dataSource.GetLoader(entityDescription).LoadEntityById(id);
                        }
                        e = Caches.GetEntityById(entityDescription.Object.ClrType, id);
                        //e = loader.LoadEntityById(id);
                    }
                }
                // temp
                //if (e == null)
                //{
                //    foreach (var context in _contexts)
                //    {
                //        e = context.GetEntityById(entityDescription, id);
                //        if (e != null)
                //            return e;
                //    }
                //}
                //
                return e;
            }
            else
            {
                foreach (var ed in entityDescription.AggregatedTypes)
                {
                    var e = GetEntityById(ed, id);
                    //if (Caches[t].Contains(id))
                    //    e = Caches[t][id];
                    if (e != null)
                        return e;
                }
                return null;
            }
        }

        public void Save(IEntity entity)
        {
            Save(new List<IEntity>(new[] {entity}));
        }

        public void Save(IList<IEntity> entities)
        {
            var entitiesBases = new List<EntityBase>();
            foreach ( var entity in entities )
            {
                if (EntityDescription.IsEntityBase(entity.GetType()))
                    entitiesBases.Add((EntityBase)entity);
                else
                    entitiesBases.Add((EntityBase)entity.Description);
            }
            var accessMediator = new XmlStorageAccessMediator(this);
            var saveStrategy = GetDefaultSaveStrategy(accessMediator);
            accessMediator.BeginCollectData();
            saveStrategy.Save(entitiesBases);
            accessMediator.EndCollectAndFlushData();
        }

        private IDataSaveStrategy GetDefaultSaveStrategy(XmlStorageAccessMediator accessMediator)
        {
            return new FullyRecursiveDataSaveStrategy(accessMediator, this);
        }

        public StorageAccessMediator GetSourceStorageAccessMediator()
        {
            return _accessMediator;
        }


        #endregion

        #region IMultipleDataSourceManager
        public void AddDataSource(DataSource dataSource)
        {
            _dataSources.Add(dataSource);
            if (_dataSources.Count == 1)
                _defaultDataSource = dataSource;
            foreach (var type in _unreadDataSources.Keys)
            {
                _unreadDataSources[type].Add(dataSource);
            }
            foreach (var type in _loadedTypes)
            {
                if (!_unreadDataSources.ContainsKey(type))
                    _unreadDataSources.Add(type, new List<DataSource>());
                if (!_unreadDataSources[type].Contains(dataSource))
                    _unreadDataSources[type].Add(dataSource);
            }
        }

        public void RemoveDataSource(DataSource dataSource)
        {
            if (_dataSources.Contains(dataSource))
            {
                foreach (var type in _unreadDataSources.Keys)
                    if (_unreadDataSources[type].Contains(dataSource))
                        _unreadDataSources[type].Remove(dataSource);
                foreach (var e in dataSource.GetEntites())
                    Caches.RemoveFromCache(e);
                _dataSources.Remove(dataSource);
            }
        }
        public void SetDefaultDataSource(DataSource dataSource)
        {
            if (!_dataSources.Contains(dataSource))
                throw new InvalidOperationException("The default data source should be one of the connected to the Ps");
            _defaultDataSource = dataSource;
        }

        public IList<DataSource> GetDataSources(EntityDescription type)
        {
            return new List<DataSource>(_dataSources);
        }

        public IList<DataSource> GetDataSources()
        {
            return new List<DataSource>(_dataSources);
        }
        #endregion

        public void Dispose()
        {
            foreach ( var dataSource in _dataSources )
                dataSource.Dispose();
            this._caches.Clear();
        }

        private List<EntityDescription> GetRefenecialClosure(EntityDescription e,
                                                             out List<EntityAttributeDescription> oto, 
                                                             out List<EntityAttributeDescription> otm,
                                                             out List<EntityAttributeDescription> mtm)
        {
            oto = new List<EntityAttributeDescription>();
            otm = new List<EntityAttributeDescription>();
            mtm = new List<EntityAttributeDescription>();
            var types = new List<EntityDescription>();

            var queue = new Queue<EntityDescription>();
            queue.Enqueue(e);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                types.Add(current);
                foreach (var relationship in current.Attributes.Relationships)
                {
                    if (!types.Contains(relationship.RelatedEntity))
                    {
                        queue.Enqueue(relationship.RelatedEntity);
                        if (relationship.IsOneToOneRelation)
                            oto.Add(relationship);
                        else if (relationship.IsOneToManyRelation)
                            otm.Add(relationship);
                        else if (relationship.IsManyToManyRelation)
                            mtm.Add(relationship);
                    }
                }
            }
            return types;
        }

        private List<T> LoadFromDs<T>() //where T : 
        {
            var list = LoadFromDs(EntityDescription.GetEntityDescription<T>());
            var result = new List<T>(list.Count);
            for (var i = 0; i < list.Count; i++)
                result[i] = (T)list[i];
            return result;
        }

        private List<object> LoadFromDs(Type type)
        {
            return LoadFromDs(EntityDescription.GetEntityDescription(type));
        }

        private List<object> LoadFromDs(EntityDescription e)
        {
            List<EntityAttributeDescription> otos; 
            List<EntityAttributeDescription> otms;
            List<EntityAttributeDescription> mtms;
            var types = GetRefenecialClosure(e, out otos, out otms, out mtms);

            var tmpCache = new CachesContainer();

            foreach (var entityDescription in types)
            {
                foreach (var ds in GetDataSources(entityDescription))
                {
                    foreach (var entity in ds.GetLoader(entityDescription).LoadAll())
                        tmpCache[entityDescription].Add(entity);
                    _unreadDataSources[entityDescription].Remove(ds);
                }
            }

            foreach (var oto in otos)
            {
                foreach (var entity in tmpCache[oto.Domain].GetAll())
                {
                    var relatedId = entity.Description.SourceRawValues[oto.Name];
                    var relatedEntity = tmpCache.GetEntityById(oto.RelatedEntity, relatedId) ?? _caches.GetEntityById(oto.RelatedEntity, relatedId);
                    if (relatedEntity == null)
                        throw new InvalidOperationException("Cannot find related entity by id");
                    entity[oto] = relatedEntity;
                }
            }

            foreach (var otm in otms)
            {
                var cache = tmpCache[otm.RelatedEntity];
                foreach (var entity in tmpCache[otm.Domain].GetAll())
                {
                    var relatedId = entity.Description.SourceRawValues[otm.Name];
                    var relatedEntity = cache.GetEntitiesByRawFieldValues(otm.RelationInfo.RelatedColumn, relatedId) ??
                                        _caches[otm.RelatedEntity].GetEntitiesByRawFieldValues(otm.RelationInfo.RelatedColumn, relatedId);
                    entity[otm] = relatedEntity;
                }
            }

            foreach (var mtm in mtms)
            {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
            //return tmpCache.GetEntities(e);
        }
    }
}
