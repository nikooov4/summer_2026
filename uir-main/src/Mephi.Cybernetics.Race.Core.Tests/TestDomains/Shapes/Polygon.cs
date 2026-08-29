using System.Numerics;

namespace Mephi.Cybernetics.Race.Core.Tests.TestDomains.Shapes
{

    public abstract class Polygon : IShape
    {
        protected Polygon(Vector2 position)
        {
            Position = position;
        }

        public Vector2 Position { get; }
        public abstract float Area { get; }

        public abstract uint AngleCount { get; }
    }

    public abstract class Quadrangle : Polygon
    {
        protected Quadrangle(Vector2 position) : base(position)
        {
        }

        public override uint AngleCount => 4;

        public abstract float[] Sides { get; }

    }

    public class Rectangle : Quadrangle
    {
        private float[] _sides;

        public Rectangle(Vector2 position, float length, float height) : base(position)
        {
            Length = length;
            Height = height;
        }

        public float Length { get; }
        public float Height { get; }

        public override float Area => Length * Height;

        public override float[] Sides => _sides ??= new float[4] { Length, Height, Length, Height };
    }

    public class Trapeze : Quadrangle
    {
        private float[] _sides;

        public Trapeze(Vector2 position, float higherBase, float rightSide, float lowerBase, float leftSide) :
            base(position)
        {
            HigherBase = higherBase;
            RightSide = rightSide;
            LowerBase = lowerBase;
            LeftSide = leftSide;
        }

        public float LowerBase { get; }
        public float HigherBase { get; }
        public float LeftSide { get; }
        public float RightSide { get; }

        public override float Area => throw new System.NotImplementedException();

        public override float[] Sides => _sides ??= new float[4] { HigherBase, RightSide, LowerBase, LeftSide };
    }
}
