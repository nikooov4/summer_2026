using System;

namespace Mephi.Cybernetics.Race.Core
{
	public interface IObjectFactory
	{
		object CreateObject(Type type);
		object CreateObject(Type type, params object[] args);
	}
}
