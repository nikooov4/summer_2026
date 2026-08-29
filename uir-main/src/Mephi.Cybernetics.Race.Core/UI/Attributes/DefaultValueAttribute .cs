using System;

namespace Mephi.Cybernetics.Race.Core.UI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class DefaultValueAttribute : System.Attribute
    {
        public object DefaultValue
        {
            get;
        }

        public DefaultValueAttribute(Object value)
        {
            this.DefaultValue = value;
        }

        public DefaultValueAttribute(int value)
        {
            this.DefaultValue = value;
        }

    }
}
