using LMSDashboardBlazor.Models.Attendance;
using LMSDashboardBlazor.Services.Statements;

namespace LMSDashboardBlazor.Models;

public sealed class LmsSnapshot
{
    public List<LmsUserModel> Users { get; set; } = [];
    public List<LmsCourseModel> Courses { get; set; } = [];
    public List<LmsEnrollmentModel> Enrollments { get; set; } = [];
    public AttendanceDataModel Attendance { get; set; } = new();
    public StatementModel? Statement { get; set; }
    public long StatementRevision { get; set; }
}

public sealed class CatalogCommand
{
    public string Kind { get; set; } = "";
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string RecordBookNumber { get; set; } = "";
    public bool IsAdmin { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string GroupName { get; set; } = "";
    public string DefaultRoom { get; set; } = "";
    public string UserId { get; set; } = "";
    public string CourseId { get; set; } = "";
    public LmsCourseRole Role { get; set; }
    public string Format { get; set; } = "";
    public string Payload { get; set; } = "";
    public List<OcrImportRow> Rows { get; set; } = [];
}

public sealed record CatalogCommandResult(LmsSnapshot Snapshot, int Count = 0, int Created = 0, int Enrolled = 0);
public sealed record JournalSaveRequest(long Revision, List<AttendanceEntryModel> Entries);
public sealed record JournalStateRequest(long Revision);
public sealed record StatementSaveRequest(StatementModel Statement, long Revision);
public sealed record StatementSaveResult(StatementModel Statement, long Revision);
