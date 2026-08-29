using System;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    public abstract class ConstraintAttribute : Attribute
    {
        public abstract string ErrorMessage { get; }
    }
}