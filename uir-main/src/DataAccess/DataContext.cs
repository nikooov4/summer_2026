using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.DataAccess.DataSources;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public class DataContext : DataContextBase
    {
        private readonly IDataContext _parentContext;
        private readonly List<IEntity> _createdEntities = new List<IEntity>();
        private readonly List<IEntity> _deletedEntities = new List<IEntity>();
        private readonly DataContextInnerData _contextData;

        internal DataContext(IDataContext parentContext)
        {
            _parentContext = parentContext;
            _contextData = new DataContextInnerData(parentContext.ContextData);
        }

        public IDataContext ParentContext
        {
            get { return _parentContext; }
        }

        public override CachesContainer Caches
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEntity CreateEntity(MetaObjects.EntityDescription entityDescription, DataSources.DataSource dataSource)
        {
            var entity = base.CreateEntity(entityDescription, dataSource);
            _createdEntities.Add(entity);
            return entity;
        }

        public override IEntity GetEntityById(Type type, object id)
        {
            var entity = _parentContext.GetEntityById(type, id);
            if (entity == null || _deletedEntities.Contains(entity))
                return _createdEntities.Find(e => id.Equals(e.ID));
            else
                return entity;
        }

        public override IList<IEntity> GetEntities(Type type)
        {
            var entities = _parentContext.GetEntities(type);
            _deletedEntities.ForEach(e =>
                                         {
                                             if ( type.IsAssignableFrom(e.GetType()) )
                                                 entities.Remove(e);
                                         });
            _createdEntities.ForEach(e =>
                                         {
                                             if ( type.IsAssignableFrom(e.GetType()) )
                                                 entities.Add(e);
                                         });
            return entities;
        }

        public override List<T> GetEntities<T>()
        {
            var entities = _parentContext.GetEntities<T>();
            _deletedEntities.ForEach(e =>
                                         {
                                             if ( typeof (T).IsAssignableFrom(e.GetType()) )
                                                 entities.Remove((T)e);
                                         });
            _createdEntities.ForEach(e =>
                                         {
                                             if ( typeof (T).IsAssignableFrom(e.GetType()) )
                                                 entities.Add((T)e);
                                         });
            return entities;
        }

        public override void AddEntity(IEntity entity)
        {
            _createdEntities.Add(entity);
            SetState(DataContextState.Changed);
        }

        public override void Delete(IEntity e)
        {
            if (_createdEntities.Contains(e))
                _createdEntities.Remove(e);
            else
                _deletedEntities.Add(e);
            SetState(DataContextState.Changed);
        }

        public override void Save(IEntity entity)
        {
            if (_createdEntities.Contains(entity))
            {
                _parentContext.AddEntity(entity);
                _createdEntities.Remove(entity);
            }
            else if (_deletedEntities.Contains(entity))
            {
                _parentContext.Delete(entity);
                _deletedEntities.Remove(entity);
            }
        }

        public override void Save(List<IEntity> entities)
        {
            entities.ForEach(Save);
        }

        public override IDictionary<string,object> ContextData
        {
            get { return _contextData; }
        }

        public override IDataContext CreateChildContext()
        {
            return new DataContext(this);
        }

        #region Transaction

        public override void RollBack()
        {
            if (_state == DataContextState.NoChanges)
                return;
            if (_state == DataContextState.PendingChanges)
            {
                foreach ( var createdEntity in _createdEntities )
                    if ( _parentContext.Caches.Contains(createdEntity) )
                        _parentContext.Caches.RemoveFromCache(createdEntity);
                foreach ( var deletedEntity in _deletedEntities )
                {
                    //_parentContext.
                }
            }
            _deletedEntities.Clear();
            _createdEntities.Clear();
            SetState(DataContextState.NoChanges);
        }

        /// <summary>
        /// Preliminarily submits changes to the parent context. At this point the changes are available in
        /// the parent context, though still may be cancelled.
        /// </summary>
        private void Submit()
        {
            if (_state == DataContextState.PendingChanges)
                return;
            foreach (var entity in _createdEntities)
                _parentContext.AddEntity(entity);
            foreach ( var deletedEntity in _deletedEntities )
                _parentContext.Delete(deletedEntity);
            SetState(DataContextState.PendingChanges);
        }

        public override void AcceptChanges()
        {
            if (_state == DataContextState.NoChanges)
                return;
            if (_state != DataContextState.PendingChanges)
                Submit();
            _createdEntities.Clear();
            _deletedEntities.Clear();
            SetState(DataContextState.NoChanges);
        }

        #endregion
    }

}
