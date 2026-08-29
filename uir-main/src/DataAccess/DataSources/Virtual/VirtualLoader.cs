using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Virtual
{
    public delegate List<IEntity> LoadAllDelegate();
    public delegate List<T> LoadAllDelegate<T>();
    public delegate IEntity LoadEntityByIdDelegate(EntityDescription ed, object id);
    public delegate List<IEntity> LoadByFieldValueDelegate(EntityDescription ed, string field, object value);
    public delegate void ReloadEntityDelegate(IEntity entity);

    public class VirtualLoader : ILoader
    {
        private readonly PersistentStorage _ps;
        private readonly EntityDescription _entityDescription;
        private readonly Mapper _mapper;
        private readonly LoadAllDelegate _loadAll;
        private readonly LoadEntityByIdDelegate _loadEntityById;
        private readonly LoadByFieldValueDelegate _loadByFieldValue;
        private readonly ReloadEntityDelegate _reloadEntity;

        public VirtualLoader(PersistentStorage ps, Mapper mapper, LoadAllDelegate loadAll, LoadEntityByIdDelegate loadEntityById, 
            LoadByFieldValueDelegate loadByFieldValue, ReloadEntityDelegate reloadEntity)
        {
            _ps = ps;
            _mapper = mapper;
            _entityDescription =_mapper.MappedEntity;
            _loadAll = loadAll;
            _loadEntityById = loadEntityById;
            _loadByFieldValue = loadByFieldValue;
            _reloadEntity = reloadEntity;
        }

        public EntityDescription ReflectedType
        {
            get { return _entityDescription; }
        }

        public Mapper Mapper
        {
            get { return _mapper; }
        }

        public List<IEntity> LoadAll()
        {
            return _loadAll();
        }

        public List<T> LoadAll<T>() where T : IEntity
        {
            throw new NotImplementedException();
        }

        public IEntity LoadEntityById(object id)
        {
            return _loadEntityById(_entityDescription, id);
        }

        public List<IEntity> LoadByFieldValue(string field, object value)
        {
            return _loadByFieldValue(_entityDescription, field, value);
        }

        public void ReloadEntity(IEntity entity)
        {
            throw new NotImplementedException();
        }

        public void ReloadEntity(EntityBase entity)
        {
            _reloadEntity(entity);
        }
    }
}
