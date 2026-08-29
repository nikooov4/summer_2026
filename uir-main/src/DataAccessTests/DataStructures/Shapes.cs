using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using NUnit.Framework.Constraints;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataStructures
{
    public class Figure
    {
        public static PropertyDescription FullNameProperty => ObjectDescription.Get<Figure>().Properties[nameof(FullName)];
        public string[] FullName { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public FigureType Type { get; set; }
    }

    public enum FigureType
    {
        Ellipse,
        Circle,
        Rectangle
    }

    public class Shape
    {
        public string Label
        {
            get;
            set;
        }
    }

    public class Ellipse : Shape
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int LongSemiAxis { get; set; }
        public int ShortSemiAxis { get; set; }
    }

    public interface ILocatable
    {
        Point Location { get; }
    }

    public interface IMovableHardcore
    {
        Point Location { set; }
    }

    public interface IMovable
    {
        Point Location { get; set; }
    }

    public interface IDrawable: ILocatable, IMovable
    {
        bool IsDrawn { get; }
        //void Draw(Point location, Size clip);
    }

    public interface IFigure : IDrawable
    {
        
    }

    public class Ellipse3 : Shape, IFigure
    {
        public Point Location { get; set; }
        public Size Semiaxises { get; set; }
        public bool IsDrawn { get; set; }
    }

    public class Ellipse2 : Shape, ILocatable, IMovable
    {
        public Point Location { get; set; }
        public Size Semiaxises { get; set; }
    }

    public class Circle : Shape
    {
        private static Circle _defaultCircle;
        public static Circle DefaultCircle {
            get
            {
                if (_defaultCircle == null)
                    _defaultCircle = new Circle()
                    {
                        Radius = 1,
                        X = 0,
                        Y = 0
                    };
                return _defaultCircle;
            }
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }

        [Computable]
        public double Space => Math.PI * Radius * Radius;
    }

    public class Circle2 : Circle
    {
        private static readonly Circle2 _defaultCircle = new Circle2(Color.Black) { X = 0, Y = 0, Radius = 1 };
        public static Circle2 DefaultCircle2 => _defaultCircle;

        private readonly Color _color;

        public Circle2(Color color)
        {
            _color = color;
        }

        public Color Color
        {
            get { return _color; }
        }
    }

    public class HollowEllipse : Ellipse2
    {
        public Size InternalSemiaxises { get; set; }
    }

    public interface IPoint<T>
    {
        T X { get; }
        T Y { get; }
    }

    public struct IntPoint : IPoint<int>
    {
        private readonly int _x;
        private readonly int _y;
        private readonly string _label;

        public IntPoint(int x, int y, string label)
        {
            _x = x;
            _y = y;
            _label = label;
        }

        public string Label
        {
            get { return _label; }
        }

        public int Y
        {
            get { return _y; }
        }

        public int X
        {
            get { return _x; }
        }
    }

    public struct RealPoint : IPoint<float>
    {

        public RealPoint(float x, float y, string label)
        {
            X = x;
            Y = y;
            Label = label;
        }

        public string Label { get; }

        public float Y { get; }

        public float X { get; }
    }
}
