using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.XPath;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.MetaData
{
    public class TableMapping
    {
        private readonly EntityDescription _entityDescription;
        private readonly Dictionary<string, string> _mapping;

        public TableMapping(XPathNavigator mappingRootNode, Dictionary<string, TableAttributeInfo> properties)
        {
            _mapping = new Dictionary<string,string>();
            mappingRootNode.MoveToChild("mappingRootNode", "");
            
            _entityDescription = EntityDescription.GetEntityDescription(Assembly.Load(mappingRootNode.SelectSingleNode("assembly").Value).GetType(mappingRootNode.SelectSingleNode("target-class").Value));
            foreach (XPathNavigator mappingNode in mappingRootNode.SelectChildren("map", ""))
            {
                _mapping.Add(mappingNode.GetAttribute("source", ""), mappingNode.GetAttribute("target", ""));
            }
        }

        public Dictionary<string, string> Mapping
        {
            get { return _mapping; }
        }
        public EntityDescription Type
        {
            get { return _entityDescription; }
        }
    }
}
