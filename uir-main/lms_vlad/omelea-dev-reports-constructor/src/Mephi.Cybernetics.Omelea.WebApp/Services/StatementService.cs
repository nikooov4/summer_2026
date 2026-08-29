using System;
using System.Collections.Generic;
using System.Linq;

namespace Mephi.Cybernetics.Race.WebApp.Services
{
    public class StatementService
    {
        public StatementModel? CurrentStatement { get; set; }

        public event Action? OnStatementChanged;

        public StatementService()
        {
            CurrentStatement = GetDefaultStatement();
        }

        private StatementModel GetDefaultStatement()
        {
            var students = new List<StudentModel>
            {
                new StudentModel
                {
                    FullName = "Иванов И.И.",
                    Sections = new List<int> { 30, 20 },
                    Exam = 40
                },
                new StudentModel
                {
                    FullName = "Петров П.П.",
                    Sections = new List<int> { 25, 25 },
                    Exam = 35
                },
                new StudentModel
                {
                    FullName = "Сидоров С.С.",
                    Sections = new List<int> { 20, 30 },
                    Exam = 25
                }
            };

            foreach (var student in students)
            {
                student.SectionSum = student.Sections.Sum();
                student.Total = student.SectionSum + student.Exam;

                var grade = GetGrade(student.Total);
                student.Mark = grade.Text;
                student.Code = grade.Code;
                student.ECTS = grade.ECTS;
            }

            return new StatementModel
            {
                Name = "CТИС",
                Subjects = new List<string> { "Раздел 1", "Раздел 2" },
                Students = students
            };
        }

        public void SetStatement(StatementModel statement)
        {
            CurrentStatement = statement;
            OnStatementChanged?.Invoke();
        }

        private Grade GetGrade(int total) => total switch
        {
            >= 90 => new Grade("Отлично", "5", "A"),
            >= 85 => new Grade("Хорошо", "4", "B"),
            >= 75 => new Grade("Хорошо", "4", "C"),
            >= 70 => new Grade("Хорошо", "4", "D"),
            >= 65 => new Grade("Удовлетворительно", "3", "D"),
            >= 60 => new Grade("Удовлетворительно", "3", "E"),
            _ => new Grade("Неудовлетворительно", "2", "F"),
        };

        private record Grade(string Text, string Code, string ECTS);
    }

    public class StatementModel
    {
        public string Name { get; set; } = "";
        public List<string> Subjects { get; set; } = new();
        public List<StudentModel> Students { get; set; } = new();
    }

    public class StudentModel
    {
        public string FullName { get; set; } = "";
        public List<int> Sections { get; set; } = new();
        public int Exam { get; set; }
        public int SectionSum { get; set; }
        public int Total { get; set; }
        public string Mark { get; set; } = "";
        public string Code { get; set; } = "";
        public string ECTS { get; set; } = "";
    }
}
