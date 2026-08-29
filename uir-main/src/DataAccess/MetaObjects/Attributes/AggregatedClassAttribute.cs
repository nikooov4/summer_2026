using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes
{
    [Serializable, AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AgregatedClassAttribute : Attribute
	{
		private Type[] _childTypes;

		public AgregatedClassAttribute(params Type[] parentType)
		{
			_childTypes = parentType;
		}

		public Type[] ChildTypes
		{
			get
			{
				return _childTypes;
			}
		}
	}
}
