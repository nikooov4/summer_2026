using System;
using System.Collections.ObjectModel;
using System.Linq;
using Mephi.Cybernetics.Race.Core.Tests.DataStructures;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Mephi.Cybernetics.Race.Core.Tests.EntityTests
{
    [TestFixture]
    public class ErnEntityTest
    {
        [Test]
        public void TestErdEntityOneToMany()
        {
            Selection selection = new Selection();
            var triggerEvent = new TriggerEvent();
            var guid1 = new Guid();
            var guid2 = new Guid();
            selection.Events.Add(triggerEvent);
            selection.Code = "code1";
            selection.Description = "description1";
            selection.Name = "name1";
            selection.ID = guid1;

            var eventValue1D = new EventValue1D();
            var eventValue2D = new EventValue2D();
            var eventValue3D = new EventValue3D();
            SelectionData selectionData = new SelectionData();
            selectionData.Event = new TriggerEvent();
            selectionData.Event.ID = guid1;
            selectionData.Event.Values.Add(eventValue1D);
            selectionData.Event.Values.Add(eventValue2D);
            selectionData.Event.Values.Add(eventValue3D);
            selectionData.Event.IsValid = true;
            selectionData.Event.EventDayIndex = 2;
            selectionData.Event.EventDayNumber = 2;
            selectionData.Event.EventTime = DateTime.Now;
            selectionData.Selection = selection;
            selectionData.Selection.Events.Add(triggerEvent);
            PhysicalVariable physicalVariable = new PhysicalVariable();
            physicalVariable.Code = "code2";
            physicalVariable.Description = "description2";
            physicalVariable.Dimension = 23;
            physicalVariable.Name = "name";
            physicalVariable.ID = guid2;

            var od = ObjectDescription.Get<Selection>();
            ErdDomainModel erdDomainModel = new ErdDomainModel();
            var entity = erdDomainModel.GetEntity(od);

            var assoc = entity.Associations;
            var attributes = entity.Attributes;
            var entityName = entity.Name;
            var entityNamespace = entity.Namespace;
            var entitySource = entity.Source;
            var entityFullName = entity.FullName;
            var entityPotentialKeys = entity.PotentialKeys;
            var entityFullyQualifiedName = entity.FullyQualifiedName;
            Assert.AreEqual("Selection", entityName);
            Assert.AreEqual("Mephi.Cybernetics.Race.Core.Tests.DataStructures", entityNamespace);
            Assert.AreEqual("Mephi.Cybernetics.Race.Core.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                entitySource);
            Assert.AreEqual("Mephi.Cybernetics.Race.Core.Tests.DataStructures.Selection", entityFullName);
            Assert.AreEqual(
                "[Mephi.Cybernetics.Race.Core.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null::Mephi.Cybernetics.Race.Core.Tests.DataStructures.Selection]",
                entityFullyQualifiedName);

            Assert.AreEqual(1, assoc.OneToOne.Count());
            Assert.AreEqual("Event", assoc.OneToOne[0].Name);
            Assert.AreEqual(1, assoc.OneToMany.Count());
            Assert.AreEqual("Events", assoc.OneToMany[0].Name);
            Assert.AreEqual(0, assoc.ManyToMany.Count());

            Assert.AreEqual("ID", attributes.StoredAttributes[0].Name);
            Assert.AreEqual("Name", attributes.StoredAttributes[1].Name);
            Assert.AreEqual("Code", attributes.StoredAttributes[2].Name);
            Assert.AreEqual("Description", attributes.StoredAttributes[3].Name);
            Assert.AreEqual(3, entityPotentialKeys.Count);
            Assert.AreEqual("ID", entityPotentialKeys[0][0].Name);
            Assert.AreEqual("Name",  entityPotentialKeys[1][0].Name);
            Assert.AreEqual("Code",  entityPotentialKeys[1][1].Name);
            Assert.AreEqual("Description", entityPotentialKeys[2][0].Name);
    
        }

        [Test]
        public void TestErdEntityManyToMany()
        {
            var od = ObjectDescription.Get<SelectionData>();
            ErdDomainModel erdDomainModel = new ErdDomainModel();
            var entity = erdDomainModel.GetEntity(od);
            var assoc = entity.Associations;
            var attributes = entity.Attributes;
            var entityName = entity.Name;
            var entityNamespace = entity.Namespace;
            var entitySource = entity.Source;
            var entityFullName = entity.FullName;
            var entityPotentialKeys = entity.PotentialKeys;
            var entityFullyQualifiedName = entity.FullyQualifiedName;
            Assert.AreEqual("SelectionData", entityName);
            Assert.AreEqual("Mephi.Cybernetics.Race.Core.Tests.DataStructures", entityNamespace);
            Assert.AreEqual("Mephi.Cybernetics.Race.Core.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                entitySource);
            Assert.AreEqual("Mephi.Cybernetics.Race.Core.Tests.DataStructures.SelectionData", entityFullName);
            Assert.AreEqual(
                "[Mephi.Cybernetics.Race.Core.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null::Mephi.Cybernetics.Race.Core.Tests.DataStructures.SelectionData]",
                entityFullyQualifiedName);
            Assert.AreEqual(2, assoc.OneToOne.Count());
            Assert.AreEqual("Event", assoc.OneToOne[0].Name);
            Assert.AreEqual("Selection", assoc.OneToOne[1].Name);
            Assert.AreEqual(0, assoc.OneToMany.Count());
            Assert.AreEqual(0, assoc.ManyToMany.Count());
            Assert.AreEqual( 0, entityPotentialKeys.Count);
        }
    }
}