using System;
using System.Collections.Generic;
using System.Text;

namespace Avant.DPO
{
    public class Connection
    {
        private string _connectionString;

        public Connection()
        {
        }

        public Connection(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}
