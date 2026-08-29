using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests
{
    [TestFixture]
    public class NullableTests
    {
        [Test]
        public void TestLongIntEquals()
        {
            long l = 1;
            int i = 1;
            Assert.IsTrue(l.Equals(i));
            Assert.IsTrue(l == i);
            //Assert.IsTrue(i.Equals(l));
        }

        [Test]
        public void TestNullableOperatorEquals()
        {
            Nullable<int> i1 = 3;
            Nullable<int> i2 = 3;
            Nullable<int> i3 = 4;

            Assert.IsTrue(i1 == i2);
            Assert.IsFalse(i1 == i3);

            i1 = null;
            i2 = null;
             
            Assert.IsTrue(i1 == i2);
            Assert.IsFalse(i1 == i3);

            Nullable<int> i4 = new Nullable<int>();
            Assert.IsFalse(i4.HasValue);
            Assert.IsFalse(i1.HasValue);
            Assert.IsTrue(i4 == i1);
            Assert.IsFalse(i4 == i3);
        }

        [Test]
        public void TestNullableMethodEquals()
        {
            Nullable<int> i1 = 3;
            Nullable<int> i2 = 3;
            Nullable<int> i3 = 4;

            Assert.IsTrue(i1.Equals(i2));
            Assert.IsFalse(i1.Equals(i3));

            i1 = null;
            i2 = null;

            Assert.IsTrue(i1.Equals(i2));
            Assert.IsFalse(i1.Equals(i3));

            Nullable<int> i4 = new Nullable<int>();
            Assert.IsFalse(i4.HasValue);
            Assert.IsFalse(i1.HasValue);
            Assert.IsTrue(i4.Equals(i1));
            Assert.IsFalse(i4.Equals(i3));
        }

        [Test]
        public void TestTypeInfoEquality()
        {
            var o = new object();
            var o2 = new object();

            var type1 = typeof(object);
            var type2 = typeof(object);
            var type3 = o.GetType();
            var type4 = o.GetType();
            var type5 = o2.GetType();

            Assert.AreEqual(type1, type2);
            Assert.AreEqual(type1, type3);
            Assert.AreEqual(type1, type4);
            Assert.AreEqual(type1, type5);

            Assert.IsTrue(type1.Equals(type2));
            Assert.IsTrue(type1.Equals(type3));
            Assert.IsTrue(type1.Equals(type4));
            Assert.IsTrue(type1.Equals(type5));

            Assert.AreEqual(type1.GetHashCode(), type2.GetHashCode());
            Assert.AreEqual(type1.GetHashCode(), type3.GetHashCode());
            Assert.AreEqual(type1.GetHashCode(), type4.GetHashCode());
            Assert.AreEqual(type1.GetHashCode(), type5.GetHashCode());
        }

        [Test]
        public void TestMemoryStream()
        {
            const int AttemptsCount = 1000000;
            const int elementCount = 10;
            var buffer = new byte[sizeof(int) * elementCount];

            Func<int[]> getRandomInts = () =>
            {
                var elements = new int[elementCount];
                var memoryStream = new MemoryStream(buffer, true);
                var stream = new StreamWriter(memoryStream);
                var rnd = new Random((int)DateTime.Now.TimeOfDay.TotalMilliseconds);
                for ( var i = 0; i < elementCount; i++ )
                    stream.Write(elements[i] = rnd.Next());
                return elements;
            };

            var sampleElements = getRandomInts();

            var sw = Stopwatch.StartNew();
            for ( var i = 0; i < AttemptsCount; i++ )
            {
                var data = new int[buffer.Length / sizeof(int)];
                var counter = 0;
                for ( var j = 1; j < elementCount; j += sizeof(int) )
                {
                    data[counter++] = BitConverter.ToInt32(buffer, j - 1);
                }
            }
            sw.Stop();
            Console.WriteLine("Reading data {0} times: {1} ms", AttemptsCount, sw.Elapsed.TotalMilliseconds);
        }
    }
}
