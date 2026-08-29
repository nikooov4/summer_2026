using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using Mephi.Cybernetics.Race.DataAccess.DataSources.MetaData;
using Mephi.Cybernetics.Race.DataAccess.Intension;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources
{
    public abstract class DataSource : IDisposable
    {
        protected readonly PersistentStorage _ps;
        protected readonly Dictionary<string, DsTypeInfo> MappedTypes = new Dictionary<string,DsTypeInfo>();
        private readonly List<DataSource> _dependencySources = new List<DataSource>();
        private readonly Dictionary<EntityDescription, ILoader> _loaders = new Dictionary<EntityDescription, ILoader>();
        private readonly Dictionary<EntityDescription, ISaver> _savers = new Dictionary<EntityDescription, ISaver>();
        protected string _name;
        protected readonly Dictionary<EntityDescription,Mapper> _mappers = new Dictionary<EntityDescription, Mapper>();

        public static DataSource GetDataSource(string fileName)
        {
            return null;
        }

        protected DataSource(PersistentStorage ps)
        {
            _ps = ps;
        }

        public List<DataSource> DependencySources
        {
            get { return _dependencySources; }
        }

        public string Name
        {
            get { return _name; }
        }

        public abstract void Open();

        public void Save(object entity)
        {
            throw new NotImplementedException();
        }
        public void Save(ICollection<object> entities)
        {
            throw new NotImplementedException();
        }
        public void Save(IEntity entity)
        {
            throw new NotImplementedException();
        }
        public void Save(ICollection<IEntity> entities)
        {
            throw new NotImplementedException();
        }

        // public abstract void Save(StorageTransaction command);
        public abstract void Save(List<StorageCommand> command);
        public abstract void Save(StorageCommand command);
        public abstract void Flush();
        public abstract void Close();

        public Mapper GetMapper(EntityDescription ed)
        {
            lock ( _mappers )
            {
                if (!_mappers.ContainsKey(ed))
                    _mappers.Add(ed, new Mapper(ed, null));
            }
            return _mappers[ed];
        }

        public ILoader GetLoader<T>()
        {
            return GetLoader(EntityDescription.GetEntityDescription<T>());
        }

        public ILoader GetLoader(EntityDescription entityDescription)
        {
            if (!_loaders.ContainsKey(entityDescription))
                lock (_loaders)
                    _loaders.Add(entityDescription, GetLoaderInner(entityDescription));
            return _loaders[entityDescription];
        }

        protected abstract ILoader GetLoaderInner(EntityDescription entityDescription);

        public ISaver GetSaver<T>()
        {
            return GetSaver(EntityDescription.GetEntityDescription<T>());
        }

        public ISaver GetSaver(EntityDescription entityDescription)
        {
            if (!_savers.ContainsKey(entityDescription))
                lock (_savers)
                    _savers.Add(entityDescription, GetSaverInner(entityDescription));
            return _savers[entityDescription];
        }

        public IEnumerable<IEntity> GetEntites()
        {
            return _ps.Select(e => e.Description.Source == this);
        }

        protected abstract ISaver GetSaverInner(EntityDescription ed);

        public override string ToString()
        {
            return Name;
        }

        public virtual void Dispose()
        {
            _dependencySources.Clear();
            _mappers.Clear();
            _savers.Clear();
            _loaders.Clear();
        }

        protected void ReadXmlConfig(XmlReader xmlReader)
        {
            var primitiveTypes =
                new List<Type>(new Type[]
                                   {
                                       //typeof(Delegate), 
                                       typeof(object), 
                                       //typeof(ValueType), 
                                       //typeof(Nullable), 
                                       typeof(byte), 
                                       typeof(Int16), 
                                       typeof(Int32), 
                                       typeof(Int64),
                                       typeof(UInt16), 
                                       typeof(UInt32), 
                                       typeof(UInt64), 
                                       typeof(Char), 
                                       typeof(DateTime), 
                                       typeof(float), 
                                       typeof(double),
                                       typeof(bool), 
                                       typeof(string)
                                       //typeof(Enum), 
                                       //typeof(Attribute)
                                   });
            var xpath = new XPathDocument(xmlReader).CreateNavigator();
            if ( xpath.Name != "types" )
                if (!xpath.MoveToChild("types", ""))
                    throw new FormatException("Xml configuration file is invalid: unable to find the 'types' section");

            foreach (XPathNavigator typeNode in xpath.SelectChildren("entityDescription", ""))
            {
                var typeId = typeNode.GetAttribute("id", "");
                var type = DataSourceManifest.GetTypeFromAssembly(typeNode.SelectSingleNode("assembly").Value, typeNode.SelectSingleNode("name").Value);
                var typeInfo = new DsTypeInfo()
                {
                    Id = typeId,
                    Name = type.Name,
                    ManagedType = type,
                    EntDescription =
                        EntityDescription.IsEntity(type) ? EntityDescription.GetEntityDescription(type) : null,
                    IsAtomic = primitiveTypes.Contains(type)
                };
                this.MappedTypes.Add(typeInfo.Id, typeInfo);
                if (typeNode.MoveToFollowing("hook", ""))
                {
                    var mapper = new Mapper(typeInfo.EntDescription, typeNode.ReadSubtree());
                    _mappers.Add(typeInfo.EntDescription, mapper);
                }
            }


        }

        protected class DsTypeInfo
        {

            public string Id { get; internal set; }
            public string Name { get; internal  set; }
            public Type ManagedType { get; internal  set; }
            public EntityDescription EntDescription { get; internal  set; }
            public bool IsAtomic { get; internal  set; }

            public bool IsEntity
            {
                get { return EntDescription != null; }
            }
        }
    }

    //public class MappedTypesCollection : IEnumerable<Type>
    //{
    //    public 

    //    public IEnumerator<Type> GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }
    //}
}
