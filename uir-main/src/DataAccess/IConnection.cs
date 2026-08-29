using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public interface IConnection
	{
		string ConnectionString {get;}
		void Open();
		void Close();
		ITransaction OpenTransaction();
		void CloseTransaction();
	}
}
