using NUnit.Framework;
using Mephi.Cybernetics.Race.DataAccess.DataSources.NH;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataSources
{
    [TestFixture]
    public class NHibernateDataSourceTests
    {
        private NhDataSource _nhDataSource;
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _connectionString = "Data Source " +
                                "= localhost\\SQLEXPRESS;" +
                                "Initial Catalog=NHibernateTest;" +
                                "Integrated Security=True;" +
                                "Pooling=False";
            _nhDataSource = new NhDataSource(_connectionString, null);
        }

        [Test]
        public void TestOpen()
        {
            _nhDataSource.Open();
            Assert.IsTrue(_nhDataSource.Session.IsConnected);
            Assert.IsTrue(_nhDataSource.Transaction.IsActive);
        }

        [Test]
        public void TestFlush()
        {
            _nhDataSource.Open();
            _nhDataSource.Flush();
            Assert.IsTrue(_nhDataSource.Transaction.WasCommitted);
        }

        [Test]
        public void TestClose()
        {
            _nhDataSource.Open();
            _nhDataSource.Flush();
            _nhDataSource.Close();
            Assert.IsFalse(_nhDataSource.Transaction.IsActive);
        }
    }
}