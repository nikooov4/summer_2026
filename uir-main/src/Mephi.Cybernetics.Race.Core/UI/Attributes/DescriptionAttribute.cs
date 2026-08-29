namespace Mephi.Cybernetics.Race.Core.UI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class DescriptionAttribute : System.Attribute
    {
        public DescriptionAttribute(string description)
        {
            this.Description = description;
        }

        public string Description
        {
            get;
        }
    }
}