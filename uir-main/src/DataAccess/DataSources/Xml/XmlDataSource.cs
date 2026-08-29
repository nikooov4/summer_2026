using Mephi.Cybernetics.Race.DataAccess.DataSources.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Xml;
using System.Xml.XPath;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Xml
{
    public class XmlDataSource : DataSource
    {
        private readonly string _fileName;
        private readonly string _entitiesTagPath;
        private XmlDocument _document;
        private FileStream _fileController = null;
        private bool _clean = false;
        private bool _opened = false;
        private XPathNavigator _rootNavogator;
        private readonly List<XmlReader> xmlReaders = new List<XmlReader>();
        private DataSourceManifest _metadata;

        internal DataSourceManifest Metadata
        {
            get { return _metadata; }
        }

        public XmlDataSource(string fileName, string entitesTagPath, PersistentStorage ps) : base(ps)
        {
            _fileName = fileName;
            _entitiesTagPath = entitesTagPath;
            _name = Path.GetFileNameWithoutExtension(_fileName);
        }

        public override void Open()
        {
            if (!_opened)
            {
                CreateFileController();
                _document = new XmlDocument();
                _document.Load(_fileController);
                _rootNavogator = XmlUtils.NavigateToNode(_entitiesTagPath, _document.DocumentElement.CreateNavigator());
                XPathNavigator rtNavigator = _rootNavogator.Clone();
                _metadata = new DataSourceManifest(rtNavigator);
                _opened = true;
            }
        }

        private void CreateFileController()
        {
            if (_fileController == null || !_fileController.CanRead)
            {

                _fileController = new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                _clean = false;

            }
        }

        public override void Save(List<StorageCommand> command)
        {
            foreach (var entity in command.SelectMany(storageCommand => storageCommand.Entities))
            {
                Save(entity);
            }
        }

        public override void Save(StorageCommand command)
        {
            foreach (var entity in command.Entities)
            {
                Save(entity);
            }
        }

        public override void Flush()
        {
            _fileController.Seek(0, SeekOrigin.Begin);
            _fileController.SetLength(1);
            var xmlWriterSetting = new XmlWriterSettings
                                       {
                                           CloseOutput = false,
                                           Indent = true,
                                           NewLineHandling = NewLineHandling.Entitize
                                       };
            var xmlWriter = XmlWriter.Create(_fileController, xmlWriterSetting);
            _document.Save(xmlWriter);
        }

        public override void Close()
        {
            Flush();
            CleanUp();
            _opened = false;
        }

        private void CleanUp()
        {
            if (!_clean)
            {
                _document = null;
                foreach ( var xmlReader in xmlReaders )
                    xmlReader.Close();
                xmlReaders.Clear();
                _fileController.Close();
                _clean = true;
            }
        }

        protected override ILoader GetLoaderInner(EntityDescription type)
        {
            CreateFileController();
            var xmlReaderSettings = new XmlReaderSettings
                                        {
                                            CloseInput = true,
                                            IgnoreComments = true,
                                            IgnoreProcessingInstructions = false,
                                            IgnoreWhitespace = false
                                            
                                        };
            var readOnlyStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = XmlReader.Create(readOnlyStream, xmlReaderSettings);
            xmlReaders.Add(reader);
            XmlUtils.NavigateThisToNode(reader, _entitiesTagPath);
            //XmlUtils.NavigateThisToNode(reader, GetMapper(ed).TableName);
            var xmlLoader = new XmlDataLoader(reader, GetMapper(type), this, _ps);
            return xmlLoader;
        }

        protected override ISaver GetSaverInner(EntityDescription ed)
        {
            Open();
            var navigator = XmlUtils.NavigateToNodeOrCreate(GetMapper(ed).TableName, _rootNavogator);
            var xmlSaver = new XmlDataSaver(navigator, ed, this);
            return xmlSaver;
        }

        public override void Dispose()
        {
            CleanUp();
            base.Dispose();
        }

        public DataTableInfo GetTable(EntityDescription type)
        {
            foreach (var table in _metadata.SourceTables)
            {
                if (table.Value.Mapping.Type == type)
                {
                    return table.Value;
                }
            }
            return null;
        }

    }
}