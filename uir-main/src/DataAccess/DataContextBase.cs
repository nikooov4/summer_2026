using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public abstract class DataContextBase : DataManagerBase, IDataContext
    {
        protected DataContextState _state = DataContextState.NoChanges;
        //public IList<DataSource> GetDataSources(Type type)
        //{
        //    var list = new List<DataSource>();
        //    list.AddRange(_dataSources);
        //    return list;
        //}

        public virtual IEntity CreateEntity(Type type)
        {
            return CreateEntity(type, null);
        }

        public virtual IEntity CreateEntity(EntityDescription entityDescription)
        {
            return CreateEntity(entityDescription, null);
        }

        public virtual IEntity CreateEntity(EntityDescription entityDescription, DataSource dataSource)
        {
            var entity = entityDescription.CreateObject() as EntityBase;
            if (entityDescription.SystemId == entityDescription.EntityAttributes["ID"] &&
                (entityDescription.IdFieldType == typeof(object) || entityDescription.IdFieldType == typeof(Guid)) && (entity.ID == null || entity.ID is UnasignedValue))
                entity.ID = Guid.NewGuid();
            (entity.Description as EntityBase).Source = dataSource;
            (entity.Description as EntityBase).DataContext = this;
            SetState(DataContextState.Changed);
            return entity;
        }

        public IEntity CreateEntity(Type type, DataSource dataSource)
        {
            return CreateEntity(EntityDescription.GetEntityDescription(type), dataSource);
        }

        public T CreateEntity<T>(DataSource dataSource) where T : IEntity
        {
            return (T)CreateEntity(EntityDescription.GetEntityDescription(typeof(T)), dataSource);
        }

        public virtual T CreateEntity<T>() where T : IEntity
        {
            return CreateEntity<T>(null);
        }

        //public EntityEditingContext CreateChildContext(DataSource primaryDataSource, IEntity contextEntity)
        //{
        //    var context = new EntityEditingContext(contextEntity, this, primaryDataSource, null);
        //    _contexts.AddProperty(context);
        //    return context;
        //}

        public abstract IDataContext CreateChildContext();

        public abstract IDictionary<string, object> ContextData { get; }

        public virtual DataContextState State
        {
            get
            {
                return _state;
            }
        }

        public abstract void RollBack();
        public abstract void AcceptChanges();

        protected void SetState(DataContextState newState)
        {
            switch (newState)
            {
                case DataContextState.NoChanges:
                    if (newState == DataContextState.Changed)
                        _state = newState;
                    break;
                case DataContextState.PendingChanges:
                    _state = newState;
                    break;
                case DataContextState.Changed:
                    _state = newState;
                    break;
            }
        }
    }
}