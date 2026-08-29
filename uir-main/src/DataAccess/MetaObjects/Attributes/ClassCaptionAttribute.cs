using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes
{
    [Serializable]
    public class ClassCaptionAttribute : Attribute
	{
		private string _caption;

		public ClassCaptionAttribute(string caption)
		{
			_caption = caption;
		}

		public string Caption
		{
			get
			{
				return _caption;
			}
			set
			{
				_caption = value;
			}
		}
	}
}
