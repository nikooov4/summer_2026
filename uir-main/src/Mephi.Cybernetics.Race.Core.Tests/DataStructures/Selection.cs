using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.Tests.DataStructures
{
    [IsEntity]
    public class Selection
    {
        [IdField]
        public virtual Guid ID { get; set; }
        [IdField(2)]
        public virtual string Name { get; set; }
        [IdField(2)]
        [IdField(3)]
        public virtual string Code { get; set; }
        [IdField(3)]
        public virtual string Description { get; set; }
        //сделать атрибут много ко многим , связка между сущностями - SelectionData
        [IsManyToMany("Analystics", "SelectionsData", "SelectionID", "EventID")]
        //[RelationshipName("blah-blah-blah")]
        //[IsManyToMany("Mephi.Cybernetics.Race.Core.Tests.DataStructures", nameof(SelectionData), nameof(SelectionData.Selection), nameof(SelectionData.Event))]
        public virtual IList<TriggerEvent> Events { get; protected set; } = new List<TriggerEvent>();
        public virtual TriggerEvent Event { get; protected set; } 
        

    }
}