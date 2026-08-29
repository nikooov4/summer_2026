using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public interface IDataSaveStrategy
    {
        void Save(List<EntityBase> entities);
        void Save(EntityBase entity);
    }
}