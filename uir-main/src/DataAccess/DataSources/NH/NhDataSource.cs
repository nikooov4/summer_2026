using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.Driver;


namespace Mephi.Cybernetics.Race.DataAccess.DataSources.NH
{
    public class NhDataSource : DataSource
    {
        private readonly string _connectionString;

        public ISession Session { get; set; }

        public NHibernate.ITransaction Transaction { get; set; }

        public NhDataSource(string connectionString, PersistentStorage ps) : base(ps)
        {
            _connectionString = connectionString;
        }

        public override void Open()
        {
            var cfg = new NHibernate.Cfg.Configuration();

            cfg.DataBaseIntegration(x =>
            {
                x.ConnectionString = _connectionString;
                x.Driver<SqlClientDriver>();
                x.Dialect<MsSql2008Dialect>();
            });

            cfg.AddAssembly(Assembly.GetExecutingAssembly());

            var sefact = cfg.BuildSessionFactory();

            Session = sefact.OpenSession();

            Transaction = Session.BeginTransaction();
        }

        public override void Save(List<StorageCommand> command)
        {
            foreach (var entity in command.SelectMany(storageCommand => storageCommand.Entities))
            {
                Session.Save(entity);
            }
        }

        public override void Save(StorageCommand command)
        {
            foreach (var entity in command.Entities)
            {
                Session.Save(entity);
            }
        }

        public override void Flush()
        {
            Transaction.Commit();
        }

        public override void Close()
        {
            if (Transaction.WasCommitted)
            {
                Transaction.Dispose();
            }
        }

        protected override ILoader GetLoaderInner(EntityDescription entityDescription)
        {
            return null;
        }

        protected override ISaver GetSaverInner(EntityDescription entityDescription)
        {
            return null;
        }

        public override void Dispose()
        {
            if (Session.IsConnected)
            {
                Session.Disconnect();
            }
            // Закрывать все соединения и т.п.
        }
    }
}