using System;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.Tests.DataStructures
{
    [IsEntity()]
    public  class PhysicalVariable
    {
        public virtual  Guid ID { get; set; }
        public virtual  string Name { get; set; }
        public virtual  string Code { get; set; }
        public virtual  string Description { get; set; }
        public virtual  bool IsScalar { get; set; }
        public virtual  int Dimension { get; set; }
    }
}