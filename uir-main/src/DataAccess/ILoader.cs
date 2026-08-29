using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public interface ILoader
	{
        EntityDescription ReflectedType { get; }

        Mapper Mapper { get; }

        List<IEntity> LoadAll();
        List<T> LoadAll<T>() where T : IEntity;
        IEntity LoadEntityById(object Id);
        List<IEntity> LoadByFieldValue(string field, object value);
        void ReloadEntity(IEntity entity);
	}
}
