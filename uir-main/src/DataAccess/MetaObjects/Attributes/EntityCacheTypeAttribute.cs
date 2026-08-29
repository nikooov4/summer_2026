using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes
{
    [Serializable]
    internal class EntityCacheTypeAttribute : Attribute
	{
		private readonly Type _entityCacheType;

		public EntityCacheTypeAttribute(Type type)
		{
			_entityCacheType = type;
		}

		public Type EntityCacheType
		{
			get
			{
				return _entityCacheType;
			}
		}
	}
}
