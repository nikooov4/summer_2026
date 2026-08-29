using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public class DataLoadStrategy
    {
        public IList<IEntity> LoadAll(Type type)
        {
            throw new NotImplementedException();
        }

        public IList<IEntity> LoadById(Type type, object id)
        {
            throw new NotImplementedException();
        }

        public IList<IEntity> LoadRange(Type type, string fieldName, IComparable min, IComparable max)
        {
            throw new NotImplementedException();
        }

        public IList<IEntity> LoadRange(Type type, string fieldName, object min, object max, Comparison<object> comparison)
        {
            throw new NotImplementedException();
        }

        public IList<IEntity> LoadAll(IEnumerable<Type> types)
        {
            throw new NotImplementedException();
        }

        private void OtoRelationships(List<IEntity> entities)
        {
            
        }

        private void OtmRelationships(List<IEntity> entities)
        {

        }

        private void MtmRelationships(List<IEntity> entities)
        {

        }
    }

}
