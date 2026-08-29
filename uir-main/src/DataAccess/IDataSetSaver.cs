using System;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public interface ISaver
	{
        EntityDescription ReflectedType
		{
			get;
		}

		System.Data.DataSet UnderlyngDataSet
		{
			get;
		}

        void Save(EntityBase entity);
        void Save(System.Collections.Generic.List<EntityBase> entities);
        void Save(StorageCommand command);
	}
}
