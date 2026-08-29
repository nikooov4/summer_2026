using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.UI
{
    public class PropertiesGroup
    {
        public PropertiesGroup(ICollection<PropertyDisplayInfo> properties, PropertyDisplayGroupMode groupMode, PropertyDisplaySortMode sortMode)
        {
            
        }

        public IReadOnlyCollection<PropertyDisplayInfo> DisplayedProperties { get; }

        public IReadOnlyCollection<PropertiesGroup> SubGroups { get; }

    }

    public enum PropertyDisplayGroupMode
    {
        None,
        Flat,
        Hierarchy
    }
}