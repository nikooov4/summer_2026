using System;
using System.Collections.Generic;
using System.Reflection;
using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures.ComplexObjects.Entities;
using NUnit.Framework;
using System.Linq;
using Concept = Mephi.Cybernetics.Race.DataAccessTests.DataStructures.Concept;
using Frame = Mephi.Cybernetics.Race.DataAccessTests.DataStructures.Frame;
using SemanticConstruction = Mephi.Cybernetics.Race.DataAccessTests.DataStructures.SemanticConstruction;

namespace Mephi.Cybernetics.Race.DataAccessTests
{
    [TestFixture]
    public class EntityDescriptionTests
    {
        [Test]
        public void TestSimpleClassDescription()
        {
            var od = EntityDescription.GetEntityDescription(typeof(SemanticConstruction));
            Assert.AreEqual(4, od.EntityAttributes.Count);
            Assert.IsTrue(od.EntityAttributes.Contains("ID"));
            Assert.IsTrue(od.EntityAttributes.Contains("Name"));
            Assert.IsTrue(od.EntityAttributes.Contains("Parent"));
        }

        [Test]
        public void TestComplexEntityDescription()
        {
            //Assert.IsNotNull(typeof (BookConstructor).GetCustomAttribute<ObjectConstructor>());
            var entityConstructorOd = EntityConstructorDescription.GetConstructorDescription<BookConstructor>();
        }

        [Test]
        public void TestHidingProperty()
        {

            var propInfos = new PropertyInfo[2];
            var index = 0;
            //propInfos[0].
            foreach (var property in typeof(Frame).GetProperties())
            {
                if (property.Name == "Parent")
                    propInfos[index++] = property;
                if (index > 1)
                    break;
            }
            var i = 0;
        }

        [Test]
        public void TestClassWithHidingProperty()
        {
            var od = EntityDescription.GetEntityDescription(typeof(Frame));
            Assert.AreEqual(5, od.EntityAttributes.Count);
            Assert.IsTrue(od.EntityAttributes.Contains("ID"));
            Assert.IsTrue(od.EntityAttributes.Contains("Name"));
            Assert.IsTrue(od.EntityAttributes.Contains("Parent"));
            Assert.IsTrue(od.EntityAttributes.Contains("Valuation"));

            var odBase = EntityDescription.GetEntityDescription(typeof(SemanticConstruction));
            //            Assert.AreEqual(odBase.EntityAttributes["ID"].Index, od.EntityAttributes["ID"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Name"].Index, od.EntityAttributes["Name"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Parent"].Index, od.EntityAttributes["Parent"].Index);
        }

        [Test]
        public void TestClassWithVirtualProperty()
        {
            var od = EntityDescription.GetEntityDescription(typeof(Concept));
            Assert.AreEqual(5, od.EntityAttributes.Count);
            Assert.IsTrue(od.EntityAttributes.Contains("ID"));
            Assert.IsTrue(od.EntityAttributes.Contains("Name"));
            Assert.IsTrue(od.EntityAttributes.Contains("Parent"));
            Assert.IsTrue(od.EntityAttributes.Contains("Domain"));

            var odBase = EntityDescription.GetEntityDescription(typeof(SemanticConstruction));
            //            Assert.AreEqual(odBase.EntityAttributes["ID"].Index, od.EntityAttributes["ID"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Name"].Index, od.EntityAttributes["Name"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Parent"].Index, od.EntityAttributes["Parent"].Index);
        }

        [Test]
        public void TestClassWithNoOverrideForVirtualProperty()
        {
            var od = EntityDescription.GetEntityDescription(typeof(SimpleConcept));
            Assert.AreEqual(5, od.EntityAttributes.Count);
            Assert.IsTrue(od.EntityAttributes.Contains("ID"));
            Assert.IsTrue(od.EntityAttributes.Contains("Name"));
            Assert.IsTrue(od.EntityAttributes.Contains("Parent"));
            Assert.IsTrue(od.EntityAttributes.Contains("Parts"));
            Assert.IsTrue(od.EntityAttributes.Contains("Domain"));

            var odBase = EntityDescription.GetEntityDescription(typeof(Concept));
            //            Assert.AreEqual(odBase.EntityAttributes["ID"].Index, od.EntityAttributes["ID"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Name"].Index, od.EntityAttributes["Name"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Parent"].Index, od.EntityAttributes["Parent"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Parts"].Index, od.EntityAttributes["Parts"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Domain"].Index, od.EntityAttributes["Domain"].Index);
        }

        [Test]
        public void TestClassWithOverrideForVirtualProperty()
        {
            var od = EntityDescription.GetEntityDescription(typeof(SimpleConcept));
            Assert.AreEqual(5, od.EntityAttributes.Count);
            Assert.IsTrue(od.EntityAttributes.Contains("ID"));
            Assert.IsTrue(od.EntityAttributes.Contains("Name"));
            Assert.IsTrue(od.EntityAttributes.Contains("Parent"));
            Assert.IsTrue(od.EntityAttributes.Contains("Domain"));

            var odBase = EntityDescription.GetEntityDescription(typeof(Concept));
            //            Assert.AreEqual(odBase.EntityAttributes["ID"].Index, od.EntityAttributes["ID"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Name"].Index, od.EntityAttributes["Name"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Parent"].Index, od.EntityAttributes["Parent"].Index);
            //            Assert.AreEqual(odBase.EntityAttributes["Domain"].Index, od.EntityAttributes["Domain"].Index);
        }

        [Test]
        public void TestPropertyGetValueFromTheMostSpecificOd()
        {
            var sc = new SimpleConcept
            {
                Name = "111",
                Domain = typeof(string)
            };
            var od = EntityDescription.GetEntityDescription(typeof(SimpleConcept));
            Assert.AreEqual("111", od.EntityAttributes["Name"].GetValue(sc));
            Assert.AreEqual(typeof(string), od.EntityAttributes["Domain"].GetValue(sc));

            var cc = new ComplexConcept
            {
                Name = "222",
                Domain = typeof(string)
            };
            var od2 = EntityDescription.GetEntityDescription(typeof(ComplexConcept));
            Assert.AreEqual("222", od2.EntityAttributes["Name"].GetValue(cc));
            Assert.AreEqual(typeof(int), od2.EntityAttributes["Domain"].GetValue(cc));
        }

        [Test]
        public void TestComputableProperty()
        {
            var cc = new ComplexConcept
            {
                Domain = typeof(string),
                Selector = true
            };

            var od = EntityDescription.GetEntityDescription(typeof(ComplexConcept));

            Assert.AreEqual(true, od.EntityAttributes["Selector"].GetValue(cc));

            Assert.AreEqual(typeof(int), od.EntityAttributes["Domain"].GetValue(cc));
            Assert.AreEqual(typeof(int), cc.Domain);
            Assert.AreEqual(typeof(int), od.EntityAttributes["Domain"].GetValue(cc)); // check the value has not changed on its own
            Assert.AreEqual(true, od.EntityAttributes["Selector"].GetValue(cc));


            Assert.AreEqual(typeof(DateTime), od.EntityAttributes["Domain2"].GetValue(cc));
            Assert.AreEqual(typeof(DateTime), cc.Domain2);
            Assert.AreEqual(typeof(DateTime), od.EntityAttributes["Domain2"].GetValue(cc)); // check the value has not changed on its own

            cc.Selector = false;
            Assert.IsTrue(cc["Domain"] is UnasignedValue);
            Assert.IsTrue(cc["Domain2"] is UnasignedValue);
            Assert.AreEqual(typeof(double), od.EntityAttributes["Domain2"].GetValue(cc));
            Assert.AreEqual(typeof(double), cc.Domain2);
            Assert.AreEqual(typeof(int), od.EntityAttributes["Domain"].GetValue(cc));

            cc.Selector = true;
            Assert.IsTrue(cc["Domain"] is UnasignedValue);
            Assert.IsTrue(cc["Domain2"] is UnasignedValue);
            Assert.AreEqual(typeof(DateTime), od.EntityAttributes["Domain2"].GetValue(cc));
            Assert.AreEqual(typeof(DateTime), cc.Domain2);
            Assert.AreEqual(typeof(int), od.EntityAttributes["Domain"].GetValue(cc));
        }

        [Test]
        public void TestPropertyGetValueFromBaseClassOd()
        {
            var sc = new SimpleConcept
            {
                Name = "111",
                Domain = typeof(string)
            };
            var cc = new ComplexConcept
            {
                Name = "222",
                Domain = typeof(string)
            };

            var od = EntityDescription.GetEntityDescription(typeof(Concept));
            Assert.AreEqual("111", od.EntityAttributes["Name"].GetValue(sc));
            Assert.AreEqual(typeof(string), od.EntityAttributes["Domain"].GetValue(sc));

            Assert.AreEqual("222", od.EntityAttributes["Name"].GetValue(cc));
            Assert.AreEqual(typeof(int), od.EntityAttributes["Domain"].GetValue(cc));
        }

        [Test]
        public void TestGetInterfacePropertyValueFromInterface()
        {
            var sc = new SemanticConstruction
            {
                Name = "123"
            };

            var concept = new Concept
            {
                Name = "456",
                Domain = typeof(int)
            };
            concept.SetParts(new object[] { "4", "5", "6" });

            var iDecomposableOd = EntityDescription.GetEntityDescription(typeof(IDecomposable));
            var partsProperty = iDecomposableOd.EntityAttributes["Parts"];

            Assert.AreEqual(sc.Parts, partsProperty.GetValue(sc));
            Assert.AreEqual(concept.Parts, partsProperty.GetValue(concept));
        }

        [Test]
        public void TestAggregatedClassObjectDescription()
        {
            var obj = EntityDescription.GetEntityDescription(typeof(ApplicativeObject));
            var applObj = EntityDescription.GetEntityDescription(typeof(ObjectApplication));
            var absObj = EntityDescription.GetEntityDescription(typeof(ObjectAbstraction));

            Assert.IsTrue(applObj.Parents.Contains(obj));
            Assert.IsTrue(absObj.Parents.Contains(obj));
        }

        //        [Test]
        //        public void TestRoleCategory()
        //        {
        //            var od = EntityDescription.GetEntityDescription(typeof(RoleCategoryDescriptor));
        //        }

        [Test]
        public void TestPropertyDefaultValue()
        {
            var conceptOd = EntityDescription.GetEntityDescription(typeof(SumConcept));
            Assert.AreEqual(null, conceptOd.EntityAttributes["Name"].ObjectProperty.PropertyDefaultValue);
            Assert.AreEqual(null, conceptOd.EntityAttributes["Parent"].ObjectProperty.PropertyDefaultValue);
            Assert.AreNotEqual(null, conceptOd.EntityAttributes["NestedConcepts"].ObjectProperty.PropertyDefaultValue);
            Assert.AreEqual(typeof(List<Concept>), conceptOd.EntityAttributes["NestedConcepts"].ObjectProperty.PropertyDefaultValue.GetType());
            Assert.AreNotEqual(null, conceptOd.EntityAttributes["NestedConceptsRelation"].ObjectProperty.PropertyDefaultValue);
            Assert.AreEqual(typeof(List<Concept>), conceptOd.EntityAttributes["NestedConceptsRelation"].ObjectProperty.PropertyDefaultValue.GetType());
        }

        //        [Test]
        //        public void TestConceptDescriptorAttributesProperty()
        //        {
        //            LambdaInterop.Init();
        //
        //        }
    }
}
