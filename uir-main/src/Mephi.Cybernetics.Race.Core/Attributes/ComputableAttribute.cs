using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mephi.Cybernetics.Race.Core.Attributes
{
    public class ComputableAttribute : Attribute
    {
        public ComputableAttribute()
        {
            DependencyProperties = new ReadOnlyCollection<string>(new List<string>());
        }

        public ComputableAttribute(bool allowCaching = true)
        {
            AllowCaching = allowCaching;
        }

        public ComputableAttribute(bool allowCaching, params string[] dependencyProperties) : this(allowCaching)
        {
            DependencyProperties = new ReadOnlyCollection<string>(new List<string>(dependencyProperties)); // OMG... stupid read-only thing
        }

        public ReadOnlyCollection<string> DependencyProperties { get; }

        public bool AllowCaching { get; }
    }
}
