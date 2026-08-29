using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataStructures
{
    public class Concept : SemanticConstruction
    {
        public virtual Type Domain
        {
            get { return GetValue<Type>("Domain"); }
            set { SetValue("Domain", value); }
        }

        public override object[] Parts
        {
            get { return GetValue<object[]>("Parts"); }
        }

        public void SetParts(object[] parts)
        {
            SetValue("Parts", parts);
        }
    }

    public class SimpleConcept : Concept
    {

    }

    public class ComplexConcept : Concept
    {
        [Computable]
        public override Type Domain
        {
            get { return GetValue("Domain", () => typeof (int)); }
            set
            {
                //SetValue("Domain", value);
            }
        }

        public bool Selector
        {
            get { return GetValue<bool>("Selector"); }
            set { SetValue("Selector", value); }
        }

        [Computable]
        public Type Domain2
        {
            get { return GetValue("Domain2", () => Selector ? typeof (DateTime) : typeof (double)); }
        }
    }

    public class SumConcept : Concept
    {
        public List<Concept> NestedConcepts
        {
            get { return GetValue<List<Concept>>("NestedConcepts"); }
            set { SetValue("NestedConcepts", value); }
        }

        [Relation(typeof (Concept), "someColumn", "someTable")]
        public List<Concept> NestedConceptsRelation
        {
            get { return GetValue<List<Concept>>("NestedConceptsRelation"); }
            set { SetValue("NestedConceptsRelation", value); }
        }
    }

}
