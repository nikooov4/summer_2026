using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Mephi.Cybernetics.Race.Core.Tests.TestDomains.Shapes
{
    public interface IShape
    {
        Vector2 Position { get; }
        float Area { get; }
    }

}
