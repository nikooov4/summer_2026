using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Wrapping
{
	public class WrappedClassAttribute: Attribute
	{
		private Type _wrappedType;

		public WrappedClassAttribute(Type wrappedType)
		{
			_wrappedType = wrappedType;
		}

		public Type WrappedType
		{
			get
			{
				return _wrappedType;
			}
		}
	}
}
