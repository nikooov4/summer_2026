using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using NUnit.Framework;

namespace Mephi.Cybernetics.Race.DataAccessTests
{
    [TestFixture]
    public class EntityBaseTests
    {
        [Test]
        public void TestAcceptChanges()
        {
            var complexFrame = new SumConcept();
            var parentFrame = new Concept();

            Assert.AreEqual(null, complexFrame.Name);
            Assert.AreEqual(null, complexFrame.Parent);
            Assert.AreEqual(0, complexFrame.NestedConcepts.Count);
            Assert.AreEqual(0, complexFrame.NestedConceptsRelation.Count);

            complexFrame.Name = "1";
            complexFrame.Parent = parentFrame;
            complexFrame.NestedConcepts.Add(parentFrame);
            complexFrame.NestedConceptsRelation.Add(parentFrame);
            Assert.AreEqual("1", complexFrame.Name);
            Assert.AreEqual(parentFrame, complexFrame.Parent);
            Assert.AreEqual(1, complexFrame.NestedConcepts.Count);
            Assert.AreEqual(parentFrame, complexFrame.NestedConcepts[0]);
            Assert.AreEqual(1, complexFrame.NestedConceptsRelation.Count);
            Assert.AreEqual(parentFrame, complexFrame.NestedConceptsRelation[0]);

            complexFrame.AcceptChanges();
            Assert.AreEqual("1", complexFrame.Name);
            Assert.AreEqual(parentFrame, complexFrame.Parent);
            Assert.AreEqual(1, complexFrame.NestedConcepts.Count);
            Assert.AreEqual(parentFrame, complexFrame.NestedConcepts[0]);
            Assert.AreEqual(1, complexFrame.NestedConceptsRelation.Count);
            Assert.AreEqual(parentFrame, complexFrame.NestedConceptsRelation[0]);
        }
    }
}
