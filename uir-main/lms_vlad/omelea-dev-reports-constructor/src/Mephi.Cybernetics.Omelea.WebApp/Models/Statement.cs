using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.WebApp.Models
{
    public class test
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Student> Students { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<Grade> Grades { get; set; }
    }
}
