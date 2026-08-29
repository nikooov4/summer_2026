using Mephi.Cybernetics.Race.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.Core.Tests.TestDomains.GenericExample
{
    [IsEntity]
    public class SomeEntity1
    {
    }

    [IsEntity]
    public class SomeEntity2
    {
    }

    [IsEntity]
    public class SomeEntity3
    {
    }

    [IsEntity]
    public class SomeEntity4
    {
    }

    [IsRelationship]
    public class SomeRelationship
    {
        [ParticipantCardinality(0,1)]
        public SomeEntity1 Participant1 { get; }

        [ParticipantCardinality(1,int.MaxValue)]
        public List<SomeEntity2> Participant2 { get; }

        [ParticipantCardinality(3,5)]
        public List<SomeEntity3> Participant3 { get; }

        [ParticipantCardinality(1,1)]
        public SomeEntity4 Participant4 { get; }
    }
}
