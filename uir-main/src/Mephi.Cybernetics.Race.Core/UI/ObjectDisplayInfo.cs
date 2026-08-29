using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mephi.Cybernetics.Race.Core.TypeSystem;

namespace Mephi.Cybernetics.Race.Core.UI
{
    public class ObjectDisplayInfo
    {
        public ObjectDisplayInfo(IStrictTypeSystem typeSystem, IDomain domain)
        {
            TypeSystem = typeSystem;
            Domain = domain;
            Properties = typeSystem.GetDomainAttributes(domain)
                .Cast<PropertyDescription>()
                .Select(PropertyDisplayInfo.FromPropertyDescription)
                .ToList();
        }

        public IStrictTypeSystem TypeSystem { get; }

        public IDomain Domain { get; }

        public string DisplayName { get; }

        public IReadOnlyCollection<PropertyDisplayInfo> Properties { get; }

        public PropertyDisplayInfoCollection DisplayedProperties { get; }
    }

    public class PropertyDisplayInfoCollection //: IReadOnlyCollection<PropertyDisplayInfo>
    {
        private readonly PropertyDisplayGroupMode _groupMode;

        public PropertyDisplayInfoCollection(ICollection<PropertyDisplayInfo> properties, PropertyDisplayGroupMode groupMode = PropertyDisplayGroupMode.Hierarchy, PropertyDisplaySortMode sortMode = PropertyDisplaySortMode.ByIndex)
        {
            All = new PropertyDisplayInfoList(properties, sortMode);
            _groupMode = groupMode;
        }
        public IReadOnlyCollection<PropertiesGroup> Groups { get; }

        public PropertyDisplayInfoList WithoutDisplayGroup { get; }

        public PropertyDisplayInfoList All { get; }

        // public IEnumerator<PropertyDisplayInfo> GetEnumerator()
        // {
        //     throw new System.NotImplementedException();
        // }
        //
        // IEnumerator IEnumerable.GetEnumerator()
        // {
        //     return GetEnumerator();
        // }
        //
        // public int Count => throw new System.NotImplementedException();
    }

    public class PropertyDisplayInfoList : IReadOnlyCollection<PropertyDisplayInfo>
    {
        private readonly ICollection<PropertyDisplayInfo> _properties;

        public PropertyDisplayInfoList(ICollection<PropertyDisplayInfo> properties, PropertyDisplaySortMode mode = PropertyDisplaySortMode.ByIndex)
        {
            Mode = mode;
            _properties = properties;
        }

        public PropertyDisplaySortMode Mode { get; }

        public PropertyDisplayInfo this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator<PropertyDisplayInfo> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => throw new NotImplementedException();
    }

    public enum PropertyDisplaySortMode
    {
        None,
        ByIndex,
        ByName,
        ByDisplayName
    }
}