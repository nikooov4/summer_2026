using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.Core.ObjectMapping;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Virtual
{
    public class CollectionDataSource : VirtualDataSource
    {
        private readonly Dictionary<EntityDescription, List<IEntity>> _dataCache;
        private readonly ICollection<IEntity> _sourceData;
        

        public CollectionDataSource(PersistentStorage ps, ICollection<IEntity> collection): base(ps)
        {
            _sourceData = collection;
            _dataCache = new Dictionary<EntityDescription, List<IEntity>>();
            Init(LoadAll, LoadEntityById, LoadByFieldValue, ReloadEntity, Save);
        }

        private List<IEntity> LoadAll()
        {
            var list = new List<IEntity>();
            foreach ( var entities in _dataCache )
                list.AddRange(entities.Value);
            return list;
        }

        private List<T> LoadAll<T>()
        {
            var list = new List<T>();
            foreach ( var entity in _dataCache[EntityDescription.GetEntityDescription(typeof(T))] )
                list.Add((T)entity);
            return list;
        }

        private IEntity LoadEntityById(EntityDescription type, object id)
        {
            foreach ( var entities in _dataCache )
                foreach ( var entity in entities.Value )
                {
                    if ( entity.ID == id )
                        return entity;
                }
            return null;
        }

        private List<IEntity> LoadByFieldValue(EntityDescription type, string fieldName, object value)
        {
            var list = new List<IEntity>();
            foreach ( var entitiy in _dataCache[type] )
            {
                if ( entitiy[fieldName] == value )
                    list.Add(entitiy);
            }
            return list;
        }

        private void ReloadEntity(IEntity entity)
        {
            //Mapping<IEntity>.GetIdentityMapping(entity.GetType()).MapOnto(entity, ref entity);
            return;
        }

        private void Save(StorageCommand command)
        {
            
        }

        public override void Open()
        {
            foreach ( var item in _sourceData )
            {
                if ( !_dataCache.ContainsKey(item.Description.EntityDescriptor) )
                    _dataCache.Add(item.Description.EntityDescriptor, new List<IEntity>());
                _dataCache[item.Description.EntityDescriptor].Add(Mapping<IEntity>.GetIdentityMapping(item.GetType()).Map(item) as IEntity);
            }
        }

        public override void Close()
        {
            _dataCache.Clear();
        }

        public override void Flush()
        {
            _sourceData.Clear();
            foreach ( var entities in _dataCache )
                foreach ( var entity in entities.Value )
                    _sourceData.Add(Mapping<IEntity>.GetIdentityMapping(entity.GetType()).Map(entity) as IEntity);
        }

        public override void Dispose()
        {
            _dataCache.Clear();
        }
    }
}
