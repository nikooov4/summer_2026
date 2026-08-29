using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public class EntityEditingContext : DataContext
    {
        //private IEntity _contextEntity;
        //private DataSource _primaryDataSource;
        //private IDataContext _ps;
        //private EntityEditingContext _parentContext;
        //private List<EntityEditingContext> _childContexts = new List<EntityEditingContext>();
        //private List<IEntity> _createdEntities = new List<IEntity>();
        


        public EntityEditingContext(IDataContext parentContext) : this(parentContext, null, null)
        {
        }

        public EntityEditingContext(IDataContext parentContext, IEntity contextEntity) : this(parentContext, contextEntity, null)
        {
        }

        public EntityEditingContext(IDataContext parentContext, IEntity contextEntity, DataSource primaryDataSource) :
            base(parentContext)
        {
            if (contextEntity != null)
                ContextEntity = contextEntity;
            if (primaryDataSource != null)
                PrimaryDataSource = primaryDataSource;
        }

        //internal EntityEditingContext(IEntity contextEntity, IDataContext ps, DataSource primaryDataSource, EntityEditingContext parentContext)
        //{
        //    _contextEntity = contextEntity;
        //    _parentContext = parentContext;
        //    _ps = ps;
        //    _primaryDataSource = primaryDataSource;
        //}

        //public EntityEditingContext ParentContext
        //{
        //    get { return _parentContext; }
        //    private set { _parentContext = value; }
        //}

        //public IDataContext Context
        //{
        //    get { return _context; }
        //}

        public DataSource PrimaryDataSource
        {
            get { return ContextData["PrimaryDataSource"] as DataSource; }
            set { ContextData["PrimaryDataSource"] = value; }
        }

        public IEntity ContextEntity
        {
            get { return ContextData["ContextEntity"] as IEntity; }
            set { ContextData["ContextEntity"] = value; }
        }

        //public DataContextState State
        //{
        //    get { return _state; }
        //}

        //public EntityEditingContext CreateChildContext(IEntity contextEntity)
        //{
        //    return new EntityEditingContext(Context.CreateChildContext(), contextEntity ?? ContextEntity, PrimaryDataSource);
        //}

        //public EntityEditingContext CreateChildContext()
        //{
        //    return new EntityEditingContext(Context.CreateChildContext(), ContextEntity);
        //}

        //public override T CreateEntity<T>(DataSource dataSource) where T : IEntity
        //{
        //    var e = _ps.CreateEntity<T>(dataSource);
        //    _createdEntities.AddProperty(e);
        //    return e;
        //}

        //public IEntity CreateEntity(Type type, DataSource dataSource)
        //{
        //    var e = _ps.CreateEntity(type, dataSource);
        //    _createdEntities.AddProperty(e);
        //    SetState(DataContextState.Changed);
        //    return e;
        //}

        public override IEntity CreateEntity(Type type)
        {
            return CreateEntity(type, PrimaryDataSource);
        }

        public override T CreateEntity<T>() 
        {
            return CreateEntity<T>(PrimaryDataSource);
        }

        public override IEntity CreateEntity(EntityDescription entityDescription)
        {
            return CreateEntity(entityDescription, PrimaryDataSource);
        }
        //public IEntity CreateEntity(EntityDescription EntityDescription)
        //{
        //    if (EntityDescription.IsAggregated)
        //        throw new NotImplementedException("Creation of aggregated object not implemented");
        //    return CreateEntity(EntityDescription.ClrType);
        //}

        //public IEntity CreateEntity(EntityDescription EntityDescription, DataSource dataSource)
        //{
        //    if (EntityDescription.IsAggregated)
        //        throw new NotImplementedException("Creation of aggregated object not implemented");
        //    return CreateEntity(EntityDescription.ClrType, dataSource);
        //}

        //public void RollBack()
        //{
        //    if (_state == DataContextState.NoChanges)
        //        return;
        //    foreach ( var childContext in _childContexts )
        //        childContext.RollBack();
        //    _childContexts.Clear();
        //    foreach (var createdEntity in _createdEntities)
        //        if (_ps.Caches.Contains(createdEntity))
        //            _ps.Caches.RemoveFromCache(createdEntity);
        //    _createdEntities.Clear();
        //    SetState(DataContextState.NoChanges);
        //}

        //public void Submit()
        //{
        //    if (_state == DataContextState.PendingChanges)
        //        return;
        //    foreach (var childContext in _childContexts)
        //        childContext.Submit();
        //    foreach (var entity in _createdEntities)
        //        _ps.AddEntity(entity);
        //    SetState(DataContextState.PendingChanges);
        //}

        //public void AcceptChanges()
        //{
        //    if (_state == DataContextState.NoChanges)
        //        return;
        //    _createdEntities.Clear();
        //    foreach ( var context in _childContexts )
        //        context.AcceptChanges();
        //    SetState(DataContextState.NoChanges);
        //}

        //private void SetState(DataContextState newState)
        //{
        //    switch (newState)
        //    {
        //        case DataContextState.NoChanges:
        //            if (newState == DataContextState.Changed)
        //                _state = newState;
        //            break;
        //        case DataContextState.PendingChanges:
        //            _state = newState;
        //            break;
        //        case DataContextState.Changed:
        //            _state = newState;
        //            break;
        //    }
        //    if (newState == DataContextState.Changed)
        //        if (_parentContext != null)
        //            _parentContext.SetState(newState);
        //}

        //public IEntity GetEntityById(Type type, object id)
        //{
        //    foreach ( var entity in _createdEntities )
        //    {
        //        if (type.IsAssignableFrom( entity.GetType()) && entity.ID == id)
        //            return entity;
        //    }
        //    foreach ( var context in _childContexts )
        //    {
        //        var e = context.GetEntityById(type, id);
        //        if (e != null)
        //            return e;
        //    }
        //    return null;
        //}

        public override IDataContext CreateChildContext()
        {
            return new EntityEditingContext(this);
        }

        public EntityEditingContext CreateChildContext(IEntity contextEntity)
        {
            return new EntityEditingContext(this, contextEntity, PrimaryDataSource);
        }

        public EntityEditingContext CreateChildContext(IEntity contextEntity, DataSource primaryDataSource)
        {
            return new EntityEditingContext(this, contextEntity, primaryDataSource);
        }
    }
}
