using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Mephi.Cybernetics.Race.DataAccess.Configuration
{
	public class EntityAttributeConfigurationCollection : Dictionary<string, EntityAttributeConfiguration>
	{
	}

	public class EntityAttributeConfiguration
	{
		private string _name;
		private string _paramaters;

		public static EntityAttributeConfiguration Create(XmlNode node)
		{
			EntityAttributeConfiguration attribute = new EntityAttributeConfiguration();

			if ( node.Name != "Attribute" )
			{
				EntityConfigurationError err = new EntityConfigurationError(ErrorType.InvalidXmlDocument, "A start element expected");
				return null;
			}
			attribute._name = node.Attributes["name"].InnerXml;

			attribute._paramaters = node.Attributes["parameters"].InnerXml;

			return attribute;
		}

		private EntityAttributeConfiguration()
		{
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public string Paramaters
		{
			get
			{
				return _paramaters;
			}
		}

		//public string Compile()
		//{
		//    string code = "";
		//    code = "[" + Name;
		//    if ( Paramaters != null && Paramaters != "" )
		//        code += "(" + Paramaters + ")";
		//    code += "]";
		//    return code;
		//}
	}
}
