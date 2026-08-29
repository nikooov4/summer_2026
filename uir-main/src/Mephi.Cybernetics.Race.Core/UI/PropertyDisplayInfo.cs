using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.Core.UI.Attributes;
using DefaultValueAttribute = Mephi.Cybernetics.Race.Core.UI.Attributes.DefaultValueAttribute;
using DescriptionAttribute = Mephi.Cybernetics.Race.Core.UI.Attributes.DescriptionAttribute;
using DisplayNameAttribute = Mephi.Cybernetics.Race.Core.UI.Attributes.DisplayNameAttribute;

namespace Mephi.Cybernetics.Race.Core.UI
{
    public class PropertyDisplayInfo
    {
        public static PropertyDisplayInfo FromPropertyDescription(PropertyDescription property)
        {
            var propertyDisplayInfo = new PropertyDisplayInfo(property)
            {
                DisplayName = property.GetAttribute<DisplayNameAttribute>()?.DisplayPropertyName ??
                              property.GetAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName ?? property.Name,
                Description = property.GetAttribute<DescriptionAttribute>()?.Description ?? 
                              property.GetAttribute<System.ComponentModel.DescriptionAttribute>()?.Description ?? "",
                IsHidden = property.GetAttribute<IsHiddenAttribute>()?.IsHidden ?? false,
                PropertyIndex = property.GetAttribute<PropertyIndexAttribute>()?.PropertyIndex ?? -1,
                PropertyGroupName = property.GetAttribute<PropertyGroupAttribute>()?.Group ?? "",
                IsOptional = property.GetAttribute<IsOptionalAttribute>()?.IsOptional ?? false,
                DefaultValue = property.GetAttribute<DefaultValueAttribute>()?.DefaultValue ?? Optional<object>.None,
                IsReadOnly = property.IsReadOnly || property.HasAttribute<ReadOnlyAttribute>() || property.HasAttribute<DisplayReadOnlyAttribute>()
            };
            return propertyDisplayInfo;
        }

        public static PropertyDisplayInfo FromDomainAttribute(IDomainAttribute attribute)
        {
            if (attribute is PropertyDescription propertyDescription)
                return FromPropertyDescription(propertyDescription);
            throw new NotImplementedException();
        }

        private readonly IDomainAttribute _attribute;

        private PropertyDisplayInfo(IDomainAttribute attribute)
        {
            _attribute = attribute;
        }

        [Pure]
        public string Name => _attribute.Name;

        [Pure]
        public string DisplayName
        {
            get;
            private set;
        }

        [Pure]
        public string Description
        {
            get;
            private set;
        }

        [Pure]
        public Type ReturnType => _attribute.Domain.GetType();

        [Pure]
        public bool IsReadOnly { get; private set; }

        [Pure]
        public bool IsHidden
        {
            get;
            private set;
        }

        [Pure]
        public int PropertyIndex
        {
            get;
            private set;
        }


        [Pure]
        public string PropertyGroupName
        {
            get;
            private set;
        }

        [Pure]
        public PropertiesGroup PropertyGroup
        {
            get;
            internal set;
        }

        [Pure]
        public bool IsOptional
        {
            get;
            private set;
        }

        [Pure]
        public Optional<object> DefaultValue
        {
            get;
            private set;
        }

        [Pure]
        public object GetPropertyValue(object obj)
        {
            if (null != obj)
                return _attribute?.GetValue(obj);
            return null;
        }

        //Портировано из Race [автор А. Першина]
        public static IDomain[] GetMinListOfDomian(IDomain[] listDomain)
        {
            var mapDomainWithSubTypeList = new Dictionary<IDomain, HashSet<IDomain>>();

            foreach (IDomain dm in listDomain)
            {
                mapDomainWithSubTypeList.Add(dm, new HashSet<IDomain>());
                foreach (IDomain dm2 in listDomain)
                {
                    if (dm != dm2 && (dm2 as ObjectDescription).IsSubtypeOf(dm as ObjectDescription))
                    {
                        HashSet<IDomain> setDm;
                        mapDomainWithSubTypeList.TryGetValue(dm, out setDm);
                        setDm.Add(dm2);
                    }
                }
            }
            var resList = mapDomainWithSubTypeList.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToArray();
            return resList;
        }

    }
}