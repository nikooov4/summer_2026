using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using System.Collections.Generic;
using System;

namespace TemplateGeneratorAPI.SubjectDomain
{
    public class FinalQualifyingWork : EntityBase
    {
        public string Topic
        {
            get { return GetValue<string>(nameof(Topic)); }
            set { SetValue(nameof(Topic), value); }
        }

        public string CommissionDate
        {
            get { return GetValue<string>(nameof(CommissionDate)); }
            set { SetValue(nameof(CommissionDate), value); }
        }

        [Relation(RelationType.OneToOne, typeof(Student), "ID")]
        public Student Student
        {
            get { return GetValue<Student>(nameof(Student)); }
            set { SetValue(nameof(Student), value); }
        }

        [Relation(RelationType.OneToOne, typeof(Academic), "ID")]
        public Academic Superviser
        {
            get { return GetValue<Academic>(nameof(Superviser)); }
            set { SetValue(nameof(Superviser), value); }
        }

        [Relation(typeof(Academic), "ID", "")]
        public List<Academic> Consultants
        {
            get { return GetValue<List<Academic>>(nameof(Consultants)); }
            set { SetValue(nameof(Consultants), value); }
        }

        [Relation(typeof(Academic), "ID", "")]
        public List<Academic> CommissionMembers
        {
            get { return GetValue<List<Academic>>(nameof(CommissionMembers)); }
            set { SetValue(nameof(CommissionMembers), value); }
        }
    }
}
