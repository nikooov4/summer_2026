using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess
{
	[Serializable]
	public class StringCaseInsensitiveComparer : IEqualityComparer<string>
	{
		#region IEqualityComparer<string> Members

		public bool Equals(string x, string y)
		{
			return string.Equals(x, y, StringComparison.CurrentCultureIgnoreCase);
		}

		public int GetHashCode(string obj)
		{
			return obj.ToUpper().GetHashCode();
		}

		#endregion
	}
}
