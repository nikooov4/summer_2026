
namespace Mephi.Cybernetics.Race.Core.UI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class IsHiddenAttribute : System.Attribute
    {
        public bool IsHidden {
            get;
        }

        public IsHiddenAttribute(bool isHidden)
        {
            this.IsHidden = isHidden;
        }
    }
}
