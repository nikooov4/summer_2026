using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.DataSources.Virtual;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests.IntegrationTests
{
    [TestFixture]
    public class PersistentStorageTests
    {
        private List<IEntity> _sampleList;
        private PersistentStorage _ps;

        [SetUp]
        public void SetUp()
        {
            _sampleList = new List<IEntity>();
            for ( var i = 0; i < 100; i++ )
            {
                var sc = new SemanticConstruction
                {
                    ID = Guid.NewGuid(),
                    Name = "SC_" + i
                };
                _sampleList.Add(sc);
            }         

            _ps = new PersistentStorage();

            var collectionDs = new CollectionDataSource(null, _sampleList);
            _ps.AddDataSource(collectionDs);
            collectionDs.Open();
        }

        [Test]
        public void TestLoadAllGeneric()
        {
            var context = _ps.CreateChildContext();
            var loadAllList = context.GetEntities<SemanticConstruction>();
            Assert.AreEqual(_sampleList.Count, loadAllList.Count);
            foreach ( var entity in loadAllList )
            {
                Predicate<IEntity> byId = e => e.ID == entity.ID;
                Assert.IsTrue(_sampleList.Exists(byId));
                Assert.IsTrue(_sampleList.Exists(byId).Implies(_sampleList.Find(byId)["Name"] == entity["Name"]));
            }
        }

        [Test]
        public void TestLoadAll()
        {
            var context = _ps.CreateChildContext();
            var loadAllList = context.GetEntities(typeof(SemanticConstruction));
            Assert.AreEqual(_sampleList.Count, loadAllList.Count);
            foreach ( var entity in loadAllList )
            {
                Predicate<IEntity> byId = e => e.ID == entity.ID;
                Assert.IsTrue(_sampleList.Exists(byId));
                Assert.IsTrue(_sampleList.Exists(byId).Implies(_sampleList.Find(byId)["Name"] == entity["Name"]));
            }
        }
        [Test]
        public void TestLoadById()
        {

        }

        [Test]
        public void TestLoadByFieldValue()
        {

        }
    }
}
