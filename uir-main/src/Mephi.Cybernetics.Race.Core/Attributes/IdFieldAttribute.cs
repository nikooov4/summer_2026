using System;

namespace Mephi.Cybernetics.Race.Core.Attributes
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class IdFieldAttribute : Attribute
	{
        public IdFieldAttribute(int potentialKeyId = 1) 
        {
            PotentialKeyId = potentialKeyId;
        }

        public int PotentialKeyId { get; }
    }
}
