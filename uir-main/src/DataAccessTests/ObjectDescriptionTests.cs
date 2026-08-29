using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures.ComplexObjects;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests
{
    [TestFixture]
    public class ObjectDescriptionTests
    {
        [Test]
        public void TestUsingSystemTypeAsDictionaryKey()
        {
            var dictionary = new Dictionary<Type, object>();

            var intType = typeof (int);
            var typeofIntConst = 1.GetType();

            Assert.AreEqual(intType, typeofIntConst);
            Assert.AreEqual(intType.GetHashCode(), typeofIntConst.GetHashCode());

            dictionary.Add(typeofIntConst, 1);
            Assert.IsTrue(dictionary.ContainsKey(intType));

            var arrayType = typeof (Tuple<int>[]);
            var arrayType2 = typeof(Tuple<int>[]);
            Assert.AreEqual(arrayType, arrayType2);
            Assert.AreEqual(arrayType.GetHashCode(), arrayType2.GetHashCode());

            dictionary.Add(arrayType, null);
            Assert.IsTrue(dictionary.ContainsKey(arrayType2));

            var list = new List<Type> {arrayType};
            Assert.Contains(arrayType2, list);

            var indirectIntType = typeof (EntityBase).BaseType.GetMethod("GetHashCode").ReturnType;
            Assert.AreEqual(intType, indirectIntType);
            Assert.AreEqual(intType.GetHashCode(), indirectIntType.GetHashCode());

        }


        /// <summary>
        /// Тест проверяет корректность информации о некоторых простых встроенных типах
        /// </summary>
        [Test]
        public void TestBuiltInSimpleDataTypes()
        {
            var primitiveTypes = new List<Type>(new Type[]
                                                {
                                                    typeof(ValueType),
                                                    //typeof(Nullable),
                                                    typeof (Int16),
                                                    typeof (Int32),
                                                    typeof (Int64),
                                                    typeof (UInt16),
                                                    typeof (UInt32),
                                                    typeof (UInt64),
                                                    typeof (string),
                                                    typeof (DateTime),
                                                    typeof (float),
                                                    typeof (double),
                                                    typeof (bool),
                                                    typeof (Delegate),
                                                    typeof (Char),
                                                    typeof (Enum)
                                                });

            foreach ( var primitiveType in primitiveTypes )
            {
                var od = ObjectDescription.Get(primitiveType);
                Assert.AreEqual(typeof(ObjectDescription), od.GetType());
                Assert.IsTrue(od.IsAssumedPrimitive);
                if ( primitiveType == typeof(object) )
                {
                    Assert.AreEqual(0, od.Parents.Length);
                }

                Assert.IsFalse(od.IsComplex);
                Assert.IsTrue(od.IsAtomic);

                Assert.IsTrue(od.IsSubtypeOf(ObjectDescription.Get(typeof(object))));
                //Assert.AreEqual(!(primitiveType != typeof(Enum) && (primitiveType != typeof(ValueType))) || primitiveType.IsValueType, od.IsSubtypeOf(typeof(ValueType)));

                Assert.AreEqual(0, od.Arity);
                //Assert.AreNotEqual(primitiveType.IsValueType, od.IsSubtypeOf(typeof(Nullable)));

                //var domain = od as 
                //Assert.
            }
        }

        /// <summary>
        /// Проверяется корректность информации о конструкторах-TFF
        /// </summary>
        [Test]
        public void TestComplexObjectDescription()
        {
            var bookConstructor = ComplexObjectDescription.GetConstructorDescription<BookConstructor>();
            var complexUnit = ComplexObjectDescription.GetConstructorDescription<ComplexUnit>();

            Assert.AreEqual(0, bookConstructor.Arity);
            Assert.AreEqual(0, complexUnit.Arity);

            Assert.IsNotNull(bookConstructor);
            Assert.IsNotNull(complexUnit);

            Assert.IsTrue(bookConstructor.IsComplex);
            Assert.IsTrue(complexUnit.IsComplex);
            Assert.IsFalse(bookConstructor.IsAtomic);
            Assert.IsFalse(complexUnit.IsAtomic);

            Assert.AreEqual(3, bookConstructor.ConstructorArity);
            Assert.AreEqual(4, bookConstructor.Properties.Count);

            Assert.AreEqual(1, complexUnit.ConstructorArity);
            Assert.AreEqual(3, complexUnit.Properties.Count);


            var author1 = new Author {Name = "1", Year = 1};
            var prefaceUnit = new Unit {Name = "preface", Text = "preface text"};
            var unit1 = new Unit {Name = "1", Text = "some text"};
            var unit2 = new Unit {Name = "2", Text = "Some other text"};
            var unit3 = new Unit {Name = "3", Text = "Yet another text"};
            var unit4 = new ComplexUnit
            {
                Name = "4",
                Text = "preface",
                SubUnits = new List<Unit>(new Unit[] {unit2, unit3})
            };

            var subunits = complexUnit.GetSubObjects(unit4);
            Assert.AreEqual(1, subunits.Length);
            Assert.IsTrue(ObjectDescription.Get(subunits[0]).IsSubtypeOf(ObjectDescription.Get<IList<Unit>>()));
            for (var i = 0; i < 2; i++)
                Assert.AreEqual(unit4.SubUnits[i], (subunits[0] as IList<Unit>)[i]);
            for (var i = 0; i < 2; i++)
                Assert.AreEqual(subunits[0], complexUnit.SubObjects[0].GetSubObject(unit4));

            var book = new BookConstructor
            {
                Name = "Book1",
                Preface = prefaceUnit,
                Content = new Unit[] { unit1, unit4 },
                Authors = new List<Author>(new Author[] { author1 })
            };


            var subobjects = bookConstructor.GetSubObjects(book);
            Assert.AreEqual(3, subobjects.Length);
            for (var i = 0; i < 3; i++)
                Assert.AreEqual(subobjects[i], bookConstructor.SubObjects[i].GetSubObject(book));
            Assert.IsTrue(ObjectDescription.Get(subobjects[bookConstructor.SubObjects["Content"].Index]).IsSubtypeOf(ObjectDescription.Get<IList<Unit>>()));
            Assert.IsTrue(ObjectDescription.Get(subobjects[bookConstructor.SubObjects["Authors"].Index]).IsSubtypeOf(ObjectDescription.Get<IList<Author>>()));
            Assert.IsTrue(ObjectDescription.Get(subobjects[bookConstructor.SubObjects["Preface"].Index]).IsSubtypeOf(ObjectDescription.Get<Unit>()));

            Assert.AreEqual(book.Preface, bookConstructor.SubObjects["Preface"].GetSubObject(book));
            Assert.AreEqual(book.Content, bookConstructor.SubObjects["Content"].GetSubObject(book));
            Assert.AreEqual(book.Authors, bookConstructor.SubObjects["Authors"].GetSubObject(book));
        }

        [Test]
        public void TestExplicitInterfacePropertiesImplementation()
        {
            var list = new List<int>(new int[] { 1, 2, 3 });
            var listOd = ObjectDescription.Get<List<int>>();
            var ilistOd = ObjectDescription.Get<IList<int>>();
            Assert.AreEqual(list.Count, listOd.Properties["Count"].GetValue(list));
            //Assert.AreEqual((list as IList<int>).IsReadOnly, listOd.Properties["IsReadOnly"].GetValue(list));
            Assert.AreEqual((list as IList<int>).IsReadOnly, ilistOd.Properties["IsReadOnly"].GetValue(list));

            var doer1Od = ObjectDescription.Get<Doer>();
            var doer2Od = ObjectDescription.Get<Doer2>();
            var featureOd = ObjectDescription.Get<IFeature>();

            var doer21 = new Doer2(true, false);
            var doer22 = new Doer2(false, true);

            Assert.AreEqual(true, doer21.Available);
            Assert.AreEqual(true, (doer21 as IFeature).Available);
            //Assert.Throws<KeyNotFoundException>(() => doer1Od.Properties["Available"].GetValue<bool>(doer21));
            Assert.AreEqual(true, featureOd.Properties["Available"].GetValue<bool>(doer21));
            Assert.AreEqual(true, doer2Od.Properties["Available"].GetValue<bool>(doer21));
            Assert.AreEqual(true, doer1Od.Properties["Runnable"].GetValue<bool>(doer21));
            Assert.AreEqual(true, doer2Od.Properties["Runnable"].GetValue<bool>(doer21));
            Assert.AreEqual(false, doer22.Available);
            Assert.AreEqual(false, (doer22 as IFeature).Available);
            //Assert.Throws<KeyNotFoundException>(() => doer1Od.Properties["Available"].GetValue<bool>(doer22));
            Assert.AreEqual(false, featureOd.Properties["Available"].GetValue<bool>(doer22));
            Assert.AreEqual(false, doer2Od.Properties["Available"].GetValue<bool>(doer22));
            Assert.AreEqual(true, doer1Od.Properties["Runnable"].GetValue<bool>(doer22));
            Assert.AreEqual(true, doer2Od.Properties["Runnable"].GetValue<bool>(doer22));

            var doer11 = new Doer(true, false);
            var doer12 = new Doer(false, true);
            Assert.AreEqual(false, doer1Od.Properties["Runnable"].GetValue<bool>(doer11));
            Assert.AreEqual(true, doer1Od.Properties["Runnable"].GetValue<bool>(doer12));

            var doer3Od = ObjectDescription.Get<Doer3>();
            var additionalFeatureOd = ObjectDescription.Get<IAdditionalFeature>();
            var doer31 = new Doer3(true, false);
            var doer32 = new Doer3(false, true);
            Assert.AreEqual(true, doer3Od.Properties["Runnable"].GetValue<bool>(doer31));
            Assert.AreEqual(true, doer2Od.Properties["Runnable"].GetValue<bool>(doer31));
            Assert.AreEqual(true, doer1Od.Properties["Runnable"].GetValue<bool>(doer31));
            Assert.AreEqual(true, doer3Od.Properties["Runnable"].GetValue<bool>(doer32));
            Assert.AreEqual(true, doer2Od.Properties["Runnable"].GetValue<bool>(doer32));
            Assert.AreEqual(true, doer1Od.Properties["Runnable"].GetValue<bool>(doer32));
            Assert.AreEqual(true, additionalFeatureOd.Properties["Runnable"].GetValue<bool>(doer31));
            Assert.AreEqual(true, additionalFeatureOd.Properties["Runnable"].GetValue<bool>(doer32));
        }

        [Test]
        public void TestArrayInterfaceImplementations()
        {
            var intArray = ObjectDescription.Get<int[]>();
            var intIList = ObjectDescription.Get<IList<int>>();
            Assert.IsTrue(intArray.IsSubtypeOf(intIList));

            var list = new int[] {1, 2, 3};
            Assert.AreEqual(intArray.Properties["Length"].GetValue<int>(list), intIList.Properties["Count"].GetValue<int>(list));
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Проверяется корректность проверки типа на "вхождение" в другой тип (подтипизация)
        /// </summary>
        [Test]
        public void TestSubtyping()
        {
            var intObjectDescription = ObjectDescription.Get(typeof(int));
            var valueTypeObjectDescription = ObjectDescription.Get(typeof(ValueType));
            var objectObjectDescription = ObjectDescription.Get(typeof(object));

            Assert.IsTrue(intObjectDescription.IsSubtypeOf(intObjectDescription));
            Assert.IsTrue(intObjectDescription.IsSubtypeOf(typeof(int)));
            Assert.IsTrue(intObjectDescription.IsSubtypeOf(valueTypeObjectDescription));
            Assert.IsTrue(intObjectDescription.IsSubtypeOf(typeof(ValueType)));
            Assert.IsTrue(intObjectDescription.IsSubtypeOf(objectObjectDescription));
            Assert.IsTrue(intObjectDescription.IsSubtypeOf(typeof(object)));
        }

        /// <summary>
        /// Проверяется верность списка типов-родителей
        /// </summary>
        [Test]
        public void TestParentsList()
        {
            var objectOd = ObjectDescription.Get(typeof(object));
            var shapeOd = ObjectDescription.Get(typeof(Shape));
            var locatableOd = ObjectDescription.Get(typeof(ILocatable));
            var movableOd = ObjectDescription.Get(typeof(IMovable));
            var ellipse2Od = ObjectDescription.Get(typeof(Ellipse2));
            var hollowEllipseOd = ObjectDescription.Get(typeof(HollowEllipse));
            var drawable = ObjectDescription.Get(typeof(IDrawable));
            var figure = ObjectDescription.Get(typeof(IFigure));

            Assert.AreEqual(2, drawable.Parents.Length);
            Assert.AreEqual(2, drawable.Parents.Intersect(new[] { locatableOd, movableOd }).Count());

            Assert.AreEqual(1, figure.Parents.Length);
            Assert.AreEqual(1, figure.Parents.Intersect(new[] { drawable }).Count());

            Assert.AreEqual(1, shapeOd.Parents.Length);
            Assert.AreEqual(objectOd, shapeOd.Parents[0]);

            Assert.AreEqual(0, locatableOd.Parents.Length);
            Assert.AreEqual(0, movableOd.Parents.Length);

            Assert.AreEqual(3, ellipse2Od.Parents.Length);
            Assert.AreEqual(3, ellipse2Od.Parents.Intersect(new[] {shapeOd, locatableOd, movableOd}).Count());

            Assert.AreEqual(1, hollowEllipseOd.Parents.Length);
            Assert.AreEqual(1, hollowEllipseOd.Parents.Intersect(new[] { ellipse2Od }).Count());
        }

        /// <summary>
        /// Проверяет верность списка свойств
        /// </summary>
        [Test]
        public void TestPropertyList()
        {
            var locatableOd = ObjectDescription.Get(typeof(ILocatable));
            var movableOd = ObjectDescription.Get(typeof(IMovable));
            var drawable = ObjectDescription.Get(typeof(IDrawable));
            var ellipse2Od = ObjectDescription.Get(typeof(Ellipse2));

            Assert.AreEqual(1, locatableOd.Properties.Count);
            Assert.AreEqual(1, movableOd.Properties.Count);
            Assert.AreEqual(2, drawable.Properties.Count);
            Assert.AreEqual(3, ellipse2Od.Properties.Count);
        }

        [Test]
        public void TestStaticProperties()
        {
            var circleOd = ObjectDescription.Get(typeof(Circle));
            
            Assert.AreEqual(5, circleOd.Properties.Count);
            Assert.IsFalse(circleOd.Properties.Contains(nameof(Circle.DefaultCircle)));
            Assert.AreEqual(1, circleOd.StaticProperties.Count);
            Assert.AreEqual(0, circleOd.StaticProperties.ComputableProperties.Count);
            Assert.AreEqual(1, circleOd.StaticProperties.StoredProperies.Count);
            Assert.IsTrue(circleOd.StaticProperties.Contains(nameof(Circle.DefaultCircle)));
        }

        /// <summary>
        /// Проверяется верность чтения и установки значения свойств через PropertyDescription (без учета вопросов наследования и подтипизации)
        /// </summary>
        [Test]
        public void TestPropertyGetterAndSetter()
        {
            var movableHardcoreOd = ObjectDescription.Get(typeof(IMovableHardcore));
            Assert.AreEqual(0, movableHardcoreOd.Properties.Count);

            var circleOd = ObjectDescription.Get(typeof(Circle2));
            var theCircle = new Circle2(Color.Blue) {Label = "123", X = 1, Y = 1, Radius = 1};

            Assert.IsFalse(circleOd.Properties["Label"].IsReadOnly);
            Assert.IsFalse(circleOd.Properties["X"].IsReadOnly);
            Assert.IsFalse(circleOd.Properties["Y"].IsReadOnly);
            Assert.IsFalse(circleOd.Properties["Radius"].IsReadOnly);
            Assert.IsTrue(circleOd.Properties["Space"].IsReadOnly);
            Assert.IsTrue(circleOd.Properties["Color"].IsReadOnly);

            Assert.IsFalse(circleOd.Properties["Label"].IsComputable);
            Assert.IsFalse(circleOd.Properties["X"].IsComputable);
            Assert.IsFalse(circleOd.Properties["Y"].IsComputable);
            Assert.IsFalse(circleOd.Properties["Radius"].IsComputable);
            Assert.IsFalse(circleOd.Properties["Color"].IsComputable);
            Assert.IsTrue(circleOd.Properties["Space"].IsComputable);

            Assert.AreEqual(theCircle.X, circleOd.Properties["X"].GetValue<int>(theCircle));
            Assert.AreEqual(theCircle.Y, circleOd.Properties["Y"].GetValue<int>(theCircle));
            Assert.AreEqual(theCircle.Label, circleOd.Properties["Label"].GetValue<string>(theCircle));
            Assert.AreEqual(theCircle.Radius, circleOd.Properties["Radius"].GetValue<int>(theCircle));
            Assert.AreEqual(theCircle.Space, circleOd.Properties["Space"].GetValue<double>(theCircle));

            circleOd.Properties["Label"].SetValue(theCircle, "000");
            Assert.AreEqual(theCircle.Label, circleOd.Properties["Label"].GetValue<string>(theCircle));
            Assert.AreEqual("000", theCircle.Label);
            Assert.AreEqual("000", circleOd.Properties["Label"].GetValue<string>(theCircle));

            circleOd.Properties["Radius"].SetValue(theCircle, 2);
            Assert.AreEqual(theCircle.Radius, circleOd.Properties["Radius"].GetValue<int>(theCircle));
            Assert.AreEqual(2, theCircle.Radius);
            Assert.AreEqual(2, circleOd.Properties["Radius"].GetValue<int>(theCircle));

            bool cantSetProperty;
            try
            {
                circleOd.Properties["Space"].SetValue(theCircle, 4);
                cantSetProperty = false;
            }
            catch (Exception e)
            {
                cantSetProperty = true;
            }
            Assert.IsTrue(cantSetProperty);

            Assert.IsTrue(circleOd.StaticProperties.Contains(nameof(Circle2.DefaultCircle2)));
            var defaultCircleProperty = circleOd.StaticProperties[nameof(Circle2.DefaultCircle2)];
            Assert.IsTrue(defaultCircleProperty.IsReadOnly);
            Assert.IsFalse(defaultCircleProperty.IsComputable);
            Assert.IsFalse(defaultCircleProperty.IsOverloadable);
            var defaultCircle = defaultCircleProperty.GetValue<Circle>(null);
            Assert.IsNotNull(defaultCircle);
            Assert.AreEqual(0, circleOd.Properties[nameof(Circle2.X)].GetValue(defaultCircle));

            //TODO: добавить тесты на работу сеттера для статического свойства
        }

#if NET7_0_OR_GREATER
        [Test]
        public void TestIBinaryNumberInterfaceObjectDescription()
        {
            Assert.DoesNotThrow(() => ObjectDescription.Get(typeof(INumber<byte>)));
            Assert.DoesNotThrow(() => ObjectDescription.Get(typeof(IBinaryNumber<byte>)));
            Assert.DoesNotThrow(() => ObjectDescription.Get(typeof(byte)));
        }
#endif

        /// <summary>
        /// Проверяется верность чтения и записи значений свойств через ObjectDescription класса-родителя
        [Test]
        /// </summary>
        public void TestPropertyViaAncestorClass()
        {
            var shapeOd = ObjectDescription.Get(typeof(Shape));
            var circleOd = ObjectDescription.Get(typeof(Circle));
            var theCircle = new Circle2(Color.Blue) { Label = "123", X = 1, Y = 1, Radius = 1 };

            Assert.AreEqual(theCircle.Label, shapeOd.Properties["Label"].GetValue<string>(theCircle));

            Assert.AreEqual(theCircle.X, circleOd.Properties["X"].GetValue<int>(theCircle));
            Assert.AreEqual(theCircle.Y, circleOd.Properties["Y"].GetValue<int>(theCircle));
            Assert.AreEqual(theCircle.Label, circleOd.Properties["Label"].GetValue<string>(theCircle));
            Assert.AreEqual(theCircle.Radius, circleOd.Properties["Radius"].GetValue<int>(theCircle));

            circleOd.Properties["Radius"].SetValue(theCircle, 2);
            Assert.AreEqual(theCircle.Radius, circleOd.Properties["Radius"].GetValue<int>(theCircle));
            Assert.AreEqual(2, theCircle.Radius);
            Assert.AreEqual(2, circleOd.Properties["Radius"].GetValue<int>(theCircle));

            shapeOd.Properties["Label"].SetValue(theCircle, "000");
            Assert.AreEqual(theCircle.Label, shapeOd.Properties["Label"].GetValue<string>(theCircle));
            Assert.AreEqual(theCircle.Label, circleOd.Properties["Label"].GetValue<string>(theCircle));
            Assert.AreEqual("000", theCircle.Label);
            Assert.AreEqual("000", circleOd.Properties["Label"].GetValue<string>(theCircle));
            Assert.AreEqual("000", shapeOd.Properties["Label"].GetValue<string>(theCircle));
        }

        /// <summary>
        /// Проверяется верность чтения и записи значений свойств через ObjectDescription родительского интерфейса
        /// </summary>
        [Test]
        public void TestPropertyViaInterface()
        {
            var shapeOd = ObjectDescription.Get(typeof(Shape));
            var locatableOd = ObjectDescription.Get(typeof(ILocatable));
            var movableOd = ObjectDescription.Get(typeof(IMovable));
            var drawable = ObjectDescription.Get(typeof(IDrawable));
            var ellipse3Od = ObjectDescription.Get(typeof(Ellipse3));
            var figure = ObjectDescription.Get(typeof(IFigure));

            var theEllipse = new Ellipse3()
                                 {
                                     Label = "123",
                                     Location = new Point(1, 2),
                                     Semiaxises = new Size(3, 4),
                                     IsDrawn = false
                                 };

            Assert.AreEqual(1, movableOd.Properties.Count);

            Assert.AreEqual(theEllipse.Location, locatableOd.Properties["Location"].GetValue<Point>(theEllipse));
            Assert.AreEqual(theEllipse.Location, movableOd.Properties["Location"].GetValue<Point>(theEllipse));
            Assert.AreEqual(theEllipse.Location, ellipse3Od.Properties["Location"].GetValue<Point>(theEllipse));
            Assert.AreEqual(theEllipse.Location, drawable.Properties["Location"].GetValue<Point>(theEllipse));
            Assert.AreEqual(theEllipse.Location, figure.Properties["Location"].GetValue<Point>(theEllipse));

            Assert.AreEqual(theEllipse.IsDrawn, ellipse3Od.Properties["IsDrawn"].GetValue<bool>(theEllipse));
            Assert.AreEqual(theEllipse.IsDrawn, drawable.Properties["IsDrawn"].GetValue<bool>(theEllipse));
            Assert.AreEqual(theEllipse.IsDrawn, figure.Properties["IsDrawn"].GetValue<bool>(theEllipse));

            movableOd.Properties["Location"].SetValue(theEllipse, new Point(5, 6));
            Assert.AreEqual(theEllipse.Location, locatableOd.Properties["Location"].GetValue<Point>(theEllipse));
            Assert.AreEqual(theEllipse.Location, movableOd.Properties["Location"].GetValue<Point>(theEllipse));
            Assert.AreEqual(theEllipse.Location, ellipse3Od.Properties["Location"].GetValue<Point>(theEllipse));
            Assert.AreEqual(theEllipse.Location, drawable.Properties["Location"].GetValue<Point>(theEllipse));
            Assert.AreEqual(theEllipse.Location, figure.Properties["Location"].GetValue<Point>(theEllipse));
        }

        /// <summary>
        /// Проверка правильно вычисления значения свойства по умолчанию
        /// </summary>
        [Test]
        public void TestDefaultPropertyValue()
        {
            var figureDescription = ObjectDescription.Get(typeof(Figure));
            Assert.IsNotNull(figureDescription.Properties["FullName"].PropertyDefaultValue);
            Assert.AreEqual(default(FigureType), figureDescription.Properties["Type"].PropertyDefaultValue);
            Assert.AreEqual(new Size(0, 0), figureDescription.Properties["Size"].PropertyDefaultValue);
            Assert.AreEqual(new Point(0, 0), figureDescription.Properties["Location"].PropertyDefaultValue);

        }

        [Test]
        public void TestFunctionComplexObject()
        {
            var list = new List<int>(new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0});

            var query = list.Where(i => i % 2 == 0);

            var counter = 0;
            var sw = Stopwatch.StartNew();

            foreach ( var element in query )
                counter += element;

            for ( var i = 0; i < 10000; i++ )
            {
                foreach (var element in query)
                    counter += element;
            }

            sw.Stop();
            Console.WriteLine("Running the query 10 000 times: {0} ms", sw.Elapsed.TotalMilliseconds);

            sw = Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++)
            {
                foreach (var element in list)
                    counter += element;
            }
            sw.Stop();
            Console.WriteLine("Running the LIST 10 000 times: {0} ms", sw.Elapsed.TotalMilliseconds);
        }

        [Test, Explicit]
        public void TestGetOdForAllLoadedTypes()
        {
            List<byte> l = null;
            var imageRuntimeVersion = Assembly.GetAssembly(typeof(ObjectDescription)).ImageRuntimeVersion;
            var memoryDelta = Process.GetCurrentProcess().PrivateMemorySize64;
            var sw = Stopwatch.StartNew();
            foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                if (string.Compare(assembly.ImageRuntimeVersion, imageRuntimeVersion) > 0)
                    continue;
                foreach ( var type in assembly.GetTypes() )
                {
                    if ( !type.IsGenericTypeDefinition )
                        ObjectDescription.Get(type);
                }
            }
            sw.Stop();
            memoryDelta = Process.GetCurrentProcess().PrivateMemorySize64 - memoryDelta;
            Console.WriteLine("elapsed: {0}", sw.Elapsed.TotalMilliseconds / 1000);
            Console.WriteLine("memory delta: {0}", memoryDelta);
            Assert.IsTrue(true);
        }

        [Test, Explicit]
        public void TestConvertFunc2PredicatePerformance()
        {
            const int attempts = 1000000;
            Func<object, bool> func = o => true;
            Predicate<object> pred = o => true;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < attempts; i++)
            {
                var predicate0 = func.Method.CreateDelegate(typeof(Func<object, bool>)) as Func<object, bool>;
                var f = pred.Method.CreateDelegate(typeof(Func<object, bool>)) as Func<object,bool>;
                Predicate<object> predicate = func.Method.CreateDelegate(typeof(Predicate<object>)) as Predicate<object>;
                //Predicate<object> predicate = Delegate.CreateDelegate(typeof(Predicate<object>), func.Method) as Predicate<object>;
            }
            sw.Stop();
            var sw1 = Stopwatch.StartNew();
            for (var i = 0; i < attempts; i++)
            {
            }
            sw1.Stop();
            Console.WriteLine($"Empty-for: {sw1.Elapsed.TotalMilliseconds} ms, conversion: {sw.Elapsed.TotalMilliseconds} ms, attempts: {attempts}");
        }
    }

}
