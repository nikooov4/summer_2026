using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.DataSources.Virtual;
using Mephi.Cybernetics.Race.DataAccess.DataSources.Xml;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataSources
{
    [TestFixture]
    public class XmlDataSourceTests
    {

        private List<EntityBase> _sampleBaseList;
        private CollectionDataSource _collectionDsBase;
        private XmlDataSource _xmlDataSource;
        private string _xmlFilePath;

        [SetUp]
        public void SetUp()
        {
            _xmlFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                string.Format("..{0}..{0}..{0}..{0}etc{0}tests{0}datasources{0}testSaveXml.xml",
                Path.DirectorySeparatorChar));
            _sampleBaseList = new List<EntityBase>();
            for (var i = 0; i < 2; i++)
            {
                var sc = new SemanticConstruction
                {
                    ID = Guid.NewGuid(),
                    Name = "SC_" + i

                };
                if (EntityDescription.IsEntityBase(sc.GetType()))
                    _sampleBaseList.Add((EntityBase)sc);
                else
                    _sampleBaseList.Add((EntityBase)sc.Description);

            }
            XmlTextWriter textWritter = new XmlTextWriter(_xmlFilePath, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("root");
            textWritter.WriteEndElement();
            textWritter.Close();

            _xmlDataSource = new XmlDataSource(_xmlFilePath, "root", null);
        }

        [Test]
        public void TestSaveInsert()
        {
            _xmlDataSource.Open();
            foreach (var entity in _sampleBaseList)
            {
                _xmlDataSource.GetSaver(entity.Description.EntityDescriptor).Save(entity);
            }
            _xmlDataSource.Close();

            var fs = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fs);
            var root = xmlDoc.DocumentElement;
            XmlNodeList elemList = root.GetElementsByTagName("Name");
            Assert.AreEqual(_sampleBaseList.Count, elemList.Count);
            fs.Close();

        }
        [Test]
        public void TestSaveUpdate()
        {
            _xmlDataSource.Open();
            foreach (var entity in _sampleBaseList)
            {
                _xmlDataSource.GetSaver(entity.Description.EntityDescriptor).Save(entity);
            }
            _xmlDataSource.Close();

            foreach (var entity in _sampleBaseList)
            {
                entity["Name"] = entity["Name"] + "_Update";
                entity.Update();
            }

            foreach (var entity in _sampleBaseList)
            {
                _xmlDataSource.Open();
                _xmlDataSource.GetSaver(entity.Description.EntityDescriptor).Save(entity);
                _xmlDataSource.Close();
            }

            var fs = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fs);
            var root = xmlDoc.DocumentElement;
            XmlNodeList elemList = root.GetElementsByTagName("Name");
            Assert.AreEqual(_sampleBaseList.Count - 1, elemList.Count);

            foreach (var entity in _sampleBaseList)
            {
                _xmlDataSource.Open();
                _xmlDataSource.GetSaver(entity.Description.EntityDescriptor).Save(entity);
                _xmlDataSource.Close();
            }
            fs.Close();

        }
        [Test]
        public void TestSaveDelete()
        {
            _xmlDataSource.Open();
            foreach (var entity in _sampleBaseList)
            {
                _xmlDataSource.GetSaver(entity.Description.EntityDescriptor).Save(entity);
            }
            _xmlDataSource.Close();
            _sampleBaseList[0].Update();
            _sampleBaseList[0].Delete();

            _xmlDataSource.Open();
            _xmlDataSource.GetSaver(_sampleBaseList[0].Description.EntityDescriptor).Save(_sampleBaseList[0]);
            _xmlDataSource.Close();

            var fs = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fs);
            var root = xmlDoc.DocumentElement;
            XmlNodeList elemList = root.GetElementsByTagName("Name");
            Assert.AreEqual(_sampleBaseList.Count - 1, elemList.Count);
            fs.Close();
        }
    }
}
