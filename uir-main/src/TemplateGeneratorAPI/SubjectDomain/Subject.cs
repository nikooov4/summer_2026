using Mephi.Cybernetics.Race.DataAccess;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using System;
using System.Collections.Generic;

namespace TemplateGeneratorAPI.SubjectDomain
{
    public class Subject : EntityBase
    {
        public Subject()
        {
            ID = Guid.NewGuid();
        }

        [Relation(typeof(Course), "ID", "")]
        public List<Course> Courses
        {
            get { return GetValue<List<Course>>(nameof(Courses)); }
            set { SetValue(nameof(Courses), value); }
        }
    }
}
