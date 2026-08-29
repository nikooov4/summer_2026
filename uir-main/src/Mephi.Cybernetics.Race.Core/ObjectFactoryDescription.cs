using System;

namespace Mephi.Cybernetics.Race.Core
{
	public enum ObjectFactoryType
	{
		Class,
		Delegate
	}

	public delegate object DefaultConstruction(Type type);
	public delegate object ConstructionWithArgs(Type type, params object[] args);

	public class ObjectFactoryDescription
	{
		private ObjectFactoryType _factoryType;
		private IObjectFactory _objectFactory = null;
		private DefaultConstruction _default;
		private ConstructionWithArgs _params;

		public ObjectFactoryDescription(IObjectFactory objectFactory)
		{
			_objectFactory = objectFactory;
			_factoryType = ObjectFactoryType.Class;
		}

		public ObjectFactoryDescription(DefaultConstruction defaultConstruction,
			ConstructionWithArgs constructionWithArgs)
		{
			_default = defaultConstruction;
			_params = constructionWithArgs;
			_factoryType = ObjectFactoryType.Delegate;
		}

		public ObjectFactoryType FactoryType
		{
			get
			{
				return _factoryType;
			}
		}

		public object InvokeDefaultConstruction(Type type)
		{
			if ( FactoryType == ObjectFactoryType.Class )
				return _objectFactory.CreateObject(type);
			if ( FactoryType == ObjectFactoryType.Delegate )
				return _default(type);
			return null;
		}

		public object InvokeConstructionWithArgs(Type type, params object[] args)
		{
			if ( FactoryType == ObjectFactoryType.Class )
				return _objectFactory.CreateObject(type, args);
			if ( FactoryType == ObjectFactoryType.Delegate )
				return _params(type, args);
			return null;
		}
	}
}
