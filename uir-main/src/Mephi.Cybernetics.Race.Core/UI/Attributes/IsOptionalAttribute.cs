
namespace Mephi.Cybernetics.Race.Core.UI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class IsOptionalAttribute : System.Attribute
    {
        public IsOptionalAttribute() : this(true)
        {
        }
        public IsOptionalAttribute(bool isOptional)
        {
            IsOptional = isOptional;
        }
        public bool IsOptional
        {
            get;
        }
    }
}
