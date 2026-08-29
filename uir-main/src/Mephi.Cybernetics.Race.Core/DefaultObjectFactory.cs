using System;

namespace Mephi.Cybernetics.Race.Core
{
	public class DefaultObjectFactory: IObjectFactory
	{
		public object CreateObject(Type type)
		{
			return Activator.CreateInstance(type);
		}

		public object CreateObject(Type type, params object[] args)
		{
			return Activator.CreateInstance(type, args);
		}
	}
}
