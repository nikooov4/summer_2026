using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Mephi.Cybernetics.Race.DataAccess.Configuration
{
	public enum ErrorType
	{
		Unknown,
		InvalidXmlDocument,
		UnexpectedNode,
		InvalidStructural
	}

	public class EntityConfigurationError
	{
		private ErrorType _errorType = ErrorType.Unknown;
		private string _message;

		public EntityConfigurationError()
		{
		}

		public EntityConfigurationError(string message)
		{
			_message = message;
		}

		public EntityConfigurationError(ErrorType errorType, string message)
		{
			_errorType = errorType;
			_message = message;
		}

		public string Message
		{
			get
			{
				return _message;
			}
		}

		public ErrorType ErrorType
		{
			get
			{
				return _errorType;
			}
		}

		public override string ToString()
		{
			string res = string.Format("Error type: '{0}', Message: '{1}'", _errorType.ToString(), _message);
			return res;
		}
	}
}
