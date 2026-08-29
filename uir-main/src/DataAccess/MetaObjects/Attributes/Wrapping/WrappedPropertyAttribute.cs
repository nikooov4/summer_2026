using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Wrapping
{
	public class WrappedPropertyAttribute : Attribute
	{
		private string _className;
		private string _propertyName;

		public WrappedPropertyAttribute(string className, string propertyName)
		{
			_className = className;
			_propertyName = propertyName;
		}

		public string ClassName
		{
			get
			{
				return _className;
			}
		}

		public string PropertyName
		{
			get
			{
				return _propertyName;
			}
		}
	}
}
