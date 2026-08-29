using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public interface IEntityCache : ICache<object, IEntity>
	{
		EntityDescription Type
		{
			get;
		}

        void Add(IEntity item);
		//EntityList<EntityBase> GetAll();
        IEntity GetById(object id);
        T GetById<T>(object id) where T : IEntity;
        List<T> GetAll<T>() where T : IEntity;
		void Refresh(ILoader loader);
        void Reload(List<IEntity> list);
        void Delete(IEntity item);
		void DeleteById(object id);
		bool ContainsElements(string propertyName, object value);
        List<IEntity> GetEntitiesByRawFieldValues(string propertyName, object value);
	}
}
