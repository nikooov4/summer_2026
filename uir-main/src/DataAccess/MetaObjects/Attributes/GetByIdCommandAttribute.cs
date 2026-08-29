using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes
{
	public class GetByIdCommandAttribute : Attribute
	{
		private string _command;
		private string _idParameterString;

		public GetByIdCommandAttribute(string command, string idParameterString)
		{
			_command = command;
			_idParameterString = idParameterString;
		}

		public string Command
		{
			get
			{
				return _command;
			}
		}

		public string GetCommand(string parameterValue)
		{
			return _command.Replace(_idParameterString, parameterValue);
		}

		public string GetCommand(object parameterValue)
		{
			string pv = "";
			if ( parameterValue is string )
				pv = "'" + (parameterValue as string) + "'";
			else if ( parameterValue is Guid )
				pv = "'" + parameterValue.ToString() + "'";
			else
				pv = parameterValue.ToString();
			return _command.Replace(_idParameterString, pv);
		}
		
		public string GetCommand()
		{
			return _command;
		}
		
		public string GetIdParameterName()
		{
			return _idParameterString;			
		}
	}
}
