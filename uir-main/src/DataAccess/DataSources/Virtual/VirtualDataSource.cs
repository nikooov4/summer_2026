using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Virtual
{
    public class VirtualDataSource : DataSource
    {
        private LoadAllDelegate _loadAll;
        private LoadEntityByIdDelegate _loadEntityById;
        private LoadByFieldValueDelegate _loadByFieldValue;
        private ReloadEntityDelegate _reloadEntity;
        private SaveDelegate _saveMethod;
        private bool _inited = false;
        
        public VirtualDataSource(PersistentStorage ps, LoadAllDelegate loadAll, LoadEntityByIdDelegate loadEntityById, LoadByFieldValueDelegate loadByFieldValue, 
                                 ReloadEntityDelegate reloadEntity, SaveDelegate saveMethod) : base(ps)
        {
            Init(loadAll, loadEntityById, loadByFieldValue, reloadEntity, saveMethod);
        }

        protected void Init(LoadAllDelegate loadAll, LoadEntityByIdDelegate loadEntityById, LoadByFieldValueDelegate loadByFieldValue, ReloadEntityDelegate reloadEntity, SaveDelegate saveMethod)
        {
            _loadAll = loadAll;
            _loadEntityById = loadEntityById;
            _loadByFieldValue = loadByFieldValue;
            _reloadEntity = reloadEntity;
            _saveMethod = saveMethod;
            _inited = true;
        }

        protected VirtualDataSource(PersistentStorage ps) : base(ps)
        {
            
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
            CheckInited();
            var virtualLoader = new VirtualLoader(_ps, GetMapper(entityDescription), _loadAll, _loadEntityById, _loadByFieldValue, _reloadEntity);
            return virtualLoader;
        }

        private void CheckInited()
        {
            if ( !_inited )
                throw new InvalidOperationException("The data source was not initializedcorrectly. Constructor should call the Init method.");
        }

        protected override ISaver GetSaverInner(EntityDescription ed)
        {
            CheckInited();
            var virtualSaver = new VirtualSaver(_ps, ed, _saveMethod);
            return virtualSaver;
        }

        public override void Dispose()
        {
        }
    }
}