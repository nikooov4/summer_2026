using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.Core;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects
{
	public class InstancePropertyDescription
	{
		private object _entity;
		private PropertyDescription _property;
		private Type _propertyType;

		internal InstancePropertyDescription(PropertyDescription property, object entity)
		{
			_entity = entity;
			_property = property;
			_propertyType = _property.PropertyType;
		}
		
		public object Entity
		{
			get
			{
				return _entity;
			}
		}

		public PropertyDescription Property
		{
			get
			{
				return _property;
			}
		}
	}
}
