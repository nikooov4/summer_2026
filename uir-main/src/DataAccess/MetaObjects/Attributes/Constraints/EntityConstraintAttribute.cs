using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    public abstract class EntityConstraintAttribute : ConstraintAttribute
    {
        public abstract bool CheckEntity(EntityBase entity);
        public abstract bool CheckEntity(IEntity entity, Dictionary<string, object> values);
    }
}
