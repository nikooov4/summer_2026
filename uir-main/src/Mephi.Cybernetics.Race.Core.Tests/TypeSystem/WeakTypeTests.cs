using Mephi.Cybernetics.Race.Core.TypeSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Mephi.Cybernetics.Race.Core.Tests.TypeSystem
{
    [TestFixture]
    public class WeakTypeTests
    {
        [Test]
        public void TestCreateType()
        {
            var type = WeakType
                .Define()
                .Name("Typ1")
                .Namespace("Mephi.Cybernetics.Race.Core.Tests")
                .Source(Assembly.GetExecutingAssembly().FullName)
                .Attributes()
                    .Add("Prop1", ObjectDescription.Get<int>())
                    .Add("Prop2", ObjectDescription.Get<int>())
                    .Add("Prop3", ObjectDescription.Get<int>())
                    .End()
                .End();

            Assert.IsNotNull(type);
            Assert.AreEqual("Typ1", type.Name);
            Assert.AreEqual("Mephi.Cybernetics.Race.Core.Tests", type.Namespace);
            Assert.AreEqual(Assembly.GetExecutingAssembly().FullName, type.Source);
            Assert.AreEqual(3, type.Properties.Count);
            for (int i = 0; i < type.Properties.Count; i++)
                Assert.AreEqual($"Prop{i + 1}", type.Properties[i].Name);
        }

        [Test]
        public void TestCreateWeakInstance()
        {
            var instance = new WeakInstance(new Dictionary<string, object>
            {
                { "Prop1", 1 },
                { "Prop2", 2 },
                { "Prop3", 3 },
                { "Prop4", 4 },
            });
            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.HasAttibute("Prop1"));
            Assert.IsTrue(instance.HasAttibute("Prop2"));
            Assert.IsTrue(instance.HasAttibute("Prop3"));
            Assert.IsTrue(instance.HasAttibute("Prop4"));
            Assert.IsFalse(instance.HasAttibute("Prop"));
            Assert.IsFalse(instance.HasAttibute("Prop5"));
            Assert.AreEqual(1, instance["Prop1"]);
            Assert.AreEqual(2, instance["Prop2"]);
            Assert.AreEqual(3, instance["Prop3"]);
            Assert.AreEqual(4, instance["Prop4"]);
            Assert.AreEqual(Optional<object>.None, instance.TryGet("Prop"));
            Assert.Throws<KeyNotFoundException>(() =>
            {
                var x = instance["Prop"];
            });
        }

        [Test]
        public void TestIsSubtypeOfWeakType()
        {
            var type1 = WeakType
                .Define()
                .Name("Typ1")
                .Namespace("Mephi.Cybernetics.Race.Core.Tests")
                .Source(Assembly.GetExecutingAssembly().FullName)
                .Attributes()
                .Add("Prop1", ObjectDescription.Get<int>())
                .Add("Prop2", ObjectDescription.Get<int>())
                .Add("Prop3", ObjectDescription.Get<int>())
                .End()
                .End();
            var type2 = WeakType
                .Define()
                .Name("Typ2")
                .Namespace("Mephi.Cybernetics.Race.Core.Tests")
                .Source(Assembly.GetExecutingAssembly().FullName)
                .Attributes()
                .Add("Prop1", ObjectDescription.Get<int>())
                .Add("Prop2", ObjectDescription.Get<int>())
                .Add("Prop3", ObjectDescription.Get<int>())
                .Add("Prop4", ObjectDescription.Get<int>())
                .End()
                .End();
            Assert.True(type2.IsSubtypeOf(type1));
        }

        [Test]
        public void TestIsSubtypeOfIDomain()
        {
            var weakTypesystem = new WeakTypeSystem();
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<int>(), ObjectDescription.Get<object>()));
            Assert.IsFalse(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<int>(), ObjectDescription.Get<string>()));
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<int>(), ObjectDescription.Get<double>()));
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Person>(), ObjectDescription.Get<Person>()));
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Student>(),
                ObjectDescription.Get<Person>()));
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Teacher>(),
                ObjectDescription.Get<Person>()));
            Assert.IsTrue(
                weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Student2>(), ObjectDescription.Get<Person>()));
            Assert.IsTrue(
                weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Teacher2>(), ObjectDescription.Get<Person>()));
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Teacher>(),
                ObjectDescription.Get<Teacher2>()));
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Teacher2>(),
                ObjectDescription.Get<Teacher>()));
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Student>(),
                ObjectDescription.Get<Student2>()));
            Assert.IsTrue(weakTypesystem.IsSubtypeOf(ObjectDescription.Get<Student2>(),
                ObjectDescription.Get<Student>()));
        }

        private class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private class Student : Person
        {
            public string GroupName { get; set; }
        }

        private class Teacher : Person
        {
            public int DepNumber { get; set; }
        }

        private class Student2
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string GroupName { get; set; }
        }

        private class Teacher2
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int DepNumber { get; set; }
        }
    }
}
