using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Mephi.Cybernetics.Race.DataAccess.Configuration
{
	public class EntityConfiguration
	{
		private string _name;
		private List<string> _usings = new List<string>();
		private string _namespace;
		private Dictionary<string, EntityPropertyConfiguration> _properties = new Dictionary<string, EntityPropertyConfiguration>();
		private EntityAttributeConfigurationCollection _attributes = new EntityAttributeConfigurationCollection();

		public static EntityConfiguration Create(XmlNode node)
		{
			//XmlReaderSettings sett = new XmlReaderSettings();
			//sett.IgnoreComments = true;
			//sett.IgnoreProcessingInstructions = true;
			//sett.IgnoreWhitespace = true;

			//XmlReader rdr = XmlReader.Create(reader, sett);

			//XmlDocument doc = new XmlDocument();
			//doc.Load(rdr);

			//if ( doc.ChildNodes.Count != 1 )
			//{
			//    EntityConfigurationError err = new EntityConfigurationError(ErrorType.InvalidXmlDocument, "The xml doceument must only one node describing an entity");
			//    return null;
			//}
			//XmlNode node = doc.ChildNodes[0];

			EntityConfiguration entity = new EntityConfiguration();

			entity._name = node.Attributes["name"].Value;

			foreach ( XmlNode n in node.ChildNodes )
			{
				if ( n.Name == "Usings" )
				{
					foreach ( XmlNode nn in n.ChildNodes )
					{
						if ( nn.Name == "Using" )
							entity._usings.Add(nn.InnerXml);
						else
						{
							EntityConfigurationError err = new EntityConfigurationError(ErrorType.UnexpectedNode, string.Format("Node 'Using' expected but '{0}' found", node.FirstChild.Name));
							return null;
						}
					}
				}
				else if ( n.Name == "Namespace" )
				{
					entity._namespace = n.InnerXml;
				}
				else if ( n.Name == "ClrAttributes" )
				{
					foreach ( XmlNode nn in n.ChildNodes )
					{
						EntityAttributeConfiguration attribute = EntityAttributeConfiguration.Create(nn);
						if ( nn != null )
							entity._attributes.Add(attribute.Name, attribute);
						else
						{
							EntityConfigurationError err = new EntityConfigurationError(ErrorType.InvalidStructural, "The 'Attribute' object was not build correctly");
							return null;
						}
					}
				}
				else if ( n.Name == "Properties" )
				{
					foreach ( XmlNode nn in n.ChildNodes )
					{
						EntityPropertyConfiguration property = EntityPropertyConfiguration.Create(nn);
						if ( property != null )
							entity._properties.Add(property.Name, property);
						else
						{
							EntityConfigurationError err = new EntityConfigurationError(ErrorType.InvalidStructural, "The 'Property' object was not build correctly");
							return null;
						}
					}
				}
				else
				{
					EntityConfigurationError err = new EntityConfigurationError(ErrorType.UnexpectedNode, string.Format("Node 'Usings', 'Namespace', 'ClrAttributes' or 'Properties' expected but '{0}' found", node.FirstChild.Name));
					return null;
				}
			}

			return entity;
		}

		private EntityConfiguration()
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

		//public List<string> Usings
		//{
		//    get
		//    {
		//        return _usings;
		//    }
		//    set
		//    {
		//        _usings = value;
		//    }
		//}

		//public string Namespace
		//{
		//    get
		//    {
		//        if ( __namespace == null )
		//            __namespace = "";
		//        return __namespace;
		//    }
		//    set
		//    {
		//        __namespace = value;
		//    }
		//}

		public Dictionary<string, EntityPropertyConfiguration> Properties
		{
			get
			{
				return _properties;
			}
			set
			{
				_properties = value;
			}
		}

		public EntityAttributeConfigurationCollection Attributes
		{
			get
			{
				return _attributes;
			}
			set
			{
				_attributes = value;
			}
		}

		//public string FullName
		//{
		//    get
		//    {
		//        if ( Namespace == "" )
		//            return Name;
		//        else
		//            return Namespace + "." + Name;
		//    }
		//}

		//public string Compile()
		//{
		//    string code = "";

		//    foreach ( string s in Usings )
		//        code += "using " + s + ";\r\n";

		//    if ( Namespace != null && Namespace != "" )
		//    {
		//        code += "namespace " + Namespace + "\r\n";
		//        code += "{\r\n";
		//    }

		//    foreach ( string s in ClrAttributes.Keys )
		//        code += ClrAttributes[s].Compile();

		//    code += "public class " + Name + " : Mephi.Cybernetics.Race.DataAccess.EntityBase\r\n";
		//    code += "{\r\n";

		//    code += "public " + Name + "()\r\n";
		//    code += "{\r\n}\r\n";

		//    foreach ( string s in Properties.Keys )
		//        code += Properties[s].Compile() + "\r\n";

		//    code += "}\r\n";

		//    if ( Namespace != null && Namespace != "" )
		//        code += "}\r\n";

		//    return code;
		//}
	}
}
