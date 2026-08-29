using System.Text.Json;
using System.Text.Json.Serialization;
using Mephi.Cybernetics.Race.Concepts.Blazor.Models;

namespace Mephi.Cybernetics.Race.Concepts.Blazor.Services;

public sealed class AttendanceWorkspace
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _storagePath;
    private readonly Dictionary<string, List<AttendanceEntryModel>> _confirmedEntries = new();

    public AttendanceDataModel Data { get; private set; }
    public string? SelectedLessonId { get; private set; }
    public string? LastStorageError { get; private set; }

    public AttendanceLessonModel? SelectedLesson =>
        Data.Lessons.FirstOrDefault(lesson => lesson.Id == SelectedLessonId) ??
        Data.Lessons.OrderByDescending(lesson => lesson.StartsAt).FirstOrDefault();

    public AttendanceWorkspace(IWebHostEnvironment environment)
    {
        _storagePath = Path.Combine(environment.ContentRootPath, "App_Data", "attendance-data.json");
        Data = LoadOrCreate();
        SelectedLessonId = Data.Lessons.OrderByDescending(lesson => lesson.StartsAt).FirstOrDefault()?.Id;
        CaptureConfirmedState();
    }

    public void SelectLesson(string lessonId)
    {
        if (Data.Lessons.Any(lesson => lesson.Id == lessonId))
        {
            SelectedLessonId = lessonId;
        }
    }

    public IReadOnlyList<AttendanceRowModel> GetRows(
        AttendanceLessonModel lesson,
        string? searchText,
        AttendanceStatus? statusFilter)
    {
        var normalizedSearch = searchText?.Trim();
        var entries = lesson.Entries.ToDictionary(entry => entry.StudentId);

        return Data.Students
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
            .Where(lesson => lesson.StartsAt <= DateTime.Now)
            .Where(lesson => periodStart is null || lesson.StartsAt >= periodStart)
            .OrderByDescending(lesson => lesson.StartsAt)
            .Select(lesson => new AttendanceLessonMonitoringModel(
                lesson,
                GetSummary(lesson),
                GetLastUpdatedAt(lesson)))
            .ToList();
    }

    public AttendanceMonitoringSummaryModel GetMonitoringSummary(DateTime? periodStart)
    {
        var lessons = GetLessonMonitoring(periodStart);
        var completedLessons = lessons.Where(item => item.Lesson.IsCompleted).ToList();
        var completedEntries = completedLessons
            .SelectMany(item => item.Lesson.Entries)
            .ToList();

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

    public void UpdateStatus(AttendanceEntryModel entry, AttendanceStatus status)
    {
        entry.Status = status;

        if (status != AttendanceStatus.Late)
        {
            entry.LateMinutes = null;
        }
    }

    public void UpdateLateMinutes(AttendanceEntryModel entry, string? value)
    {
        entry.LateMinutes = int.TryParse(value, out var minutes) ? Math.Max(0, minutes) : null;
    }

    public void UpdateComment(AttendanceEntryModel entry, string? value)
    {
        entry.Comment = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public int MarkUnmarkedPresent(IEnumerable<AttendanceRowModel> rows)
    {
        var changed = 0;

        foreach (var row in rows.Where(row => row.Entry.Status == AttendanceStatus.Unmarked))
        {
            UpdateStatus(row.Entry, AttendanceStatus.Present);
            changed++;
        }

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
        var validationErrors = lesson.Entries
            .Select(entry => (Entry: entry, Error: ValidateEntry(entry)))
            .Where(item => item.Error is not null)
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

        Persist();
        _confirmedEntries[lesson.Id] = lesson.Entries.Select(entry => entry.Clone()).ToList();
    }

    public void RevertSelectedLesson()
    {
        var lesson = SelectedLesson ?? throw new InvalidOperationException("Занятие не выбрано.");
        if (!_confirmedEntries.TryGetValue(lesson.Id, out var confirmed))
        {
            return;
        }

        lesson.Entries = confirmed.Select(entry => entry.Clone()).ToList();
    }

    public void CompleteLesson(string lessonId)
    {
        var lesson = Data.Lessons.FirstOrDefault(item => item.Id == lessonId) ??
            throw new InvalidOperationException("Занятие не найдено.");

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
            throw new InvalidOperationException($"Нельзя завершить журнал: не отмечено студентов — {unmarked}.");
        }

        lesson.CompletedAt = DateTime.Now;
        Persist();
    }

    public void ReopenLesson(string lessonId)
    {
        var lesson = Data.Lessons.FirstOrDefault(item => item.Id == lessonId) ??
            throw new InvalidOperationException("Занятие не найдено.");

        lesson.CompletedAt = null;
        Persist();
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
        Data = CreateSampleData();
        SelectedLessonId = Data.Lessons.OrderByDescending(lesson => lesson.StartsAt).First().Id;
        Persist();
        CaptureConfirmedState();
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

    private AttendanceDataModel LoadOrCreate()
    {
        try
        {
            if (File.Exists(_storagePath))
            {
                var json = File.ReadAllText(_storagePath);
                var data = JsonSerializer.Deserialize<AttendanceDataModel>(json, JsonOptions);
                if (data is not null && data.Students.Count > 0 && data.Lessons.Count > 0)
                {
                    EnsureEntries(data);
                    return data;
                }
            }
        }
        catch (Exception exception) when (exception is IOException or JsonException)
        {
            LastStorageError = $"Не удалось прочитать локальные данные: {exception.Message}";
        }

        var sample = CreateSampleData();
        Persist(sample);
        return sample;
    }

    private void Persist()
    {
        Persist(Data);
    }

    private void Persist(AttendanceDataModel data)
    {
        try
        {
            var directory = Path.GetDirectoryName(_storagePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_storagePath, JsonSerializer.Serialize(data, JsonOptions));
            LastStorageError = null;
        }
        catch (IOException exception)
        {
            LastStorageError = $"Не удалось сохранить локальные данные: {exception.Message}";
            throw new InvalidOperationException(LastStorageError, exception);
        }
    }

    private void CaptureConfirmedState()
    {
        _confirmedEntries.Clear();

        foreach (var lesson in Data.Lessons)
        {
            _confirmedEntries[lesson.Id] = lesson.Entries.Select(entry => entry.Clone()).ToList();
        }
    }

    private bool IsEntryDirty(AttendanceLessonModel lesson, AttendanceEntryModel entry)
    {
        if (!_confirmedEntries.TryGetValue(lesson.Id, out var confirmedEntries))
        {
            return true;
        }

        var confirmed = confirmedEntries.FirstOrDefault(item => item.StudentId == entry.StudentId);
        return confirmed is null ||
            confirmed.Status != entry.Status ||
            confirmed.LateMinutes != entry.LateMinutes ||
            !string.Equals(confirmed.Comment, entry.Comment, StringComparison.Ordinal);
    }

    private static string? ValidateEntry(AttendanceEntryModel entry)
    {
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

    private static void EnsureEntries(AttendanceDataModel data)
    {
        foreach (var lesson in data.Lessons)
        {
            foreach (var student in data.Students)
            {
                if (lesson.Entries.All(entry => entry.StudentId != student.Id))
                {
                    lesson.Entries.Add(new AttendanceEntryModel
                    {
                        StudentId = student.Id,
                        Status = AttendanceStatus.Unmarked
                    });
                }
            }

            if (lesson.CompletedAt is null &&
                lesson.StartsAt < DateTime.Now &&
                lesson.Entries.Count > 0 &&
                lesson.Entries.All(entry => entry.Status != AttendanceStatus.Unmarked))
            {
                lesson.CompletedAt = lesson.Entries
                    .Max(entry => entry.UpdatedAt) ?? lesson.StartsAt.AddHours(2);
            }
        }
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

    private static AttendanceDataModel CreateSampleData()
    {
        var students = new List<AttendanceStudentModel>
        {
            new() { Id = "s1", FullName = "Иванов Иван Ильич", RecordBookNumber = "Б23-504-01" },
            new() { Id = "s2", FullName = "Петрова Анна Сергеевна", RecordBookNumber = "Б23-504-02" },
            new() { Id = "s3", FullName = "Сидоров Павел Олегович", RecordBookNumber = "Б23-504-03" },
            new() { Id = "s4", FullName = "Кузнецова Мария Андреевна", RecordBookNumber = "Б23-504-04" },
            new() { Id = "s5", FullName = "Смирнов Дмитрий Игоревич", RecordBookNumber = "Б23-504-05" },
            new() { Id = "s6", FullName = "Орлова Елена Максимовна", RecordBookNumber = "Б23-504-06" },
            new() { Id = "s7", FullName = "Волков Артем Николаевич", RecordBookNumber = "Б23-504-07" },
            new() { Id = "s8", FullName = "Морозова Софья Романовна", RecordBookNumber = "Б23-504-08" }
        };

        var lessons = new List<AttendanceLessonModel>
        {
            CreateLesson(
                "l1",
                "Программная инженерия",
                "Б23-504",
                "Компонентная модель Blazor",
                "Б-214",
                new DateTime(2026, 6, 18, 10, 15, 0),
                students,
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
                "Программная инженерия",
                "Б23-504",
                "Razor-компоненты и события",
                "Б-214",
                new DateTime(2026, 6, 11, 10, 15, 0),
                students,
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
                "Программная инженерия",
                "Б23-504",
                "Введение в ASP.NET Core",
                "Б-214",
                new DateTime(2026, 6, 4, 10, 15, 0),
                students,
                [
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Late, 5, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Absent, null, null),
                    (AttendanceStatus.Present, null, null),
                    (AttendanceStatus.Present, null, null)
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
            CourseName = courseName,
            GroupName = groupName,
            Topic = topic,
            Room = room,
            StartsAt = startsAt,
            CompletedAt = values.All(value => value.Status != AttendanceStatus.Unmarked)
                ? startsAt.AddHours(2)
                : null,
            Entries = students.Select((student, index) => new AttendanceEntryModel
            {
                StudentId = student.Id,
                Status = values[index].Status,
                LateMinutes = values[index].LateMinutes,
                Comment = values[index].Comment,
                UpdatedAt = startsAt.AddHours(2)
            }).ToList()
        };
    }
}
