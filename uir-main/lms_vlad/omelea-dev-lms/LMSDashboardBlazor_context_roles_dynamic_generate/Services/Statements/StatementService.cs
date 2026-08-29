namespace LMSDashboardBlazor.Services.Statements;

public class StatementService
{
    public StatementModel? CurrentStatement { get; private set; }

    public event Action? OnStatementChanged;

    public StatementService()
    {
        CurrentStatement = GetDefaultStatement();
    }

    public void SetStatement(StatementModel statement)
    {
        CurrentStatement = statement;
        OnStatementChanged?.Invoke();
    }

    public GradeResult CalculateGrade(int total) => total switch
    {
        >= 90 => new GradeResult("Отлично", "5", "A"),
        >= 85 => new GradeResult("Хорошо", "4", "B"),
        >= 75 => new GradeResult("Хорошо", "4", "C"),
        >= 70 => new GradeResult("Хорошо", "4", "D"),
        >= 65 => new GradeResult("Удовлетворительно", "3", "D"),
        >= 60 => new GradeResult("Удовлетворительно", "3", "E"),
        _ => new GradeResult("Неудовлетворительно", "2", "F")
    };

    private StatementModel GetDefaultStatement()
    {
        var students = new List<StudentModel>
        {
            new()
            {
                FullName = "Иванов И.И.",
                Sections = new List<int> { 30, 20 },
                Exam = 40
            },
            new()
            {
                FullName = "Петров П.П.",
                Sections = new List<int> { 25, 25 },
                Exam = 35
            },
            new()
            {
                FullName = "Сидоров С.С.",
                Sections = new List<int> { 20, 30 },
                Exam = 25
            }
        };

        foreach (var student in students)
        {
            Recalculate(student);
        }

        return new StatementModel
        {
            Name = "СТИС",
            Subjects = new List<string> { "Раздел 1", "Раздел 2" },
            Students = students
        };
    }

    private void Recalculate(StudentModel student)
    {
        student.SectionSum = student.Sections.Sum();
        student.Total = student.SectionSum + student.Exam;

        var grade = CalculateGrade(student.Total);
        student.Mark = grade.Text;
        student.Code = grade.Code;
        student.ECTS = grade.ECTS;
    }
}

public sealed record GradeResult(string Text, string Code, string ECTS);

public class StatementModel
{
    public string Name { get; set; } = string.Empty;
    public List<string> Subjects { get; set; } = new();
    public List<StudentModel> Students { get; set; } = new();
}

public class StudentModel
{
    public string FullName { get; set; } = string.Empty;
    public List<int> Sections { get; set; } = new();
    public int Exam { get; set; }
    public int SectionSum { get; set; }
    public int Total { get; set; }
    public string Mark { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ECTS { get; set; } = string.Empty;
}
