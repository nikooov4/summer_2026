using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.Tests.DataStructures
{
    [IsEntity]
    public class TriggerEvent
    {
        [IdField]
        public virtual  Guid ID { get; set; }
        public virtual  int EventDayNumber { get; set; }
        public virtual  int EventDayIndex { get; set; }
        public virtual  bool IsValid { get; set; }
        public virtual  DateTime EventTime { get; set; }

        public virtual IList<EventValue> Values { get; protected set; } = new List<EventValue>();

        public virtual  object GetValue(PhysicalVariable variable, int[] indices)
        {
            throw new NotImplementedException();
        }


    }
}