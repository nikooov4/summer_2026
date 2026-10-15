namespace LMSDashboardBlazor.Models.Attendance;

public sealed record LessonEditRequest(string CourseId, string Topic, string Room, DateTime StartsAt, long Revision);

public sealed record JournalRevisionRequest(long Revision);

public sealed record AttendanceAuditItem(
    long Id,
    string LessonId,
    string VisitId,
    string StudentId,
    string StudentName,
    string RecordBookNumber,
    string ActorId,
    string ActorName,
    DateTime ChangedAt,
    AttendanceStatus? OldStatus,
    AttendanceStatus NewStatus,
    int? OldLateMinutes,
    int? NewLateMinutes,
    string? OldComment,
    string? NewComment,
    string? DataSourceName,
    AttendanceStatus CurrentStatus,
    int? CurrentLateMinutes,
    string? CurrentComment,
    DateTime? CurrentUpdatedAt);
