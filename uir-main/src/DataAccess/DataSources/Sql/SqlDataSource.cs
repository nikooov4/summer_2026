using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Sql
{
    public class SqlDataSource : DataSource
    {
        private string _connectionString;

        public SqlDataSource(string connectionString, PersistentStorage ps) : base(ps)
        {
            _connectionString = connectionString;
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        public override void Save(List<StorageCommand> command)
        {
            throw new NotImplementedException();
        }

        public override void Save(StorageCommand command)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        protected override ILoader GetLoaderInner(EntityDescription entityDescription)
        {
            var sqlDataLoader = new MsSqlDataLoader(_ps, _connectionString, GetMapper(entityDescription));
            return sqlDataLoader;
        }

        protected override ISaver GetSaverInner(EntityDescription entityDescription)
        {
            var sqlSaver = new MsSqlDataSaver();
            return sqlSaver;
        }

        public override void Dispose()
        {
            // Закрывать все соединения и т.п.
        }

    }
}