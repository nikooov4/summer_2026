namespace LMSDashboardBlazor.Models;

public enum LmsCourseRole
{
    Student,
    Teacher,
    Assistant,
    Curator
}

public sealed class CourseAccessContext
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public LmsCourseRole Role { get; set; }
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
}
