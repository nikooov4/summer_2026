using System.Net.Http.Json;
using System.Text.Json;
using LMSDashboardBlazor.Models;
using LMSDashboardBlazor.Models.Attendance;
using LMSDashboardBlazor.Services.Admin;
using LMSDashboardBlazor.Services.Attendance;
using LMSDashboardBlazor.Services.Statements;

namespace LMSDashboardBlazor.Services;

public sealed class PersistentLmsService(AuthSessionService auth,
    LmsCatalogService catalog, AttendanceService attendance, StatementService statements)
{
    public bool IsConnected { get; private set; }
    public long StatementRevision { get; private set; }

    public async Task LoadAsync()
    {
        IsConnected = false;
        await auth.RefreshAsync();
        if (!await auth.IsAuthenticatedAsync() || auth.CurrentUser?.MustChangePassword == true) { Apply(new LmsSnapshot()); statements.SetStatement(new()); return; }
        var snapshot = await Send<LmsSnapshot>(HttpMethod.Get, "api/bootstrap", null);
        Apply(snapshot);
        StatementRevision = snapshot.StatementRevision;
        statements.SetStatement(snapshot.Statement ?? new StatementModel());
        IsConnected = true;
    }

    public async Task<CatalogCommandResult> ChangeCatalogAsync(CatalogCommand command)
    {
        var result = await Send<CatalogCommandResult>(HttpMethod.Post, "api/catalog/commands", command);
        Apply(result.Snapshot);
        return result;
    }

    private void Apply(LmsSnapshot snapshot)
    {
        catalog.Load(snapshot.Users, snapshot.Courses, snapshot.Enrollments);
        attendance.Load(snapshot.Attendance);
    }

    public async Task SaveJournalAsync()
    {
        var lesson = attendance.SelectedLesson ?? throw new InvalidOperationException("Занятие не выбрано.");
        var error = lesson.Entries.Select(AttendanceService.ValidateEntry).FirstOrDefault(error => error is not null);
        if (error is not null) throw new InvalidOperationException(error);
        var saved = await Send<AttendanceLessonModel>(HttpMethod.Put, $"api/attendance/lessons/{lesson.Id}/entries",
            new JournalSaveRequest(lesson.Revision, lesson.Entries.Select(entry => entry.Clone()).ToList()));
        attendance.AcceptSavedLesson(saved);
    }

    public async Task SetJournalStateAsync(string id, bool complete)
    {
        var lesson = attendance.Data.Lessons.Single(item => item.Id == id);
        if (attendance.HasUnsavedChanges(lesson)) throw new InvalidOperationException("Сначала сохраните изменения в журнале.");
        var saved = await Send<AttendanceLessonModel>(HttpMethod.Post, $"api/attendance/lessons/{id}/{(complete ? "complete" : "reopen")}", new JournalStateRequest(lesson.Revision));
        attendance.AcceptSavedLesson(saved);
    }

    public async Task SaveStatementAsync(StatementModel statement)
    {
        var saved = await Send<StatementSaveResult>(HttpMethod.Put, "api/statements/current", new StatementSaveRequest(statement, StatementRevision));
        StatementRevision = saved.Revision;
        statements.SetStatement(saved.Statement);
    }

    private async Task<T> Send<T>(HttpMethod method, string path, object? body)
    {
        using var response = await auth.SendAsync(method, path, body);
        await AuthSessionService.EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("Пустой ответ сервера LMS.");
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var text = await response.Content.ReadAsStringAsync();
        try
        {
            using var document = JsonDocument.Parse(text);
            if (document.RootElement.TryGetProperty("message", out var message)) throw new InvalidOperationException(message.GetString());
        }
        catch (JsonException) { }
        throw new InvalidOperationException($"Сервер БД вернул ошибку {(int)response.StatusCode}.");
    }
}
