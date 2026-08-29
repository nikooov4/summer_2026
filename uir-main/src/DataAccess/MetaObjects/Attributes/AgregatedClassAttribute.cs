using System;
using System.Collections.Generic;
using System.Text;
using Avant.DPO;
using Avant.DPO.MetaObjects.Attributes;

namespace Avant.DPO.MetaObjects.Attributes
{
	public class AgregatedClassAttribute:  Attribute
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
