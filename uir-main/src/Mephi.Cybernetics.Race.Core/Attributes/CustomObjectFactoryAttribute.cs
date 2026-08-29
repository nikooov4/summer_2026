using System;

namespace Mephi.Cybernetics.Race.Core.Attributes
{
    [Serializable]
    public class CustomObjectFactoryAttribute : Attribute
	{
		private IObjectFactory _factory;

		public CustomObjectFactoryAttribute(IObjectFactory factory)
		{
			_factory = factory;
		}

		public IObjectFactory Factory
		{
			get
			{
				return _factory;
			}
		}
	}
}
