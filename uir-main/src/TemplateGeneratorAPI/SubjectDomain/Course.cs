using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using System;
using System.Collections.Generic;

namespace TemplateGeneratorAPI.SubjectDomain
{
    public class Course : EntityBase
    {
        public Course()
        {
            ID = Guid.NewGuid();
        }

        public string Code
        {
            get { return GetValue<string>(nameof(Code)); }
            set { SetValue(nameof(Code), value); }
        }

        public string Name
        {
            get { return GetValue<string>(nameof(Name)); }
            set { SetValue(nameof(Name), value); }
        }

        [Relation(RelationType.OneToMany, typeof(Group), "ID")]
        public List<Group> Groups
        {
            get { return GetValue<List<Group>>(nameof(Groups)); }
            set { SetValue(nameof(Groups), value); }
        }

        [Relation(typeof(Subject), "ID", "")]
        public List<Subject> Subjects
        {
            get { return GetValue<List<Subject>>(nameof(Subjects)); }
            set { SetValue(nameof(Subjects), value); }
        }
    }
}
