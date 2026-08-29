using System;
using System.Numerics;

namespace Mephi.Cybernetics.Race.Core.Tests.TestDomains.Shapes
{

    public class Ellipse : IShape
    {
        public Ellipse(Vector2 position, float largerHalfAxis, float smallerHalfAxis)
        {
            Position = position;
            LargerHalfAxis = largerHalfAxis;
            SmallerHalfAxis = smallerHalfAxis;
        }

        public Vector2 Position { get; }

        public virtual float LargerHalfAxis { get; }
        public virtual float SmallerHalfAxis { get; }

        public float Area => LargerHalfAxis * SmallerHalfAxis * (float)Math.PI;
    }

    public class Circle : Ellipse
    {
        public Circle(Vector2 position, float radius) : base(position, radius, radius)
        {
            Radius = radius;
        }

        public float Radius { get; }

        private new float LargerHalfAxis => base.LargerHalfAxis;
        private new float SmallerHalfAxis => base.LargerHalfAxis;
    }
}