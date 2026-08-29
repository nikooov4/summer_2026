using System;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.Tests.DataStructures
{
    [IsEntity]
    [IsRelationship]
    public class SelectionData
    {
        [ParticipantCardinality(1,Int32.MaxValue)]
        public virtual TriggerEvent Event { get; set; }
        [ParticipantCardinality(1,Int32.MaxValue)]
        public virtual Selection Selection { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is SelectionData selectionData)
            {
                return Event.ID == selectionData.Event.ID && Selection.ID == selectionData.Selection.ID;
            }

            return false;
        }

        public override int GetHashCode()
        {
#if NET472
            return Event.ID.GetHashCode() ^ Selection.ID.GetHashCode();
#else
            return HashCode.Combine(Event.ID, Selection.ID);
#endif
        }
    }
}