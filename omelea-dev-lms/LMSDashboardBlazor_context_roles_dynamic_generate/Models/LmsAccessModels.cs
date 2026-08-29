namespace LMSDashboardBlazor.Models;

public enum LmsCourseRole
{
    Admin,
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

public sealed class LmsUserModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RecordBookNumber { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

public sealed class LmsCourseModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string DefaultRoom { get; set; } = string.Empty;
}

public sealed class LmsEnrollmentModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public LmsCourseRole Role { get; set; }
}
