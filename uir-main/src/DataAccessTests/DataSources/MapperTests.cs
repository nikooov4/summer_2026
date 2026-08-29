using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataSources
{
    [TestFixture]
    public class MapperTests
    {
        [Test]
        public void TestReadEntityMappingFromXml()
        {
            var ed = EntityDescription.GetEntityDescription<SemanticConstruction>();
            var xmlFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                string.Format("..{0}..{0}..{0}..{0}etc{0}tests{0}datasources{0}testOneEntityMapping.xml",
                    Path.DirectorySeparatorChar));
            var mapper = new Mapper(typeof (SemanticConstruction), XmlReader.Create(File.OpenRead(xmlFilePath)));
            Assert.AreEqual(typeof(SemanticConstruction), mapper.MappedEntity);
            Assert.AreEqual("SCs", mapper.TableName);
            Assert.AreEqual("User1", mapper.SchemaName);
            Assert.AreEqual("sName", mapper["Name"]);
            Assert.AreEqual("sName", mapper[ed.Attributes["Name"]]);
            Assert.AreEqual("idParent", mapper[ed.Attributes["Parent"]]);
        }
    }
}
