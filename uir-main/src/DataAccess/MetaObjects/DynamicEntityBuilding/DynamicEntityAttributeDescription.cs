using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.DynamicEntityBuilding
{
	public class DynamicEntityAttributeDescriptionCollection : Dictionary<string, DynamicEntityAttributeDescription>
	{
	}

	public class DynamicEntityAttributeDescription
	{
		private string _name;
		private string _paramaters;

		public static DynamicEntityAttributeDescription Create(XmlNode node)
		{
			DynamicEntityAttributeDescription attribute = new DynamicEntityAttributeDescription();

			if ( node.Name != "Attribute" )
			{
				DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.InvalidXmlDocument, "A start element expected");
				return null;
			}
			attribute._name = node.Attributes["name"].InnerXml;

			attribute._paramaters = node.InnerXml;

			return attribute;
		}

		private DynamicEntityAttributeDescription()
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

		public string Compile()
		{
			string code = "";
			code = "[" + Name;
			if ( Paramaters != null && Paramaters != "" )
				code += "(" + Paramaters + ")";
			code += "]";
			return code;
		}
	}
}
