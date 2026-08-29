using Mephi.Cybernetics.Race.Core.UI;

namespace Mephi.Cybernetics.Race.Gui.Wpf
{
    public class UiElementInfo
    {
        public UiElementInfo(PropertyDisplayInfo displayedProperty, object captionControl, object valueControl, bool showCaptionControl = true)
        {
            DisplayedProperty = displayedProperty;
            CaptionControl = captionControl;
            ShowCaptionControl = showCaptionControl;
            ValueControl = valueControl;
        }

        public PropertyDisplayInfo DisplayedProperty { get; }
        public object CaptionControl { get; }
        public bool ShowCaptionControl { get; }
        public object ValueControl { get; }
    }
}
