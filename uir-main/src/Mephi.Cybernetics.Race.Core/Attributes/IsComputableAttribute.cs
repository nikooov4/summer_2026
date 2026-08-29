using System;

namespace Mephi.Cybernetics.Race.Core.Tests.TestDomains.PartsProduction
{
    public class IsComputableAttribute : Attribute
    {
        public IsComputableAttribute() : this(null)
        {
        }

        public IsComputableAttribute(params string[] names)
        {
            Names = names;
        }

        public string[] Names { get; }
    }
}