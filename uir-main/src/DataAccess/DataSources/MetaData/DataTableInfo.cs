using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.MetaData
{
    public class DataTableInfo
    {
        private readonly string _name;
        private readonly Dictionary<string, TableAttributeInfo> _properties;
        private readonly TableMapping _mapping;

        public DataTableInfo(XPathNavigator table)
        {
            _properties = new Dictionary<string, TableAttributeInfo>();
            _name = table.GetAttribute("name", "");
            foreach (XPathNavigator property in table.Select("attributes/*"))
            {
                _properties.Add(property.GetAttribute("name", ""), new TableAttributeInfo(property));
            }

            _mapping = new TableMapping(table, _properties);
        }

        public TableMapping Mapping
        {
            get { return _mapping; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Dictionary<string, TableAttributeInfo> Properties
        {
            get { return _properties; }
        }
    }
}
