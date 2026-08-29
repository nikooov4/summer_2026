using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using static Mephi.Cybernetics.Race.MappingDescriptionLanguage.Tests.Tests;

namespace Mephi.Cybernetics.Race.MappingDescriptionLanguage.Tests
{
    public class Tests
    {
        [Test]
        public void TestPropertyAccess()
        {
            string code = "@out.LongSemiAxis=@in.Size.Width";
         
            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<Ellipse2, Ellipse>(code);

            var inObj = new Ellipse2 { Location = new(2, 4), Size = new(31, 38) };
            var outObj = new Ellipse();
            lambda(inObj, outObj);
            Assert.AreEqual(inObj.Size.Width, outObj.LongSemiAxis);

        }

        [Test]
        public void TestObjectCreation()
        {
            string code = "@out.Size={Width:@in.LongSemiAxis, Height:@in.ShortSemiAxis}";

            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<Ellipse, Ellipse2>(code);

            var inObj = new Ellipse { LongSemiAxis = 10, ShortSemiAxis = 21, X = 2, Y = 8 };
            var outObj = new Ellipse2();
            lambda(inObj, outObj);
            Assert.AreEqual(new Size() { Height = inObj.ShortSemiAxis, Width = inObj.LongSemiAxis }, outObj.Size);
        }

        public class EllipseWrapper
        {
            public Ellipse2 Ellipse { get; set; }
        }

        [Test]
        public void TestComplexObjectCreation()
        {
            string code = "@out.Ellipse={Size:{Width:@in.LongSemiAxis, Height:@in.ShortSemiAxis}, Location:{X:@in.X, Y:@in.Y}}";

            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<Ellipse, EllipseWrapper>(code);

            var inObj = new Ellipse { LongSemiAxis = 10, ShortSemiAxis = 21, X = 2, Y = 8 };
            var outObj = new EllipseWrapper();
            lambda(inObj, outObj);
            Assert.AreEqual(new Size() { Height = inObj.ShortSemiAxis, Width = inObj.LongSemiAxis }, outObj.Ellipse.Size);
            Assert.AreEqual(new Point() { X = inObj.X, Y = inObj.Y}, outObj.Ellipse.Location);
        }


        //public class EllipseWrapper2
        //{
        //    public Ellipse4 Ellipse { get; set; }
        //}
        //[Test]
        //public void TestObjectCreationWithSelect()
        //{
        //    string code = "@out.Ellipse.IntList={Size:{Width:@in.LongSemiAxis, Height:@in.ShortSemiAxis}, Location:{X:@in.X, Y:@in.Y}}";

        //    var compiler = new CustomLanguageCompiler();
        //    var lambda = compiler.Compile<List<Ellipse>, EllipseWrapper2>(code);

        //    var inObj = new List<Ellipse>() { new() { LongSemiAxis = 10, ShortSemiAxis = 21, X = 2, Y = 8 }, new() { LongSemiAxis = 12, ShortSemiAxis = 41, X = 4, Y = -2 }, new() { LongSemiAxis = 56, ShortSemiAxis = 32, X = 0, Y = 2 } };
        //    var outObj = new EllipseWrapper2();
        //    lambda(inObj, outObj);
        //    Assert.AreEqual(new Size() { Height = inObj.ShortSemiAxis, Width = inObj.LongSemiAxis }, outObj.Ellipse.Size);
        //    Assert.AreEqual(new Point() { X = inObj.X, Y = inObj.Y }, outObj.Ellipse.Location);
        //}

        internal class ListEqualityComparer<T> : IEqualityComparer<List<T>>
        {
            public bool Equals(List<T>? x, List<T>? y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode([DisallowNull] List<T> obj)
            {
                return obj.Count;
            }
        }

        private bool ListOfListSequenceEqual<T>(List<List<T>> a, List<List<T>> b)
        {
            return a.SequenceEqual(b, new ListEqualityComparer<T>());
        }

        [Test]
        public void TestSelect()
        {
            string code = "@out.IntList = select(@in.Points, p => p.X)";

            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<Ellipse3, Ellipse4>(code);

            var inObj = new Ellipse3 { Points = new List<Point>() { new(10, 20), new(-1, -4), new(3, 20) } };
            var outObj = new Ellipse4();
            lambda(inObj, outObj);
            var x = inObj.Points.Select(p => p.X).ToList();
            Assert.IsTrue(outObj.IntList.SequenceEqual(x));
        }

        [Test]
        public void TestSelectInsideSelect()
        {
            string code = "@out.ListOfList = select(@in, e => select(e.Points, p => p.X))";

            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<List<Ellipse3>, Ellipse4>(code);

            var inObj = new List<Ellipse3>() { new Ellipse3 { Points = new List<Point>() { new(10, 20), new(-1, -4), new(3, 20) } }, new Ellipse3 { Points = new List<Point>() { new(9, 18), new(-11, -9), new(4, 2) } } };
            var outObj = new Ellipse4();
            lambda(inObj, outObj);

            Assert.IsTrue(ListOfListSequenceEqual(outObj.ListOfList, inObj.Select(o => o.Points.Select(p => p.X).ToList()).ToList()));
        }

        [Test]
        public void TestTranspose()
        {
            string code = "@out.ListOfList = transpose(select(@in, e => select(e.Points, p => p.X)))";
            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<List<Ellipse3>, Ellipse4>(code);

            var inObj = new List<Ellipse3>() { new Ellipse3 { Points = new List<Point>() { new(10, 20), new(-1, -4), new(3, 20) } }, new Ellipse3 { Points = new List<Point>() { new(9, 18), new(-11, -9), new(4, 2) } } };
            var outObj = new Ellipse4();
            lambda(inObj, outObj);

            Assert.IsTrue(ListOfListSequenceEqual(LanguageUtils.Transpose(outObj.ListOfList), inObj.Select(o => o.Points.Select(p => p.X).ToList()).ToList()));
        }

        public class Container
        {
            public bool IsTrue { get; set; }
        }

        [Test]
        public void TestEqualityOperator()
        {
            string code = "@out.IsTrue = @in.X == 1";
            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<Ellipse, Container>(code);

            var inObj = new Ellipse { LongSemiAxis = 10, ShortSemiAxis = 21, X = 1, Y = 8 };
            var outObj = new Container();
            lambda(inObj, outObj);

            Assert.AreEqual(true, outObj.IsTrue);
        }

        [Test]
        public void TestFilter()
        {
            string code = "@out.IntList = filter(select(@in.Points, p => p.X), x => x != 10)";

            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<Ellipse3, Ellipse4>(code);

            var inObj = new Ellipse3 { Points = new List<Point>() { new(10, 20), new(-1, -4), new(3, 20), new(10, 1), new (2, 9) } };
            var outObj = new Ellipse4();
            lambda(inObj, outObj);
            var x = inObj.Points.Select(p => p.X).Where(x => x != 10).ToList();
            Assert.IsTrue(outObj.IntList.SequenceEqual(x));
        }

        [Test]
        public void TestCount()
        {
            string code = "@out.Count = count(@in.Points)";

            var compiler = new CustomLanguageCompiler();
            var lambda = compiler.Compile<Ellipse3, Ellipse4>(code);

            var inObj = new Ellipse3 { Points = new List<Point>() { new(10, 20), new(-1, -4), new(3, 20), new(10, 1), new(2, 9) } };
            var outObj = new Ellipse4();
            lambda(inObj, outObj);
            var x = inObj.Points.Select(p => p.X).Where(x => x != 10).ToList();
            Assert.AreEqual(outObj.Count, inObj.Points.Count);
        }

    }
}