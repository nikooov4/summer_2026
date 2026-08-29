using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Wrapping
{
	public class EncapsulatedClassesAttribute: Attribute
	{
		private Type[] _encapsulatedClasses;

		public EncapsulatedClassesAttribute(params Type[] encapsulatedClasses)
		{
			_encapsulatedClasses = encapsulatedClasses;
		}

		public Type[] EncapsulatedClasses
		{
			get
			{
				return _encapsulatedClasses;
			}
		}
	}
}
