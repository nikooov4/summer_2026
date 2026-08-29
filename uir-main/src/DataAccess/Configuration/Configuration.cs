using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Mephi.Cybernetics.Race.DataAccess.Configuration
{
	public class Configuration
	{
		private Dictionary<string, EntityConfiguration> _conf = new Dictionary<string, EntityConfiguration>();
		
		public static Configuration Create(XmlReader reader)
		{
			XmlReaderSettings sett = new XmlReaderSettings();
			sett.IgnoreComments = true;
			sett.IgnoreProcessingInstructions = true;
			sett.IgnoreWhitespace = true;

			XmlReader rdr = XmlReader.Create(reader, sett);

			XmlDocument doc = new XmlDocument();
			doc.Load(rdr);

			if (doc.ChildNodes[0].Name != "Entities")
			{
				
				EntityConfigurationError err = new EntityConfigurationError(ErrorType.UnexpectedNode, "Expected node 'Entities', but '" + doc.Name + "' found");
				return null;
			}

			Configuration conf = new Configuration();

			foreach (XmlNode node in doc.ChildNodes[0].ChildNodes)
			{
				EntityConfiguration entity = EntityConfiguration.Create(node);
				if (entity != null)
					conf.Entities.Add(node.Attributes["name"].Value, entity);
			}
			return conf;
		}

		public Dictionary<string, EntityConfiguration> Entities
		{
			get
			{
				return _conf;
			}
		}
	}
}
