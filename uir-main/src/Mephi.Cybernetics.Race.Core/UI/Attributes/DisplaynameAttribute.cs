
namespace Mephi.Cybernetics.Race.Core.UI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Interface)]
    public class DisplayNameAttribute : System.Attribute
    {
        public DisplayNameAttribute(string displayPropertyName)
        {
            this.DisplayPropertyName = displayPropertyName;
        }

        public string DisplayPropertyName
        {
            get;
        }
    }
}
