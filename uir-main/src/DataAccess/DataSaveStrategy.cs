using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data.SqlClient;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public class FullyRecursiveDataSaveStrategy : IDataSaveStrategy
    {
        private readonly XmlStorageAccessMediator _mediator;
        private readonly IDataAccessProvider _dataProvider;

        public FullyRecursiveDataSaveStrategy(XmlStorageAccessMediator mediator, IDataAccessProvider dataProvider)
        {
            _mediator = mediator;
            _dataProvider = dataProvider;
        }

        public XmlStorageAccessMediator Mediator
		{
			get
			{
				return _mediator;
			}
		}

		public void Save(List<EntityBase> entities)
		{
			foreach ( var entity in entities )
				Save(entity);
		}

		public void Save(EntityBase entity)
		{
			if ( entity.State == EntityState.Unchanged || entity.LoadingState == EntityLoadingState.Saving )
				return;

            var od = EntityDescription.GetEntityDescription(entity.GetType());

            if (od.IsWrapped)
            {
                Save((entity as IWrapObject).WrappedObject);
                return;
            }

		    var isNew = entity.State == EntityState.New;
			var isDeleted = entity.State == EntityState.Deleted;
			//ent.State = EntityState.Unchanged;

			if ( isNew && entity.ID == null )
				throw new Exception("GOTO HELL -- ID IS NULL...");

            if (isNew && !_dataProvider.Caches[entity.Description.EntityDescriptor].Contains(entity.ID))
                _dataProvider.Caches[entity.Description.EntityDescriptor].Add(entity);


			foreach ( var pd in od.EntityAttributes.Relationships.ManyToMany )
				_mediator.AddCommand(new StorageCommand(entity, pd));

			var command = new StorageCommand(entity);
			entity.BeginSave();

			if ( isNew )
			{
                foreach (var pd in od.EntityAttributes.Relationships.OneToOne)
					if ( pd.GetValue(entity) != null && !(pd.GetValue(entity) is UnasignedValue) )
						Save(pd.GetValue(entity) as EntityBase);
				_mediator.AddCommand(command);
                foreach (var pd in od.EntityAttributes.Relationships.OneToMany)
					if ( pd.GetValue(entity) as IList != null )
						foreach ( EntityBase e in UnionLists(entity[pd] as IList, entity[pd, true] as IList) )
							Save(e);
                foreach (var pd in od.EntityAttributes.Relationships.ManyToMany)
                    if (pd.GetValue(entity) as IList != null)
                        foreach (EntityBase e in UnionLists(entity[pd] as IList, entity[pd, true] as IList))
                            Save(e);
            }
			else if ( command.CommandType == StorageCommandType.Delete )
			{
                foreach (var pd in od.EntityAttributes.Relationships.OneToMany)
					if ( pd.GetValue(entity) as IList != null )
                        foreach (EntityBase e in UnionLists(entity[pd] as IList, entity[pd, true] as IList))
							Save(e);
				_mediator.AddCommand(command);
                foreach (var pd in od.EntityAttributes.Relationships.OneToOne)
					if ( pd.GetValue(entity) != null && !(pd.GetValue(entity) is UnasignedValue) )
						Save(pd.GetValue(entity) as EntityBase);
                foreach (var pd in od.EntityAttributes.Relationships.ManyToMany)
                    if (pd.GetValue(entity) as IList != null)
                        foreach (EntityBase e in UnionLists(entity[pd] as IList, entity[pd, true] as IList))
                            Save(e);
            }
			else if ( command.CommandType == StorageCommandType.Update )
			{
                foreach (var pd in od.EntityAttributes.Relationships.OneToOne)
					if ( pd.GetValue(entity) != null && !(pd.GetValue(entity) is UnasignedValue) )
						Save(pd.GetValue(entity) as EntityBase);
				_mediator.AddCommand(command);
                foreach (var pd in od.EntityAttributes.Relationships.OneToMany)
					if ( pd.GetValue(entity) as IList != null )
                        foreach (EntityBase e in UnionLists(entity[pd] as IList, entity[pd, true] as IList))
							Save(e);
                foreach (var pd in od.EntityAttributes.Relationships.ManyToMany)
                    if (pd.GetValue(entity) as IList != null)
                        foreach (EntityBase e in UnionLists(entity[pd] as IList, entity[pd, true] as IList))
                            Save(e);
            }

			if ( isDeleted )
			{
                if (_dataProvider.Caches[entity.Description.EntityDescriptor].Contains(entity.ID))
                    _dataProvider.Caches[entity.Description.EntityDescriptor].Delete(entity);
			}
			entity.AcceptChanges();
		}

        private IList UnionLists(IList list1, IList list2)
        {
            var result = new ArrayList();

            if ( list1 == null )
                list1 = new ArrayList();
            if ( list2 == null )
                list2 = new ArrayList();

            result.AddRange(list1);
            foreach (var element in list2)
                if (!result.Contains(element))
                    result.Add(element);
            return result;
        }

	}
}
