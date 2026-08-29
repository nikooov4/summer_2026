using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using System;
using System.Collections.Generic;

namespace TemplateGeneratorAPI.SubjectDomain
{
    public class Group : EntityBase
    {
        public Group()
        {
            ID = Guid.NewGuid();
        }

        public string Code
        {
            get { return GetValue<string>(nameof(Code)); }
            set { SetValue(nameof(Code), value); }
        }

        [Relation(RelationType.OneToMany, typeof(Student), "ID")]
        public List<Student> Students {
            get { return GetValue<List<Student>>(nameof(Students)); }
            set { SetValue(nameof(Students), value); }
        }

        [Relation(RelationType.OneToOne, typeof(Course), "ID")]
        public List<Course> Course {
            get { return GetValue<List<Course>>(nameof(Course)); }
            set { SetValue(nameof(Course), value); }
        }
    }
}