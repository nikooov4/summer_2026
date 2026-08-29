using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes
{
    [Serializable]
    public class FieldNameAttribute : Attribute
	{
		string _fieldName;

		public FieldNameAttribute(string fieldName)
		{
			_fieldName = fieldName;
		}

		public string FieldName
		{
			get
			{
				return _fieldName;
			}
		}
	}
}
