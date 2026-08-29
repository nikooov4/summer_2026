using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.DataSources.Virtual;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataSources
{
    [TestFixture]
    public class CollectionDataSourceTests
    {
        private List<IEntity> _sampleList;
        private CollectionDataSource _collectionDs;

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
            _collectionDs = new CollectionDataSource(null, _sampleList);
            _collectionDs.Open();
        }

        [Test]
        public void TestLoadAll()
        {
            var loadAllList = _collectionDs.GetLoader(EntityDescription.GetEntityDescription(typeof (SemanticConstruction))).LoadAll();
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
            foreach ( var entity in _sampleList )
            {
                var foundEntity = _collectionDs.GetLoader(entity.Description.EntityDescriptor).LoadEntityById(entity.ID);
                Assert.AreEqual(entity.ID, foundEntity.ID);
                Assert.AreEqual(entity["Name"], foundEntity["Name"]);
//                Assert.IsFalse(entity == foundEntity);
//                Assert.IsFalse(entity.Equals(foundEntity));
            }
        }

        [Test]
        public void TestLoadByFieldValue()
        {
            foreach ( var entity in _sampleList )
            {
                var foundRsults = _collectionDs.GetLoader(entity.Description.EntityDescriptor).LoadByFieldValue("Name", entity["Name"]);
                Assert.AreEqual(1, foundRsults.Count);
                foreach ( var foundEntity in foundRsults )
                {
                    Assert.AreEqual(entity.ID, foundEntity.ID);
                    Assert.AreEqual(entity["Name"], foundEntity["Name"]);
//                    Assert.IsFalse(entity == foundEntity);
//                    Assert.IsFalse(entity.Equals(foundEntity));
                }
            }
        }
    }
}
