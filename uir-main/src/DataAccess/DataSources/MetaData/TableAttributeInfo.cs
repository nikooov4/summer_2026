using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.MetaData
{
    public class TableAttributeInfo
    {
        private readonly string _name;
        private readonly string _typeId;
        private readonly bool _isOneToOne;
        private readonly bool _isOneToMany;
        private readonly bool _isManyToMany;
        private readonly string _referencedTable;
        private readonly string _mtmLinkingTable;


        public TableAttributeInfo(XPathNavigator property)
        {
            _name = property.GetAttribute("name", "");
            _typeId = property.GetAttribute("type", "");
            property.MoveToParent();
            property.MoveToParent();
            foreach (XPathNavigator reference in property.Select("references/*"))
            {
                if (reference.SelectSingleNode("source-field").Value == _name)
                {
                    switch (reference.GetAttribute("type", ""))
                    {
                        case "1:1":
                            _isOneToOne = true;
                            break;
                        case "1:N":
                            _isOneToMany = true;
                            break;
                        case "N:1":
                            _isOneToOne = true;
                            break;
                        case "N:N":
                            _isManyToMany = true;
                            _mtmLinkingTable = reference.SelectSingleNode("linking-table").Value;
                            break;
                    }
                    _referencedTable = reference.GetAttribute("target", "");  
                }
            }
        }

        public string Name
        {
            get { return _name; }
        }
        public string TypeId
        {
            get { return _typeId; }
        }
        public bool IsOneToOne
        {
            get { return _isOneToOne; }
        }

        public bool IsOneToMany
        {
            get { return _isOneToMany; }
        }

        public bool IsManyToMany
        {
            get { return _isManyToMany; }
        }

        public string ReferencedTable
        {
            get { return _referencedTable; }
        }

        public string MtmLinkingTable
        {
            get { return _mtmLinkingTable; }
        }
    }
}
