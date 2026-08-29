using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mephi.Cybernetics.Race.Core.Tests.ReflectionTests
{
    public class C1
    {
        public static int Get(int x)
        {
            return x;
        }

        public static int SomeValue { get; set; }

    }
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public interface IC2
    {
        private static int _someValue2;

        static int SomeValue { get; set; }
        static int SomeValue2 
        { 
            get => _someValue2; 
            set => _someValue2 = value; 
        }

#if NET7_0_OR_GREATER
        static abstract int SomeValue3 { get; set; }
        static abstract IC2 Copy(IC2 other);
#endif
    }
#endif

        [TestFixture]
    public class ReflectionTests
    {
        [Test]
        public void TestStaticMethodInClass()
        {
            var method = typeof(C1).GetMethod(nameof(C1.Get), BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(int), method.ReturnType);

            Assert.AreEqual(1, method.GetParameters().Length); 
            Assert.AreEqual(typeof(int), method.GetParameters()[0].ParameterType);


            Func<int, int> methodDelegate = null;
            Assert.DoesNotThrow(() => { methodDelegate = Delegate.CreateDelegate(typeof(Func<int, int>), method) as Func<int, int>; });
            Assert.IsNotNull(methodDelegate);
        }

        [Test]
        public void TestStaticPropertyInClass()
        {
            var property = typeof(C1).GetProperty(nameof(C1.SomeValue), BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(property);
            Assert.AreEqual(typeof (int), property.PropertyType);

            Assert.IsNotNull(property.GetGetMethod());
            Assert.AreEqual(0, property.GetGetMethod().GetParameters().Length);

            Func<int> getMethodDelegate = null;
            Assert.DoesNotThrow(() => { getMethodDelegate = Delegate.CreateDelegate(typeof(Func<int>), property.GetGetMethod()) as Func<int>; });
            Assert.IsNotNull(getMethodDelegate);

            Func<C1,int> getMethodDelegate2 = null;
            Assert.Throws<ArgumentException>(() => { getMethodDelegate2 = Delegate.CreateDelegate(typeof(Func<C1,int>), property.GetGetMethod()) as Func<C1,int>; });
            Assert.IsNull(getMethodDelegate2);
            //Assert.DoesNotThrow(() => getMethodDelegate2(null));

            Assert.IsNotNull(property.GetSetMethod());
            Assert.AreEqual(1, property.GetSetMethod().GetParameters().Length);
            Assert.AreEqual(typeof(int), property.GetSetMethod().GetParameters()[0].ParameterType);

            Action<int> setMethodDelegate = null;
            Assert.DoesNotThrow(() => { setMethodDelegate = Delegate.CreateDelegate(typeof(Action<int>), property.GetSetMethod()) as Action<int>; });
            Assert.IsNotNull(setMethodDelegate);
        }

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [Test]
        public void TestStaticMethodInInterface()
        {
            var staticProperty = typeof(IC2).GetProperty(nameof(IC2.SomeValue), BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(staticProperty);
            Assert.AreEqual(typeof(int), staticProperty.PropertyType);

            Assert.IsNotNull(staticProperty.GetGetMethod());
            Assert.AreEqual(0, staticProperty.GetGetMethod().GetParameters().Length);
            Assert.IsFalse(staticProperty.GetSetMethod().IsAbstract);

            Func<int> getMethodDelegate = null;
            Assert.DoesNotThrow(() => { getMethodDelegate = Delegate.CreateDelegate(typeof(Func<int>), staticProperty.GetGetMethod()) as Func<int>; });
            Assert.IsNotNull(getMethodDelegate);

            Assert.IsNotNull(staticProperty.GetSetMethod());
            Assert.AreEqual(1, staticProperty.GetSetMethod().GetParameters().Length);
            Assert.AreEqual(typeof(int), staticProperty.GetSetMethod().GetParameters()[0].ParameterType);
            Assert.IsFalse(staticProperty.GetSetMethod().IsAbstract);

            Action<int> setMethodDelegate = null;
            Assert.DoesNotThrow(() => { setMethodDelegate = Delegate.CreateDelegate(typeof(Action<int>), staticProperty.GetSetMethod()) as Action<int>; });
            Assert.IsNotNull(setMethodDelegate);
        }
#endif

#if NET7_0_OR_GREATER
        [Test]
        public void TestStaticAbstractMethodInInterface()
        {
            var method = typeof(IC2).GetMethod(nameof(IC2.Copy), BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(IC2), method.ReturnType);
            Assert.IsTrue(method.IsAbstract);

            Assert.AreEqual(1, method.GetParameters().Length);
            Assert.AreEqual(typeof(IC2), method.GetParameters()[0].ParameterType);

            // не совсем понятно, но почему-то это не работает -- ошибка компилятора
            // "The interface cannot be used as type argument. Static member does not have a most specific implementation in the interface"
            //Func<IC2, IC2> methodDelegate = null;
            //Assert.DoesNotThrow(() => { methodDelegate = Delegate.CreateDelegate(typeof(Func<IC2, IC2>), method) as Func<IC2, IC2>; });
            //Assert.IsNotNull(methodDelegate);
        }

        [Test]
        public void TestStaticAbstractPropertyInInterface() 
        {
            // случай, когда область значений свойства == int
            var property = typeof(IC2).GetProperty(nameof(IC2.SomeValue3), BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(int), property.PropertyType);

            Assert.IsNotNull(property.GetGetMethod());
            Assert.IsTrue(property.GetGetMethod().IsAbstract);
            Assert.AreEqual(0, property.GetGetMethod().GetParameters().Length);

            Func<int> getMethodDelegate = null;
            // это почему-то работает !!?!?!?!?!??? метод-то абстрактный...
            Assert.DoesNotThrow(() => { getMethodDelegate = Delegate.CreateDelegate(typeof(Func<int>), property.GetGetMethod()) as Func<int>; });
            Assert.IsNotNull(getMethodDelegate);
            // ну, хотя бы исключение адекватное
            Assert.Throws<EntryPointNotFoundException>(() => getMethodDelegate());

            Assert.IsNotNull(property.GetSetMethod());
            Assert.AreEqual(1, property.GetSetMethod().GetParameters().Length);
            Assert.IsTrue(property.GetSetMethod().IsAbstract);
            Assert.AreEqual(typeof(int), property.GetSetMethod().GetParameters()[0].ParameterType);

            Action<int> setMethodDelegate = null;
            // это почему-то работает !!?!?!?!?!???
            Assert.DoesNotThrow(() => { setMethodDelegate = Delegate.CreateDelegate(typeof(Action<int>), property.GetSetMethod()) as Action<int>; });
            Assert.IsNotNull(setMethodDelegate);
            // капец, а тут NullReferenceException....
            Assert.Throws<NullReferenceException>(() => setMethodDelegate(0));
        }
#endif
    }
}
