

namespace Mephi.Cybernetics.Race.Core.UI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class PropertyIndexAttribute : System.Attribute
    {
        public int PropertyIndex
        {
            get;
            set;
        }

        public PropertyIndexAttribute(int propertyIndex)
        {
            this.PropertyIndex = propertyIndex;
        }
    }
}
