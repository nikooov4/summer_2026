using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes
{
    public class DbSchemaAttribute : Attribute
    {
        string _schemaName = "";

        public DbSchemaAttribute(string schemaName)
        {
            _schemaName = schemaName;
        }

        public string SchemaName
        {
            get
            {
                return _schemaName;
            }
        }
    }
}
