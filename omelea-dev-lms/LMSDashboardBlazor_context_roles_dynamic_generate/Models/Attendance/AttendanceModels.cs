namespace LMSDashboardBlazor.Models.Attendance;

public enum AttendanceStatus
{
    Unmarked,
    Present,
    Late,
    Excused,
    Absent
}

public sealed class AttendanceDataModel
{
    public List<AttendanceStudentModel> Students { get; set; } = new();
    public List<AttendanceLessonModel> Lessons { get; set; } = new();
}

public sealed class AttendanceStudentModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FullName { get; set; } = string.Empty;
    public string RecordBookNumber { get; set; } = string.Empty;
}

public sealed class AttendanceLessonModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CourseId { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCancelled { get; set; }
    public long Revision { get; set; }
    public List<AttendanceEntryModel> Entries { get; set; } = new();
    public List<AttendanceStudentModel> Roster { get; set; } = new();

    public bool IsCompleted => CompletedAt is not null;
}

public sealed class AttendanceEntryModel
{
    public string StudentId { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public int? LateMinutes { get; set; }
    public string? Comment { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? DataSourceName { get; set; }

    public AttendanceEntryModel Clone()
    {
        return new AttendanceEntryModel
        {
            StudentId = StudentId,
            Status = Status,
            LateMinutes = LateMinutes,
            Comment = Comment,
            UpdatedAt = UpdatedAt,
            DataSourceName = DataSourceName
        };
    }
}

public sealed record AttendanceRowModel(
    AttendanceStudentModel Student,
    AttendanceEntryModel Entry,
    bool IsDirty,
    string? ValidationError);

public sealed record AttendanceSummaryModel(
    int Total,
    int Present,
    int Late,
    int Excused,
    int Absent,
    int Unmarked)
{
    public int Completed => Total - Unmarked;

    public int AttendancePercent =>
        Total == 0 ? 0 : (int)Math.Round((Present + Late + Excused) * 100d / Total);
}

public sealed record AttendanceHistoryItemModel(
    AttendanceLessonModel Lesson,
    AttendanceEntryModel Entry);

public sealed record AttendanceMonitoringSummaryModel(
    int HeldLessons,
    int CompletedLessons,
    int UnfinishedLessons,
    int Present,
    int Late,
    int Excused,
    int Absent,
    int Unmarked)
{
    public int CompletenessPercent =>
        HeldLessons == 0 ? 0 : (int)Math.Round(CompletedLessons * 100d / HeldLessons);

    public int AttendancePercent
    {
        get
        {
            var finalizedEntries = Present + Late + Excused + Absent;
            return finalizedEntries == 0
                ? 0
                : (int)Math.Round((Present + Late) * 100d / finalizedEntries);
        }
    }
}

public sealed record AttendanceLessonMonitoringModel(
    AttendanceLessonModel Lesson,
    AttendanceSummaryModel Summary,
    DateTime LastUpdatedAt)
{
    public int CompletionPercent =>
        Summary.Total == 0 ? 0 : (int)Math.Round(Summary.Completed * 100d / Summary.Total);

    public bool NeedsAttention => !Lesson.IsCompleted && Lesson.StartsAt <= DateTime.Now && Summary.Unmarked > 0;
}

public sealed record AttendanceStudentMonitoringModel(
    AttendanceStudentModel Student,
    int Lessons,
    int Present,
    int Late,
    int Excused,
    int Absent)
{
    public int AttendancePercent =>
        Lessons == 0 ? 0 : (int)Math.Round((Present + Late) * 100d / Lessons);
}
