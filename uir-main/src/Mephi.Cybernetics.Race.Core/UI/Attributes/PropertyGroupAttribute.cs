

namespace Mephi.Cybernetics.Race.Core.UI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class PropertyGroupAttribute : System.Attribute
    {
        private string _group;
        public string Group
        {
            get
            {
                return _group;
            }
            set {
                this._group = value;
            }
        }

        public PropertyGroupAttribute(string group)
        {
            this.Group = group;
        }

    }
}
