using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.DynamicEntityBuilding;
using Mephi.Cybernetics.Race.DataAccess.Predicates;
using Mephi.Cybernetics.Race.DataAccess.DataSources;

namespace Mephi.Cybernetics.Race.DataAccess
{
    //TODO: куда-то добавить язык запросов
    //TODO: Расширить до поддержки Relatioship
    //TODO: Добавить работу с концептуальной схемо ПО
    /// <summary>
    /// Высокоуровневый интерфейс - точка входа в среду
    /// </summary>
    public interface IDataManager
    {
        IList<IEntity> GetEntities(Type type);
        EntityList<T> GetEntities<T>(Predicate<T> where) where T : IEntity;
        List<T> GetEntities<T>() where T : IEntity;
        IEntity GetOneEntityByPredicate(Type type, UnaryOperation<IEntity> op);
        T GetOneEntityByPredicate<T>(UnaryOperation<T> op) where T : IEntity;
        List<IEntity> GetEntitiesByPredicate(Type type, UnaryOperation<IEntity> op);
        List<T> GetEntitiesByPredicate<T>(UnaryOperation<T> op) where T : IEntity;
        List<IEntity> GetEntitiesByFieldValue(Type type, string fieldName, object value);
        List<T> GetEntitiesByFieldValue<T>(string fieldName, object value) where T : IEntity;
        T GetOneEntityByFieldValue<T>(string filedName, object value) where T : IEntity;
        T GetEntityById<T>(object id) where T : IEntity;
        IEntity GetEntityById(Type type, object id);
        List<IEntity> GetReferencingEntities(IEntity entity);
        List<IEntity> GetReferencingEntites(IEntity entity, Type restriction);
        List<IEntity> GetReferencedEntities(IEntity entity);
        List<IEntity> GetReferencedEntites(IEntity entity, Type restriction);
        List<T> GetReferencedEntities<T>(IEntity entity) where T : IEntity;
        List<IEntity> Select(UnaryOperation<IEntity> predicate);
        void Save(IEntity entity);
        void Save(List<IEntity> entities);

        [Obsolete("Такого свойства вообще не должно быть у PersistentStoeage")]
        CachesContainer Caches { get; }

        void AddEntity(IEntity entity);
        void Delete(IEntity e);
    }

    /// <summary>
    /// Реализует функционал транзакций, в т.ч. вложенных
    /// </summary>
    public interface IDataContext : IDataManager
    {
        IEntity CreateEntity(Type type);
        T CreateEntity<T>() where T : IEntity;
        //EntityEditingContext CreateChildContext(DataSource primaryDataSource, IEntity contextEntity);
        IEntity CreateEntity(Type type, DataSource dataSource);
        T CreateEntity<T>(DataSource dataSource) where T : IEntity;

        IDictionary<string, object> ContextData { get; }
        DataContextState State { get; }
        void RollBack();
        void AcceptChanges();
        IDataContext CreateChildContext();
        IEntity CreateEntity(EntityDescription entityDescription);
        IEntity CreateEntity(EntityDescription entityDescription, DataSource dataSource);
    }

    //TODO: Добавить события DataSourceAdded и DataSourceDeleted
    //TODO: Наверное, тут должен быть функционал по распределению объектов между DS ?
    /// <summary>
    /// 
    /// </summary>
    public interface IDataSourceManager
    {
        void AddDataSource(DataSource dataSource);
        void SetDefaultDataSource(DataSource dataSource);
        IList<DataSource> GetDataSources(EntityDescription type);
        IList<DataSource> GetDataSources();
        void RemoveDataSource(DataSource dataSource);
    }

    public interface IDataAccessProvider
    {
        IList<T> GetEntities<T>() where T : IEntity;
        IList<T> GetEntities<T>(string propertyName, object value) where T : IEntity;
        IList<T> GetEntities<T>(string propertyName, object min, object max) where T : IEntity;
        T GetEntityBiId<T>(object id);

        IList<IEntity> GetEntities(Type type);
        IList<IEntity> GetEntities(Type type, string propertyName, object value);
        IList<IEntity> GetEntities(Type type, string propertyName, object min, object max);
        IEntity GetEntityById(Type type, object id);

        void Save(IEntity entity);
        void Save(IList<IEntity> entities);
        StorageAccessMediator GetSourceStorageAccessMediator();
        IList<DataSource> GetDataSources(EntityDescription entityType);
        CachesContainer Caches { get; }
    }

    /// <summary>
    /// Убрать поддержку интерфейса IDataSourceManager ?
    /// Методы все равно оставить, ибо фасад
    /// </summary>
    [Serializable]
    public class PersistentStorage : DataContextBase, IDataSourceManager
    {
        public enum PsImportMode
        {
            PreserveEntitiesStates,
            SetEntitiesStatesToNew
        }

        private CachableDataProvider _dataProvider;
        private readonly Dictionary<string, Type> _customEntities = new Dictionary<string, Type>();
        private readonly Dictionary<Type, FacadeBuilder> _entityFacades = new Dictionary<Type, FacadeBuilder>();
        private string _fileName;
        private string _entitiesTagPath;
        private Configuration.Configuration _configuration;

        private static readonly Dictionary<string, PersistentStorage> _psCache = new Dictionary<string, PersistentStorage>();
        private readonly DataContextInnerData _contextData = new DataContextInnerData(null);

        public delegate void PsEntityAdded();


        /// <summary>
        /// Keeps the Ps in the memory for future use. Usefull for data interchange between different
        /// modules, and for web applications as well
        /// </summary>
        /// <param name="key">the string key under which to store the Ps</param>
        /// <param name="ps">the Ps to store</param>
        public static void CachePs(string key, PersistentStorage ps)
        {
            if (!_psCache.ContainsKey(key))
                _psCache[key] = ps;
            else
                _psCache.Add(key, ps);
        }

        public static PersistentStorage GetCachedPs(string key)
        {
            return !_psCache.ContainsKey(key) ? null : _psCache[key];
        }

        public FacadeBuilder GetFacadeBuilder(Type entityType)
        {
            //FacadeBuilder builder = null;
            if (!_entityFacades.ContainsKey(entityType))
            {
                FacadeBuilder builder = new FacadeBuilder(entityType, this);
                _entityFacades.Add(entityType, builder);
            }
            return _entityFacades[entityType];
        }

        //public PersistentStorage(DataSet dataSet, string configuration)
        //{
        //    _dataSet = dataSet;
        //    LoadConfiguration(configuration);
        //    Init();
        //}

        //public PersistentStorage(string fileName, string entitiesTagPath, string configuration)
        //{
        //    _fileName = fileName;
        //    _entitiesTagPath = entitiesTagPath;
        //    _storageAccessMediator = new XmlStorageAccessMediator(this);
        //    LoadConfiguration(configuration);
        //    Init();
        //}

        //public PersistentStorage(Adapter adapter)
        //{
        //    Init();
        //}

        public PersistentStorage()
        {
            Init();
        }


        public PersistentStorage(IEnumerable<DataSource> dataSources)
        {
            Init();
            foreach (var ds in dataSources)
                _dataProvider.AddDataSource(ds);
        }

        public void Reconfigure(string fileName, string entitiesTagPath)
        {
            //_mode = DataSourceMode.Xml;
            _fileName = fileName;
            _entitiesTagPath = entitiesTagPath;
            //_storageAccessMediator = new XmlStorageAccessMediator(this);
            //_savers.Clear();
        }

        public PersistentStorage(string connectionString, string configuration)
        {
            //_mode = DataSourceMode.MsSql;
            //_connectionString = connectionString;
            LoadConfiguration(configuration);
            Init();
            _storageAccessMediator = new MsSqlStorageAccessMediator(this, new System.Data.SqlClient.SqlConnection(connectionString));
        }

        private void LoadConfiguration(string configuration)
        {
            if (string.IsNullOrEmpty(configuration))
                return;
            System.IO.StringReader r = new System.IO.StringReader(configuration);
            System.Xml.XmlReader reader = System.Xml.XmlReader.Create(r);
            reader.ReadToFollowing("Entities");
            _configuration = Configuration.Configuration.Create(reader);
        }

        private void Init()
        {
            _dataProvider = new CachableDataProvider();
            _entityFactory = new EntityFactory(this);
        }

        [Obsolete("Это свойство специфично для XmlDataSource")]
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        [Obsolete("Это свойство специфично для XmlDataSource")]
        public string EntitiesTagPath
        {
            get { return _entitiesTagPath; }
            set { _entitiesTagPath = value; }
        }

        [Obsolete("Перенести в DataSource полностью")]
        public Configuration.Configuration PsConfiguration
        {
            get { return _configuration; }
        }

        //protected override IMephi.Cybernetics.Race.DataAccessProvider Mephi.Cybernetics.Race.DataAccessProvider
        //{
        //    get { return _dataProvider; }
        //}

        [Obsolete("Такого свойства вообще не должно быть у PersistentStoeage")]
        public override CachesContainer Caches
        {
            get { return _dataProvider.Caches; }
        }

        [Obsolete("Перенести в DataSource")]
        public bool LoadAllOnGetById
        {
            get { return _loadAllOnGetById; }
            set { _loadAllOnGetById = value; }
        }

        public EntityFactory EntitiesFactory
        {
            get { return _entityFactory; }
        }

        public void AddDataSource(DataSource dataSource)
        {
            _dataProvider.AddDataSource(dataSource);
        }

        public void RemoveDataSource(DataSource dataSource)
        {
            _dataProvider.RemoveDataSource(dataSource);
        }

        public void SetDefaultDataSource(DataSource dataSource)
        {
            _dataProvider.SetDefaultDataSource(dataSource);
        }

        public IList<DataSource> GetDataSources()
        {
            return _dataProvider.GetDataSources();
        }

        public IList<DataSource> GetDataSources(EntityDescription type)
        {
            return _dataProvider.GetDataSources(type);
        }

        public Type GetCustomEntityType(string name)
        {
            if (!_customEntities.ContainsKey(name))
            {
                UnaryOperation<IEntity> u = e => (e as DynamicEntityBuilder).Name == name;
                var l = Functions.Select<IEntity>((List<IEntity>) GetEntities(typeof (DynamicEntityBuilder)), u);
                if (l.Count == 0)
                    return null;

                _customEntities.Add(name, (l[0] as DynamicEntityBuilder).Type);
            }
            return _customEntities[name];
        }

        public override IList<IEntity> GetEntities(Type type)
        {
            return _dataProvider.GetEntities(type);
        }

        public override List<T> GetEntities<T>()
        {
            return new List<T>(_dataProvider.GetEntities<T>());
        }

        public override IEntity GetEntityById(Type type, object id)
        {
            return _dataProvider.GetEntityById(type, id);
        }

        public override void Save(IEntity entity)
        {
            _dataProvider.Save(entity);
        }

        public override void Save(List<IEntity> entities)
        {
            _dataProvider.Save(entities);
        }

        public override void Delete(IEntity e)
        {
            _dataProvider.Caches.RemoveFromCache(e);
            SetState(DataContextState.Changed);
        }

        public override void AddEntity(IEntity entity)
        {
            if (_dataProvider.Caches[entity.Description.EntityDescriptor].Contains(entity.ID))
                throw new Exception(string.Format("Entity (type = '{0}', id = '{1}') already exists. Entity.ToString() = '{2}'", entity.GetType().Name,
                    entity.ID, entity.ToString()));
            (entity.Description as EntityBase).DataContext = this;
            _dataProvider.Caches[entity.Description.EntityDescriptor].Add(entity);
            SetState(DataContextState.Changed);
        }

        /// <summary>
        /// Adds the entity, if it is not already added
        /// </summary>
        /// <param name="entity">entity to add</param>
        /// <returns>true if the entity was added now, and false if it has been added previously</returns>
        [Obsolete("Метод необходимо ликвидировать, это чит")]
        public bool EnsureEntityAdded(EntityBase entity)
        {
            if (!_dataProvider.Caches[entity.Description.EntityDescriptor].Contains(entity.ID))
            {
                entity.DataContext = this;
                _dataProvider.Caches[entity.Description.EntityDescriptor].Add(entity);
                return true;
            }
            return false;
        }

        //public List<T> GetEntitiesByPredicate<T>(Type type, UnaryOperation<T> op) where T : EntityBase
        //{
        //    return Functions.Select<T>(GetEntities(type), op);
        //}

        //public bool IsLoaded(Type type)
        //{
        //    return _loadedTypes.Contains(type) && _unreadDataSources.Count == 0;
        //}

        public delegate void EntityProc(EntityBase e);

        public void ReloadEntity(EntityBase entity)
        {
            entity.Source.GetLoader(entity.EntityDescriptor).ReloadEntity(entity);
            //GetLoader(entity.GetType()).ReloadEntity(entity);
        }

        public void Dispose()
        {
            //this.Caches.Clear();
            //this._loadedTypes.Clear();
            //this._loaders.Clear();
            //this._savers.Clear();
            //this._underLoading.Clear();
            this._dataProvider.Dispose();
            this._entityFacades.Clear();
            this._customEntities.Clear();
        }

        public override IDataContext CreateChildContext()
        {
            return new DataContext(this);
        }

        public override IDictionary<string, object> ContextData
        {
            get { return _contextData; }
        }

        public override void RollBack()
        {
            SetState(DataContextState.NoChanges);
        }

        public override void AcceptChanges()
        {
            var unsaved = Select(e => e.Description.Source != null && e.Description.State != EntityState.Unchanged);
            if (unsaved.Count > 0)
                Save(unsaved);
            SetState(DataContextState.NoChanges);
        }
    }

    public enum DataLoadStrategyType
    {
        Eager,
        Lazy
    }

}
