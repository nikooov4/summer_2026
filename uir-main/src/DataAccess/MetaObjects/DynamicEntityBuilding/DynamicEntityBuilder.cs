using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.DynamicEntityBuilding;
using System.Xml;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public class DynamicEntityBuilder: EntityBase, IObjectFactory
	{
		private string _name;
		private string _xmlSource;
		private bool _sourceAssigned = false;
		private Type _type;

		public DynamicEntityBuilder()
		{
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public string XmlSource
		{
			get
			{
				return _xmlSource;
			}
			set
			{
				if ( _sourceAssigned )
					throw new InvalidOperationException("The value for the property XmlSource have already been set");
				_xmlSource = value;
				if ( Build() )
					_sourceAssigned = true;
				else
					throw new System.IO.FileLoadException("The entity description cannot be parsed");
			}
		}

		public Type Type
		{
			get
			{
				return _type;
			}
		}

		public object CreateObject(Type type)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public object CreateObject(Type type, params object[] args)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		private bool Build()
		{
			var sr = new StringReader(_xmlSource);
			var reader = XmlReader.Create(sr);
			var ded = DynamicEntityDescription.Create(reader);
			var code = ded.Compile();
			var codeProvider = new CSharpCodeProvider();
			var param = new CompilerParameters(new string[] { Assembly.GetExecutingAssembly().Location });
			var res = codeProvider.CompileAssemblyFromSource(param, new string[] { code });
			_type = res.CompiledAssembly.GetType(ded.FullName);
			RegisterFactory();
			return true;
		}

		private void RegisterFactory()
		{
			if ( _type == null )
				throw new System.IO.FileLoadException("The entity description cannot be parsed");
            //ObjectDescription.SetObjectFactory(_type, this);
		}
	}
}
