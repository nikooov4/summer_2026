using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public class Adapter
	{
		private IConnection _connection;

		public Adapter(IConnection connection)
		{
			_connection = connection;
		}

		public IConnection Connection
		{
			get
			{
				return _connection;
			}
		}
	}
}
