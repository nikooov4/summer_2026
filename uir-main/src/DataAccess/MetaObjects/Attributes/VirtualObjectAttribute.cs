using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes
{
    [Serializable]
    public class VirtualObjectAttribute : Attribute
	{
		private string _getAllMethodName;

		public VirtualObjectAttribute(string getAllMethodName)
		{
			_getAllMethodName = getAllMethodName;
		}

		public string GetAllMethodName
		{
			get
			{
				return _getAllMethodName;
			}
			set
			{
				_getAllMethodName = value;
			}
		}
	}
}
