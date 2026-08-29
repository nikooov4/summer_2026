using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.DomainModelMapping;
using Mephi.Cybernetics.Race.Core.ObjectMapping;
using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests
{
    [TestFixture]
    public class ObjectMappingTests
    {
        [Test]
        public void TestNonPersistentMapping()
        {

            // that part is required by the persistence feature
            var transformation = new Mapping<Ellipse, Ellipse2>();

            transformation.UsingCustomInstantiation(o => new Ellipse2());

            //transformation.TargetProperties["Location"].DependsOn("X", "Y");
            //transformation.TargetProperties["Semiaxises"].DependsOn("LongSemiAxis", "ShortSemiAxis");

            // that part is transformation itself
            transformation.TargetProperties["Label"].FromProperty<string,string>("Label", Mapping<string>.IdentityMapping);
            transformation.TargetProperties["Location"].UsingMap(source => new Point(source.X, source.Y));
            transformation.TargetProperties["Semiaxises"].UsingMap(source => new Size(source.LongSemiAxis, source.ShortSemiAxis));

            var ellipse = new Ellipse
                             {
                                 Label = "111",
                                 LongSemiAxis = 4,
                                 ShortSemiAxis = 3,
                                 X = 5,
                                 Y = -6
                             };
            var convertedEllipse = transformation.Convert(ellipse);
            Assert.AreEqual(ellipse.Label, convertedEllipse.Label);
            Assert.AreEqual(ellipse.X, convertedEllipse.Location.X);
            Assert.AreEqual(ellipse.Y, convertedEllipse.Location.Y);
            Assert.AreEqual(ellipse.LongSemiAxis, convertedEllipse.Semiaxises.Width);
            Assert.AreEqual(ellipse.ShortSemiAxis, convertedEllipse.Semiaxises.Height);

            var ell2 = new Ellipse2
                           {
                               Label = "444",
                               Location = new Point(11, 12),
                               Semiaxises = new Size(34, 45)
                           };
            transformation.MapOnto(ellipse, ref ell2);
            Assert.AreEqual(ellipse.Label, ell2.Label);
            Assert.AreEqual(ellipse.X, ell2.Location.X);
            Assert.AreEqual(ellipse.Y, ell2.Location.Y);
            Assert.AreEqual(ellipse.LongSemiAxis, ell2.Semiaxises.Width);
            Assert.AreEqual(ellipse.ShortSemiAxis, ell2.Semiaxises.Height);

            // Проверяем, что можно вычислять значения свойств по отдельности
            Assert.AreEqual(convertedEllipse.Label, transformation.TargetProperties["Label"].Map(ellipse));
            Assert.AreEqual(convertedEllipse.Location, transformation.TargetProperties["Location"].Map(ellipse));
        }

        [Test]
        public void TestTestNonPersistentMapping()
        {
            var transformation = MappingPoco<Ellipse, Ellipse2>.New().UsingCustomInstantiation(o => new Ellipse2());
            //var propertyMap = trans.IdentityMapping("Label", "Label");
            var ellipse = new Ellipse
            {
                Label = "111",
                LongSemiAxis = 4,
                ShortSemiAxis = 3,
                X = 5,
                Y = -6
            };
            transformation.Property("Location").UsingMap<Ellipse, Point>(source => new Point(source.X, source.Y));
            transformation.Property("Semiaxises").UsingMap<Ellipse, Size>(source => new Size(source.LongSemiAxis, source.ShortSemiAxis));

            var convertedEllipse = transformation.Map<Ellipse, Ellipse2>(ellipse);
           
            Assert.AreEqual(ellipse.Label, convertedEllipse.Label);
            Assert.AreEqual(ellipse.X, convertedEllipse.Location.X);
            Assert.AreEqual(ellipse.Y, convertedEllipse.Location.Y);
            Assert.AreEqual(ellipse.LongSemiAxis, convertedEllipse.Semiaxises.Width);
            Assert.AreEqual(ellipse.ShortSemiAxis, convertedEllipse.Semiaxises.Height);

            var ell2 = new Ellipse2
            {
                Label = "444",
                Location = new Point(11, 12),
                Semiaxises = new Size(34, 45)
            };
            transformation.MapOnto(ellipse, ref ell2);
            Assert.AreEqual(ellipse.Label, ell2.Label);
            Assert.AreEqual(ellipse.X, ell2.Location.X);
            Assert.AreEqual(ellipse.Y, ell2.Location.Y);
            Assert.AreEqual(ellipse.LongSemiAxis, ell2.Semiaxises.Width);
            Assert.AreEqual(ellipse.ShortSemiAxis, ell2.Semiaxises.Height);

            // Проверяем, что можно вычислять значения свойств по отдельности
            Assert.AreEqual(convertedEllipse.Label, transformation.Property("Label").Map(ellipse));
            Assert.AreEqual(convertedEllipse.Location, transformation.Property("Location").Map(ellipse));
        }

        [Test]
        public void TestTestTransformationsWithInterfaces()
        {
            var intPoints = new[]
            {
                new IntPoint(1,2,"1"),
                new IntPoint(2,2,"2"),
                new IntPoint(0,0,"3"),
                new IntPoint(2,1,"4"),
                new IntPoint(3,3,"5"),
            }
                .Cast<IPoint<int>>().ToArray();
            //var typeIPointInt = ObjectDescription.Get<IPoint<int>>();
            //var typeIPointFloat = ObjectDescription.Get<IPoint<float>>();
            var trans = MappingPoco<IPoint<int>, IPoint<float>>.New();
            trans.CustomMapping<IPoint<int>, IPoint<float>>(p => p is IntPoint ip ? new RealPoint(ip.X, ip.Y, ip.Label) : throw new ArgumentException(""));

            var realPoints = trans.Map(intPoints);

            Assert.AreEqual(intPoints.Length, realPoints.Count);
            for (var i = 0; i < intPoints.Length; i++)
            {
                Assert.AreEqual(intPoints[i].X, (int)realPoints[i].X);
                Assert.AreEqual(intPoints[i].Y, (int)realPoints[i].Y);
                Assert.AreEqual(((IntPoint)intPoints[i]).Label, ((RealPoint)realPoints[i]).Label);
            }
        }

        [Test]
        public void TestTestMappingWithSubtyping()
        {
            var trans = MappingPoco<Figure, Shape>.New().UsingCustomInstantiation(f =>
            {
                switch (f.Type)
                {
                    case FigureType.Circle:
                        return new Circle();
                    case FigureType.Ellipse:
                        return new Ellipse2();
                    default:
                        throw new ArgumentException();
                }
            });

            trans.Property("Label").FromProperty<string[], string>("FullName", ss =>
            {
                return string.Join(".", ss);
            });

            trans.ForType<Circle>().Property("X").FromProperty<Point, int>("Location", p => p.X);
            trans.ForType<Circle>().Property("Y").FromProperty<Point, int>("Location", p => p.Y);
            trans.ForType<Circle>().Property("Radius").FromProperty<Size, int>("Size", s => s.Width);

            trans.ForType<Ellipse2>().Property("Location").FromProperty("Location");
            trans.ForType<Ellipse2>().Property("Semiaxises").FromProperty("Size");

            var figure1 = new Figure
            {
                FullName = new string[] { "ns1", "ns2", "name1" },
                Size = new Size(3, 4),
                Location = new Point(1, 2),
                Type = FigureType.Circle
            };

            var figure2 = new Figure
            {
                FullName = new string[] { "n3", "n4", "name2" },
                Size = new Size(3, 4),
                Location = new Point(2, 1),
                Type = FigureType.Ellipse
            };

            var circle = trans.Map(figure1);
            Assert.AreEqual(typeof(Circle), circle.GetType());
            Assert.AreEqual("ns1.ns2.name1", (circle as Circle).Label);
            Assert.AreEqual(figure1.Size.Width, (circle as Circle).Radius);
            Assert.AreEqual(figure1.Location.X, (circle as Circle).X);
            Assert.AreEqual(figure1.Location.Y, (circle as Circle).Y);

            var ellipse = trans.Map(figure2);
            Assert.AreEqual(typeof(Ellipse2), ellipse.GetType());
            Assert.AreEqual("n3.n4.name2", (ellipse as Ellipse2).Label);
            Assert.AreEqual(figure2.Size, (ellipse as Ellipse2).Semiaxises);
            Assert.AreEqual(figure2.Location, (ellipse as Ellipse2).Location);
        }


        [Test]
        public void TestMappingWithSubtyping()
        {
            var mapping = new Mapping<Figure, Shape>();
            mapping.UsingCustomInstantiation(f =>
                                                 {
                                                     switch (f.Type)
                                                     {
                                                         case FigureType.Circle:
                                                             return new Circle();
                                                         case FigureType.Ellipse:
                                                             return new Ellipse2();
                                                         default:
                                                             throw new ArgumentException();
                                                     }
                                                 });
            mapping.TargetProperties["Label"].FromProperty<string[], string>("FullName", ss =>
                                                                                             {
                                                                                                 var sb = new StringBuilder();
                                                                                                 var counter = 0;
                                                                                                 foreach ( var s in ss )
                                                                                                 {
                                                                                                     if ( counter > 0 )
                                                                                                         sb.Append(".");
                                                                                                     sb.Append(s);
                                                                                                     counter++;
                                                                                                 }
                                                                                                 return sb.ToString();
                                                                                             });

            mapping.ForType<Circle>()["X"].FromProperty<Point, int>("Location", p => p.X);
            mapping.ForType<Circle>()["Y"].FromProperty<Point, int>("Location", p => p.Y);
            mapping.ForType<Circle>()["Radius"].FromProperty<Size, int>("Size", s => s.Width);

            mapping.ForType<Ellipse2>()["Location"].FromProperty<Point, Point>("Location", Mapping<Point>.IdentityMapping);
            mapping.ForType<Ellipse2>()["Semiaxises"].FromProperty<Size, Size>("Size", Mapping<Size>.IdentityMapping);

            var figure1 = new Figure
                              {
                                  FullName = new string[] {"ns1", "ns2", "name1"},
                                  Size = new Size(3, 4),
                                  Location = new Point(1, 2),
                                  Type = FigureType.Circle
                              };

            var figure2 = new Figure
            {
                FullName = new string[] { "n3", "n4", "name2" },
                Size = new Size(3, 4),
                Location = new Point(2, 1),
                Type = FigureType.Ellipse
            };

            var circle = mapping.Map(figure1);
            Assert.AreEqual(typeof(Circle), circle.GetType());
            Assert.AreEqual("ns1.ns2.name1", (circle as Circle).Label);
            Assert.AreEqual(figure1.Size.Width, (circle as Circle).Radius);
            Assert.AreEqual(figure1.Location.X, (circle as Circle).X);
            Assert.AreEqual(figure1.Location.Y, (circle as Circle).Y);

            var ellipse = mapping.Map(figure2);
            Assert.AreEqual(typeof(Ellipse2), ellipse.GetType());
            Assert.AreEqual("n3.n4.name2", (ellipse as Ellipse2).Label);
            Assert.AreEqual(figure2.Size, (ellipse as Ellipse2).Semiaxises);
            Assert.AreEqual(figure2.Location, (ellipse as Ellipse2).Location);
            
        }

        [Test]
        public void TestDelegateConversion()
        {
            Fun<int, int> f = i => i;
            Fun<int, object> target = o => (object)f(o);
            Assert.AreEqual(1, target(1));
        }

        [Test]
        public void TestPropertyGetterType()
        {
            var ellipse = new Ellipse
                              {
                                  Label = "123"
                              };
            var point = new IntPoint(122, 34, "567");
            var ellipseProperty = typeof (Ellipse).GetProperty("Label");
            var pointXProperty = typeof (IntPoint).GetProperty("X");
            var pointLabelProperty = typeof (IntPoint).GetProperty("Label");
            var classPropertyGetter = PropertyDescription.GetComputablePropertyGetter(ellipseProperty);
            var structValueTypePropertyGetter = PropertyDescription.GetComputablePropertyGetter(pointXProperty);
            var structReferenceTypePropertyGetter = PropertyDescription.GetComputablePropertyGetter(pointLabelProperty);

            Assert.AreEqual(ellipse.Label, classPropertyGetter(ellipse));
            Assert.AreEqual(point.X, structValueTypePropertyGetter(point));
            Assert.AreEqual(point.Label, structReferenceTypePropertyGetter(point));
        }

        public Func<object ,object > CreateGetter(PropertyInfo propertyInfo)
        {
            // The third version
            var param = Expression.Parameter(typeof(object), "target");
            var func = Expression.Lambda(
                Expression.Convert(
                    Expression.Property(
                        Expression.Convert(param, propertyInfo.ReflectedType),
                        propertyInfo),
                    typeof(object)),
                param
                );
            return (Func<object, object>)func.Compile();

        }

        [Test]
        public void TestTransformationsWithInterfaces()
        {
            var intPoints = new[]
            {
                new IntPoint(1,2,"1"),
                new IntPoint(2,2,"2"),
                new IntPoint(0,0,"3"),
                new IntPoint(2,1,"4"),
                new IntPoint(3,3,"5"),
            }
                .Cast<IPoint<int>>().ToArray();
            var typeIPointInt = ObjectDescription.Get<IPoint<int>>();
            var typeIPointFloat = ObjectDescription.Get<IPoint<float>>();

            var mapping = new Mapping<IPoint<int>, IPoint<float>>();
            mapping.UsingMap(p => p is IntPoint ip ? new RealPoint(ip.X, ip.Y, ip.Label) : throw new ArgumentException(""));

            var realPoints = mapping.Convert(intPoints);

            Assert.AreEqual(intPoints.Length, realPoints.Count);
            for (var i = 0; i < intPoints.Length; i++)
            {
                Assert.AreEqual(intPoints[i].X, (int)realPoints[i].X);
                Assert.AreEqual(intPoints[i].Y, (int)realPoints[i].Y);
                Assert.AreEqual(((IntPoint)intPoints[i]).Label, ((RealPoint)realPoints[i]).Label);
            }
        }

        public void TestConvertion()
        {
            var v = new Union<int, string>(1);
            var i = (string)v;
        }

        private class Union<T1,T2>
        {
            public Union(T1 value)
            {
                
            }

            public Union(T2 value)
            {
                
            }

            public static implicit operator string(Union<T1, T2> u)
            {
                return "2";
            }

            public static implicit operator Union<T2, T1>(Union<T1, T2> u)
            {
                return null;//new Union<T2, T1>(u.);
            }

        }
    }

    //public class PersistentTransformation<TSource, TTarget> : Mapping<TSource, TTarget>
    //{
    //    public Mapping<TSource> DependsOn(params string[] properties)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

}
