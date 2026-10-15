using LMSDashboardBlazor.Models;
using LMSDashboardBlazor.Models.Attendance;
using LMSDashboardBlazor.Services.Admin;

namespace LMSDashboardBlazor.Services.Attendance;

public sealed class AttendanceService
{
    private readonly LmsCatalogService catalogService;
    private readonly Dictionary<string, List<AttendanceEntryModel>> confirmedEntries = new();
    private bool persistentMode;

    public AttendanceDataModel Data { get; private set; }
    public string? SelectedLessonId { get; private set; }

    public event Action? Changed;

    public AttendanceService(LmsCatalogService catalogService)
    {
        this.catalogService = catalogService;
        catalogService.Changed += SyncFromCatalog;

        Data = CreateSampleData(catalogService);
        SyncFromCatalog();
        SelectedLessonId = Data.Lessons.OrderByDescending(lesson => lesson.StartsAt).FirstOrDefault()?.Id;
        CaptureConfirmedState();
    }

    public AttendanceLessonModel? SelectedLesson =>
        Data.Lessons.FirstOrDefault(lesson => lesson.Id == SelectedLessonId) ??
        Data.Lessons.OrderByDescending(lesson => lesson.StartsAt).FirstOrDefault();

    public void Load(AttendanceDataModel data)
    {
        persistentMode = true;
        Data = data;
        if (Data.Lessons.All(lesson => lesson.Id != SelectedLessonId))
            SelectedLessonId = Data.Lessons.OrderByDescending(lesson => lesson.StartsAt).FirstOrDefault()?.Id;
        confirmedEntries.Clear();
        CaptureConfirmedState();
        Changed?.Invoke();
    }

    public void AcceptSavedLesson(AttendanceLessonModel saved)
    {
        var index = Data.Lessons.FindIndex(lesson => lesson.Id == saved.Id);
        if (index < 0) throw new InvalidOperationException("Занятие не найдено.");
        Data.Lessons[index] = saved;
        confirmedEntries[saved.Id] = saved.Entries.Select(entry => entry.Clone()).ToList();
        Changed?.Invoke();
    }

    public void SelectLesson(string lessonId)
    {
        if (Data.Lessons.Any(lesson => lesson.Id == lessonId))
        {
            SelectedLessonId = lessonId;
            Changed?.Invoke();
        }
    }

    public IReadOnlyList<AttendanceLessonModel> GetVisibleLessons(string? username)
    {
        var user = catalogService.GetUserByUsername(username);
        if (user is null) return Array.Empty<AttendanceLessonModel>();

        var allowedCourseIds = catalogService.GetCoursesForUser(username)
            .Select(course => course.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return Data.Lessons
            .Where(lesson => allowedCourseIds.Contains(lesson.CourseId) ||
                (persistentMode && lesson.Entries.Any(entry => entry.StudentId == user.Id)))
            .OrderByDescending(lesson => lesson.StartsAt)
            .ToList();
    }

    public bool CanEditLesson(string? username, AttendanceLessonModel lesson)
    {
        if (lesson.IsCancelled) return false;
        var role = catalogService.GetCourseRole(username, lesson.CourseId);
        return role is LmsCourseRole.Admin or LmsCourseRole.Teacher or LmsCourseRole.Assistant;
    }

    public AttendanceStudentModel? GetCurrentStudent(string? username)
    {
        var user = catalogService.GetUserByUsername(username);
        if (user is null) return null;
        return SelectedLesson?.Roster.FirstOrDefault(student => student.Id == user.Id)
            ?? Data.Students.FirstOrDefault(student => student.Id == user.Id)
            ?? Data.Lessons.SelectMany(lesson => lesson.Roster).FirstOrDefault(student => student.Id == user.Id);
    }

    public IReadOnlyList<AttendanceStudentModel> GetStudentsForLesson(AttendanceLessonModel lesson)
    {
        var studentIds = lesson.Entries
            .Select(entry => entry.StudentId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var students = lesson.Roster.Count > 0 ? lesson.Roster : Data.Students;
        return students
            .Where(student => studentIds.Contains(student.Id))
            .OrderBy(student => student.FullName)
            .ToList();
    }

    public IReadOnlyList<AttendanceRowModel> GetRows(
        AttendanceLessonModel lesson,
        string? searchText,
        AttendanceStatus? statusFilter)
    {
        var normalizedSearch = searchText?.Trim();
        var entries = lesson.Entries.ToDictionary(entry => entry.StudentId);

        return GetStudentsForLesson(lesson)
            .Where(student =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                student.FullName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                student.RecordBookNumber.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
            .Select(student =>
            {
                var entry = entries.TryGetValue(student.Id, out var existingEntry)
                    ? existingEntry
                    : CreateEntry(lesson, student.Id);

                return new AttendanceRowModel(
                    student,
                    entry,
                    IsEntryDirty(lesson, entry),
                    ValidateEntry(entry));
            })
            .Where(row => statusFilter is null || row.Entry.Status == statusFilter)
            .OrderBy(row => row.Student.FullName)
            .ToList();
    }

    public AttendanceSummaryModel GetSummary(AttendanceLessonModel lesson)
    {
        return new AttendanceSummaryModel(
            lesson.Entries.Count,
            lesson.Entries.Count(entry => entry.Status == AttendanceStatus.Present),
            lesson.Entries.Count(entry => entry.Status == AttendanceStatus.Late),
            lesson.Entries.Count(entry => entry.Status == AttendanceStatus.Excused),
            lesson.Entries.Count(entry => entry.Status == AttendanceStatus.Absent),
            lesson.Entries.Count(entry => entry.Status == AttendanceStatus.Unmarked));
    }

    public IReadOnlyList<AttendanceLessonMonitoringModel> GetLessonMonitoring(DateTime? periodStart)
    {
        return Data.Lessons
            .Where(lesson => !lesson.IsCancelled && lesson.StartsAt <= DateTime.Now)
            .Where(lesson => periodStart is null || lesson.StartsAt >= periodStart)
            .OrderByDescending(lesson => lesson.StartsAt)
            .Select(lesson => new AttendanceLessonMonitoringModel(
                lesson,
                GetSummary(lesson),
                GetLastUpdatedAt(lesson)))
            .ToList();
    }

    public IReadOnlyList<AttendanceLessonMonitoringModel> GetLessonMonitoring(DateTime? periodStart, string? username)
    {
        var visibleLessonIds = GetVisibleLessons(username)
            .Select(lesson => lesson.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return GetLessonMonitoring(periodStart)
            .Where(item => visibleLessonIds.Contains(item.Lesson.Id))
            .ToList();
    }

    public AttendanceMonitoringSummaryModel GetMonitoringSummary(DateTime? periodStart)
    {
        var lessons = GetLessonMonitoring(periodStart);
        var completedLessons = lessons.Where(item => item.Lesson.IsCompleted).ToList();
        var completedEntries = completedLessons.SelectMany(item => item.Lesson.Entries).ToList();

        return new AttendanceMonitoringSummaryModel(
            lessons.Count,
            completedLessons.Count,
            lessons.Count - completedLessons.Count,
            completedEntries.Count(entry => entry.Status == AttendanceStatus.Present),
            completedEntries.Count(entry => entry.Status == AttendanceStatus.Late),
            completedEntries.Count(entry => entry.Status == AttendanceStatus.Excused),
            completedEntries.Count(entry => entry.Status == AttendanceStatus.Absent),
            lessons.SelectMany(item => item.Lesson.Entries)
                .Count(entry => entry.Status == AttendanceStatus.Unmarked));
    }

    public AttendanceMonitoringSummaryModel GetMonitoringSummary(DateTime? periodStart, string? username)
    {
        var lessons = GetLessonMonitoring(periodStart, username);
        var completedLessons = lessons.Where(item => item.Lesson.IsCompleted).ToList();
        var completedEntries = completedLessons.SelectMany(item => item.Lesson.Entries).ToList();

        return new AttendanceMonitoringSummaryModel(
            lessons.Count,
            completedLessons.Count,
            lessons.Count - completedLessons.Count,
            completedEntries.Count(entry => entry.Status == AttendanceStatus.Present),
            completedEntries.Count(entry => entry.Status == AttendanceStatus.Late),
            completedEntries.Count(entry => entry.Status == AttendanceStatus.Excused),
            completedEntries.Count(entry => entry.Status == AttendanceStatus.Absent),
            lessons.SelectMany(item => item.Lesson.Entries)
                .Count(entry => entry.Status == AttendanceStatus.Unmarked));
    }

    public IReadOnlyList<AttendanceStudentMonitoringModel> GetStudentMonitoring(DateTime? periodStart)
    {
        var completedLessons = GetLessonMonitoring(periodStart)
            .Where(item => item.Lesson.IsCompleted)
            .Select(item => item.Lesson)
            .ToList();

        return Data.Students
            .Select(student =>
            {
                var entries = completedLessons
                    .Select(lesson => lesson.Entries.FirstOrDefault(entry => entry.StudentId == student.Id))
                    .Where(entry => entry is not null && entry.Status != AttendanceStatus.Unmarked)
                    .Select(entry => entry!)
                    .ToList();

                return new AttendanceStudentMonitoringModel(
                    student,
                    entries.Count,
                    entries.Count(entry => entry.Status == AttendanceStatus.Present),
                    entries.Count(entry => entry.Status == AttendanceStatus.Late),
                    entries.Count(entry => entry.Status == AttendanceStatus.Excused),
                    entries.Count(entry => entry.Status == AttendanceStatus.Absent));
            })
            .OrderBy(item => item.AttendancePercent)
            .ThenBy(item => item.Student.FullName)
            .ToList();
    }

    public IReadOnlyList<AttendanceStudentMonitoringModel> GetStudentMonitoring(DateTime? periodStart, string? username)
    {
        var lessons = GetLessonMonitoring(periodStart, username)
            .Where(item => item.Lesson.IsCompleted)
            .Select(item => item.Lesson)
            .ToList();

        var studentIds = lessons
            .SelectMany(lesson => lesson.Entries)
            .Select(entry => entry.StudentId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var currentStudent = GetCurrentStudent(username);
        var currentRoleIsOnlyStudent = currentStudent is not null && !catalogService.IsAdmin(username) &&
            lessons.All(lesson => catalogService.GetCourseRole(username, lesson.CourseId) is not
                (LmsCourseRole.Admin or LmsCourseRole.Teacher or LmsCourseRole.Assistant or LmsCourseRole.Curator));

        if (currentRoleIsOnlyStudent)
        {
            studentIds.Clear();
            studentIds.Add(currentStudent!.Id);
        }

        return lessons.SelectMany(GetStudentsForLesson)
            .Concat(Data.Students)
            .Concat(currentStudent is null ? Array.Empty<AttendanceStudentModel>() : new[] { currentStudent })
            .DistinctBy(student => student.Id)
            .Where(student => studentIds.Contains(student.Id))
            .Select(student =>
            {
                var entries = lessons
                    .Select(lesson => lesson.Entries.FirstOrDefault(entry => entry.StudentId == student.Id))
                    .Where(entry => entry is not null && entry.Status != AttendanceStatus.Unmarked)
                    .Select(entry => entry!)
                    .ToList();

                return new AttendanceStudentMonitoringModel(
                    student,
                    entries.Count,
                    entries.Count(entry => entry.Status == AttendanceStatus.Present),
                    entries.Count(entry => entry.Status == AttendanceStatus.Late),
                    entries.Count(entry => entry.Status == AttendanceStatus.Excused),
                    entries.Count(entry => entry.Status == AttendanceStatus.Absent));
            })
            .OrderBy(item => item.AttendancePercent)
            .ThenBy(item => item.Student.FullName)
            .ToList();
    }

    public void UpdateStatus(AttendanceEntryModel entry, AttendanceStatus status)
    {
        EnsureEntryEditable(entry);
        entry.Status = status;

        if (status != AttendanceStatus.Late)
        {
            entry.LateMinutes = null;
        }

        Changed?.Invoke();
    }

    public void UpdateLateMinutes(AttendanceEntryModel entry, string? value)
    {
        EnsureEntryEditable(entry);
        entry.LateMinutes = int.TryParse(value, out var minutes) ? Math.Max(0, minutes) : null;
        Changed?.Invoke();
    }

    public void UpdateComment(AttendanceEntryModel entry, string? value)
    {
        EnsureEntryEditable(entry);
        entry.Comment = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        Changed?.Invoke();
    }

    public int MarkUnmarkedPresent(IEnumerable<AttendanceRowModel> rows)
    {
        var entries = rows.Where(row => row.Entry.Status == AttendanceStatus.Unmarked).Select(row => row.Entry).ToList();
        foreach (var entry in entries) EnsureEntryEditable(entry);
        var changed = 0;

        foreach (var entry in entries)
        {
            entry.Status = AttendanceStatus.Present;
            entry.LateMinutes = null;
            changed++;
        }

        if (changed > 0)
        {
            Changed?.Invoke();
        }

        return changed;
    }

    public int ApplyRecognizedPresent(string? username, string lessonId, IEnumerable<string> studentIds, bool replaceExisting)
    {
        var lesson = Data.Lessons.FirstOrDefault(item => item.Id == lessonId)
            ?? throw new InvalidOperationException("Занятие не найдено.");
        if (!CanEditLesson(username, lesson))
            throw new InvalidOperationException("Нет прав на заполнение этого журнала.");
        if (lesson.IsCompleted)
            throw new InvalidOperationException("Журнал завершён. Сначала возобновите его.");
        var ids = studentIds.ToList();
        if (ids.Count == 0 || ids.Distinct().Count() != ids.Count)
            throw new InvalidOperationException("Выберите студентов без повторов.");
        var allowedIds = lesson.Entries.Select(entry => entry.StudentId).ToHashSet(StringComparer.Ordinal);
        if (ids.Any(id => !allowedIds.Contains(id)))
            throw new InvalidOperationException("Студент отсутствует в составе этого занятия. Проверьте распознанный список заново.");
        var changed = 0;
        foreach (var entry in lesson.Entries.Where(entry => ids.Contains(entry.StudentId)))
        {
            if (entry.Status == AttendanceStatus.Present || (!replaceExisting && entry.Status != AttendanceStatus.Unmarked)) continue;
            entry.Status = AttendanceStatus.Present;
            entry.LateMinutes = null;
            entry.DataSourceName = "attendance-ocr-service";
            changed++;
        }
        if (changed > 0) Changed?.Invoke();
        return changed;
    }

    public bool HasUnsavedChanges(AttendanceLessonModel lesson)
    {
        return lesson.Entries.Any(entry => IsEntryDirty(lesson, entry));
    }

    public int DirtyCount(AttendanceLessonModel lesson)
    {
        return lesson.Entries.Count(entry => IsEntryDirty(lesson, entry));
    }

    public void SaveSelectedLesson()
    {
        var lesson = SelectedLesson ?? throw new InvalidOperationException("Занятие не выбрано.");
        EnsureLessonEditable(lesson);
        var validationErrors = lesson.Entries
            .Select(entry => ValidateEntry(entry))
            .Where(error => error is not null)
            .ToList();

        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Нельзя сохранить журнал: исправьте {validationErrors.Count} некорректных строк.");
        }

        var now = DateTime.Now;
        foreach (var entry in lesson.Entries.Where(entry => IsEntryDirty(lesson, entry)))
        {
            entry.UpdatedAt = now;
        }

        confirmedEntries[lesson.Id] = lesson.Entries.Select(entry => entry.Clone()).ToList();
        Changed?.Invoke();
    }

    public void RevertSelectedLesson()
    {
        var lesson = SelectedLesson ?? throw new InvalidOperationException("Занятие не выбрано.");
        if (!confirmedEntries.TryGetValue(lesson.Id, out var confirmed))
        {
            return;
        }

        lesson.Entries = confirmed.Select(entry => entry.Clone()).ToList();
        Changed?.Invoke();
    }

    public void CompleteLesson(string lessonId)
    {
        var lesson = Data.Lessons.FirstOrDefault(item => item.Id == lessonId) ??
            throw new InvalidOperationException("Занятие не найдено.");

        EnsureLessonEditable(lesson);

        if (HasUnsavedChanges(lesson))
        {
            throw new InvalidOperationException("Сначала сохраните изменения в журнале.");
        }

        var errors = lesson.Entries.Count(entry => ValidateEntry(entry) is not null);
        if (errors > 0)
        {
            throw new InvalidOperationException($"Исправьте ошибки в {errors} строках перед завершением.");
        }

        var unmarked = lesson.Entries.Count(entry => entry.Status == AttendanceStatus.Unmarked);
        if (unmarked > 0)
        {
            throw new InvalidOperationException($"Нельзя завершить журнал: не отмечено студентов - {unmarked}.");
        }

        lesson.CompletedAt = DateTime.Now;
        Changed?.Invoke();
    }

    public void ReopenLesson(string lessonId)
    {
        var lesson = Data.Lessons.FirstOrDefault(item => item.Id == lessonId) ??
            throw new InvalidOperationException("Занятие не найдено.");

        if (lesson.IsCancelled) throw new InvalidOperationException("Занятие отменено.");
        lesson.CompletedAt = null;
        Changed?.Invoke();
    }

    public IReadOnlyList<AttendanceHistoryItemModel> GetStudentHistory(string studentId)
    {
        return Data.Lessons
            .Select(lesson => new
            {
                Lesson = lesson,
                Entry = lesson.Entries.FirstOrDefault(entry => entry.StudentId == studentId)
            })
            .Where(item => item.Entry is not null)
            .OrderByDescending(item => item.Lesson.StartsAt)
            .Select(item => new AttendanceHistoryItemModel(item.Lesson, item.Entry!))
            .ToList();
    }

    public AttendanceSummaryModel GetStudentSummary(string studentId)
    {
        var entries = Data.Lessons
            .Where(lesson => !lesson.IsCancelled)
            .SelectMany(lesson => lesson.Entries)
            .Where(entry => entry.StudentId == studentId)
            .ToList();

        return new AttendanceSummaryModel(
            entries.Count,
            entries.Count(entry => entry.Status == AttendanceStatus.Present),
            entries.Count(entry => entry.Status == AttendanceStatus.Late),
            entries.Count(entry => entry.Status == AttendanceStatus.Excused),
            entries.Count(entry => entry.Status == AttendanceStatus.Absent),
            entries.Count(entry => entry.Status == AttendanceStatus.Unmarked));
    }

    public void ResetSample()
    {
        if (persistentMode) throw new InvalidOperationException("Загруженные журналы нельзя заменить демонстрационными данными.");
        Data = CreateSampleData(catalogService);
        SyncFromCatalog();
        SelectedLessonId = Data.Lessons.OrderByDescending(lesson => lesson.StartsAt).First().Id;
        CaptureConfirmedState();
        Changed?.Invoke();
    }

    public static string GetStatusLabel(AttendanceStatus status)
    {
        return status switch
        {
            AttendanceStatus.Present => "Присутствовал",
            AttendanceStatus.Late => "Опоздал",
            AttendanceStatus.Excused => "Уважительная причина",
            AttendanceStatus.Absent => "Отсутствовал",
            _ => "Не отмечен"
        };
    }

    public static string GetStatusShortLabel(AttendanceStatus status)
    {
        return status switch
        {
            AttendanceStatus.Present => "Есть",
            AttendanceStatus.Late => "Опоздание",
            AttendanceStatus.Excused => "Уваж.",
            AttendanceStatus.Absent => "Нет",
            _ => "Не отмечен"
        };
    }

    public static string? ValidateEntry(AttendanceEntryModel entry)
    {
        if (!Enum.IsDefined(entry.Status)) return "Неизвестный статус посещения.";
        if (entry.Status != AttendanceStatus.Late && entry.LateMinutes is not null)
            return "Минуты опоздания допустимы только для статуса опоздания.";
        if (entry.Comment?.Length > 2000) return "Комментарий не должен превышать 2000 символов.";
        if (entry.Status == AttendanceStatus.Late &&
            (entry.LateMinutes is null || entry.LateMinutes <= 0))
        {
            return "Для опоздания укажите количество минут.";
        }

        if (entry.Status == AttendanceStatus.Excused &&
            string.IsNullOrWhiteSpace(entry.Comment))
        {
            return "Для уважительной причины добавьте комментарий.";
        }

        return null;
    }

    private void SyncFromCatalog()
    {
        // Persisted lessons own their roster snapshots; catalog notifications must not rewrite them.
        if (persistentMode) return;

        var enrolledStudentIds = catalogService.Enrollments
            .Where(enrollment => enrollment.Role == LmsCourseRole.Student)
            .Select(enrollment => enrollment.UserId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Data.Students = catalogService.Users
            .Where(user => enrolledStudentIds.Contains(user.Id))
            .Select(user => new AttendanceStudentModel
            {
                Id = user.Id,
                FullName = user.FullName,
                RecordBookNumber = user.RecordBookNumber
            })
            .OrderBy(student => student.FullName)
            .ToList();

        Data.Lessons.RemoveAll(lesson => catalogService.GetCourse(lesson.CourseId) is null);

        foreach (var course in catalogService.Courses)
        {
            var courseLessons = Data.Lessons
                .Where(lesson => lesson.CourseId == course.Id)
                .OrderByDescending(lesson => lesson.StartsAt)
                .ToList();

            if (courseLessons.Count == 0)
            {
                Data.Lessons.Add(CreateLesson(
                    $"lesson-{course.Id}",
                    course.Id,
                    course.Code,
                    course.GroupName,
                    "Вводное занятие",
                    course.DefaultRoom,
                    DateTime.Today.AddDays(-1).AddHours(10),
                    ToAttendanceStudents(catalogService.GetStudentsForCourse(course.Id)),
                    []));
            }

            foreach (var lesson in Data.Lessons.Where(lesson => lesson.CourseId == course.Id))
            {
                lesson.CourseName = course.Code;
                lesson.GroupName = course.GroupName;
                lesson.Room = string.IsNullOrWhiteSpace(lesson.Room) ? course.DefaultRoom : lesson.Room;
                SyncLessonStudents(lesson, course.Id);
            }
        }

        if (SelectedLessonId is null || Data.Lessons.All(lesson => lesson.Id != SelectedLessonId))
        {
            SelectedLessonId = Data.Lessons.OrderByDescending(lesson => lesson.StartsAt).FirstOrDefault()?.Id;
        }

        Changed?.Invoke();
    }

    private void SyncLessonStudents(AttendanceLessonModel lesson, string courseId)
    {
        var studentIds = catalogService.GetStudentsForCourse(courseId)
            .Select(user => user.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        lesson.Entries.RemoveAll(entry => !studentIds.Contains(entry.StudentId));

        foreach (var studentId in studentIds)
        {
            if (lesson.Entries.All(entry => entry.StudentId != studentId))
            {
                lesson.Entries.Add(new AttendanceEntryModel
                {
                    StudentId = studentId,
                    Status = AttendanceStatus.Unmarked
                });
            }
        }

        if (!confirmedEntries.TryGetValue(lesson.Id, out var confirmed))
        {
            confirmedEntries[lesson.Id] = lesson.Entries.Select(entry => entry.Clone()).ToList();
            return;
        }

        confirmed.RemoveAll(entry => !studentIds.Contains(entry.StudentId));
        foreach (var entry in lesson.Entries)
        {
            if (confirmed.All(item => item.StudentId != entry.StudentId))
            {
                confirmed.Add(entry.Clone());
            }
        }
    }

    private void CaptureConfirmedState()
    {
        confirmedEntries.Clear();

        foreach (var lesson in Data.Lessons)
        {
            confirmedEntries[lesson.Id] = lesson.Entries.Select(entry => entry.Clone()).ToList();
        }
    }

    private static void EnsureLessonEditable(AttendanceLessonModel lesson)
    {
        if (lesson.IsCancelled) throw new InvalidOperationException("Занятие отменено.");
        if (lesson.IsCompleted) throw new InvalidOperationException("Журнал завершён. Сначала возобновите его.");
    }

    private void EnsureEntryEditable(AttendanceEntryModel entry)
    {
        var lesson = Data.Lessons.FirstOrDefault(item => item.Entries.Contains(entry))
            ?? throw new InvalidOperationException("Запись не найдена в журнале.");
        EnsureLessonEditable(lesson);
    }

    private bool IsEntryDirty(AttendanceLessonModel lesson, AttendanceEntryModel entry)
    {
        if (!confirmedEntries.TryGetValue(lesson.Id, out var entries))
        {
            return true;
        }

        var confirmed = entries.FirstOrDefault(item => item.StudentId == entry.StudentId);
        return confirmed is null ||
            confirmed.Status != entry.Status ||
            confirmed.LateMinutes != entry.LateMinutes ||
            !string.Equals(confirmed.Comment, entry.Comment, StringComparison.Ordinal);
    }

    private static AttendanceEntryModel CreateEntry(AttendanceLessonModel lesson, string studentId)
    {
        var entry = new AttendanceEntryModel
        {
            StudentId = studentId,
            Status = AttendanceStatus.Unmarked
        };

        lesson.Entries.Add(entry);
        return entry;
    }

    private static DateTime GetLastUpdatedAt(AttendanceLessonModel lesson)
    {
        var entryUpdate = lesson.Entries
            .Where(entry => entry.UpdatedAt is not null)
            .Select(entry => entry.UpdatedAt!.Value)
            .DefaultIfEmpty(lesson.StartsAt)
            .Max();

        return lesson.CompletedAt is not null && lesson.CompletedAt > entryUpdate
            ? lesson.CompletedAt.Value
            : entryUpdate;
    }

    private static AttendanceDataModel CreateSampleData(LmsCatalogService catalogService)
    {
        var students = ToAttendanceStudents(catalogService.Users
            .Where(user => !string.IsNullOrWhiteSpace(user.RecordBookNumber))
            .ToList());

        var stisStudents = ToAttendanceStudents(catalogService.GetStudentsForCourse("c-stis"));
        var ppStudents = ToAttendanceStudents(catalogService.GetStudentsForCourse("c-pp"));
        var bicaStudents = ToAttendanceStudents(catalogService.GetStudentsForCourse("c-bica"));

        var lessons = new List<AttendanceLessonModel>
        {
            CreateLesson(
                "l1",
                "c-stis",
                "СТИС",
                "Б23-504",
                "Компонентная модель Blazor",
                "Б-214",
                new DateTime(2026, 8, 21, 10, 15, 0),
                stisStudents,
                [
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Late, 12, "Проблемы с транспортом"),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Excused, null, "Медицинская справка"),
                    (AttendanceStatus.Absent, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Unmarked, null, null),
                    (AttendanceStatus.Present, null, null)
                ]),
            CreateLesson(
                "l2",
                "c-stis",
                "СТИС",
                "Б23-504",
                "Razor-компоненты и события",
                "Б-214",
                new DateTime(2026, 8, 14, 10, 15, 0),
                stisStudents,
                [
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Late, 7, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Absent, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Excused, null, "Участие в конференции")
                ]),
            CreateLesson(
                "l3",
                "c-pp",
                "Дискретная математика",
                "Б23-504",
                "Контрольная работа",
                "А-312",
                new DateTime(2026, 8, 7, 12, 10, 0),
                ppStudents,
                [
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Late, 5, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Absent, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Present, null, null)
                ]),
            CreateLesson(
                "l4",
                "c-bica",
                "BICA",
                "Б23-504",
                "Разбор архитектур",
                "Б-305",
                new DateTime(2026, 8, 20, 14, 35, 0),
                bicaStudents,
                [
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Unmarked, null, null),
                    (AttendanceStatus.Late, 10, "После пары в другом корпусе")
                ])
        };

        return new AttendanceDataModel
        {
            Students = students,
            Lessons = lessons
        };
    }

    private static AttendanceLessonModel CreateLesson(
        string id,
        string courseId,
        string courseName,
        string groupName,
        string topic,
        string room,
        DateTime startsAt,
        IReadOnlyList<AttendanceStudentModel> students,
        IReadOnlyList<(AttendanceStatus Status, int? LateMinutes, string? Comment)> values)
    {
        return new AttendanceLessonModel
        {
            Id = id,
            CourseId = courseId,
            CourseName = courseName,
            GroupName = groupName,
            Topic = topic,
            Room = room,
            StartsAt = startsAt,
            CompletedAt = values.Count > 0 && values.All(value => value.Status != AttendanceStatus.Unmarked)
                ? startsAt.AddHours(2)
                : null,
            Entries = students.Select((student, index) => new AttendanceEntryModel
            {
                StudentId = student.Id,
                Status = index < values.Count ? values[index].Status : AttendanceStatus.Unmarked,
                LateMinutes = index < values.Count ? values[index].LateMinutes : null,
                Comment = index < values.Count ? values[index].Comment : null,
                UpdatedAt = startsAt.AddHours(2)
            }).ToList()
        };
    }

    private static List<AttendanceStudentModel> ToAttendanceStudents(IEnumerable<LmsUserModel> users)
    {
        return users
            .Where(user => !string.IsNullOrWhiteSpace(user.RecordBookNumber))
            .Select(user => new AttendanceStudentModel
            {
                Id = user.Id,
                FullName = user.FullName,
                RecordBookNumber = user.RecordBookNumber
            })
            .OrderBy(student => student.FullName)
            .ToList();
    }
}
