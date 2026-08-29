using Mephi.Cybernetics.Race.DataAccess.DataSources.MetaData;
using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.Core;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects
{
    //TODO: Отделить от ObjectDescription и сделать IDomain'ом
    public class DataObjectDescription: ObjectDescription
    {
        private readonly DataTableInfo _dataTable;
        internal DataTableInfo DataTable
        {
            get { return _dataTable; }
        }

        public DataObjectDescription(Type type, DataTableInfo dataTable)
        {
            _dataTable = dataTable;
        }
    }
}
