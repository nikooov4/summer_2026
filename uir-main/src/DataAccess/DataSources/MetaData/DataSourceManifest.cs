using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;


namespace Mephi.Cybernetics.Race.DataAccess.DataSources.MetaData
{
    public class DataSourceManifest
    {
        public static Type GetTypeFromAssembly(string assemblyName, string typeName)
        {
            Assembly assembly = null;
            foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
                if (loadedAssembly.GetName().Name == assemblyName)
                {
                    assembly = loadedAssembly;
                    break;
                }
            if (assembly == null)
                assembly = Assembly.Load(assemblyName);
            var type = assembly.GetType(typeName);
            return type;
        }

        private readonly Dictionary<string, DataTableInfo> _sourceTables;
        private readonly Dictionary<string, EntityDescription> _exportedTypes;

        public DataSourceManifest(XPathNavigator navigator)
        {
            _sourceTables = new Dictionary<string, DataTableInfo>();
            _exportedTypes = new Dictionary<string, EntityDescription>();
            var metaPart = navigator;
            while (metaPart.MoveToParent()) { }
            metaPart.MoveToChild("root", "");
            metaPart.MoveToChild("meta-data", "");
            foreach (XPathNavigator tableNode in metaPart.SelectChildren("data-table", ""))
                _sourceTables.Add(tableNode.GetAttribute("name", ""), new DataTableInfo(tableNode));

            metaPart.MoveToChild("types", "");
            foreach (XPathNavigator entityTypeNode in metaPart.SelectChildren("type", ""))
            {
                var assemblyName = entityTypeNode.SelectSingleNode("assembly").Value;
                var typeName = entityTypeNode.SelectSingleNode("name").Value;
                var type = GetTypeFromAssembly(assemblyName, typeName);
                // The assembly is not loaded
                _exportedTypes.Add(entityTypeNode.GetAttribute("id", ""), EntityDescription.GetEntityDescription(type));
            }
        }

        public Dictionary<string, DataTableInfo> SourceTables
        {
            get { return _sourceTables; }
        }

        public Dictionary<string, EntityDescription> ExportedTypes
        {
            get { return _exportedTypes; }
        }
    }
}
