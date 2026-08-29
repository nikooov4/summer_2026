using System;
using System.Collections;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.Tests.DataStructures
{
    [IsEntity()]
    public abstract class EventValue
    {
        public virtual TriggerEvent Event { get; set; }
        public virtual PhysicalVariable Variable { get; set; }
        public virtual decimal ValueDecimal { get; set; }

        public  abstract int[] Indices { get; }

        public  abstract bool Matches(PhysicalVariable variable);

        public  override bool Equals(object obj)
        {
            if (obj is EventValue eventValue)
            {
                return Event.ID == eventValue.Event.ID && Variable.ID == eventValue.Variable.ID 
                                                     && StructuralComparisons.StructuralEqualityComparer.Equals(Indices,eventValue.Indices) 
                                                     && ValueDecimal == eventValue.ValueDecimal;
            }

            return false;
        }

        public override int GetHashCode()
        {
#if NET472
            return Event.ID.GetHashCode() ^ Variable.ID.GetHashCode() ^ Indices.GetHashCode() ^ ValueDecimal.GetHashCode();
#else
            return HashCode.Combine(Event.ID,Variable.ID,Indices,ValueDecimal);
#endif
        }
    }
    [IsEntity()]
    public class EventValueScalar : EventValue
    {
        public  override int[] Indices => Array.Empty<int>();

        public  override bool Matches(PhysicalVariable variable)
        {
            return variable.Dimension == 0;
        }
    }
    [IsEntity()]
    public  class EventValue1D : EventValue
    {
        public virtual int Index { get; set; }

        public  override int[] Indices => new[] { Index };

        public  override bool Matches(PhysicalVariable variable)
        {
            return variable.Dimension == 1;
        }
    }
    [IsEntity()]
    public  class EventValue2D : EventValue
    {
        public virtual int Index1 { get; set; }
        public virtual int Index2 { get; set; }
        public  override int[] Indices => new[] { Index1, Index2 };

        public  override bool Matches(PhysicalVariable variable)
        {
            return variable.Dimension == 2;
        }
    }
    [IsEntity()]
    public  class EventValue3D : EventValue
    {
        public virtual int Index1 { get; set; }
        public virtual int Index2 { get; set; }
        public virtual int Index3 { get; set; }
        public  override int[] Indices => new[] { Index1, Index2, Index3 };

        public  override bool Matches(PhysicalVariable variable)
        {
            return variable.Dimension == 3;
        }
    }
}