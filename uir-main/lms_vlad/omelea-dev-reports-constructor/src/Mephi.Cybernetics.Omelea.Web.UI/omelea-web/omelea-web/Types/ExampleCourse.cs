using Mephi.Cybernetics.Omelea.Core.Models.Course;
using Mephi.Cybernetics.Omelea.Core.Models.CourseReport;

namespace Mephi.Cybernetics.Omelea.Core.Models.Example
{
    public class ExampleCourse
    {
        public Guid Id { get; set; }

        public string CourseName { get; set; }

        public string Description { get; set; }

        //public virtual ICollection<CourseInstance> CourseInstances { get; set; } = new List<CourseInstance>();
    }

}

