using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Sql
{
    public class MsSqlDataSaver : ISaver
    {
        #region ISaver Members

        public EntityDescription ReflectedType
        {
            get { throw new NotImplementedException(); }
        }

        public System.Data.DataSet UnderlyngDataSet
        {
            get { throw new NotImplementedException(); }
        }

        public PersistentStorage Ps
        {
            get { throw new NotImplementedException(); }
        }

        public void Save(EntityBase entity)
        {
            throw new NotImplementedException();
        }

        public void Save(List<EntityBase> entities)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISaver Members


        public void Save(StorageCommand command)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
