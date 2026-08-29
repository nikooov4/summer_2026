using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Virtual
{
    public delegate void SaveDelegate(StorageCommand command);

    public class VirtualSaver : ISaver
    {

        private readonly PersistentStorage _ps;
        private readonly EntityDescription _entityDescription;
        private SaveDelegate _doSave;

        public VirtualSaver(PersistentStorage ps, EntityDescription entityDescription, SaveDelegate saveMethod)
        {
            _ps = ps;
            _entityDescription = entityDescription;
            _doSave = saveMethod;
        }

        public EntityDescription ReflectedType
        {
            get
            {
                return _entityDescription;
            }
        }

        public System.Data.DataSet UnderlyngDataSet
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PersistentStorage Ps
        {
            get
            {
                return _ps;
            }
        }

        public void Save(StorageCommand command)
        {
            throw new NotImplementedException();
        }

        public void Save(EntityBase entity)
        {
            throw new NotImplementedException();
        }

        public void Save(List<EntityBase> entities)
        {
            throw new NotImplementedException();
        }
    }
}