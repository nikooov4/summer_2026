using System.Security.Claims;
using LMSDashboardBlazor.Models;
using LMSDashboardBlazor.Models.Attendance;
using Npgsql;

namespace LmsApi;

public sealed class AttendanceOperations(LmsRepository repository)
{
    public async Task<AttendanceLessonModel> SaveLesson(ClaimsPrincipal actor, string? lessonId, LessonEditRequest request)
    {
        var result = await repository.Mutate(async (state, connection, transaction) =>
        {
            var user = ApiOperations.Actor(state, actor);
            AttendanceLessonModel? lesson = null;
            if (lessonId is not null)
            {
                lesson = ApiOperations.RequireLesson(state, user, LessonId(lessonId), request.Revision);
                RequireOpen(lesson);
            }
            else if (request.Revision != 0)
                throw new ApiFailure(409, "Новое занятие должно иметь нулевую ревизию.");

            if (!Guid.TryParse(request.CourseId, out var courseId)) throw new ApiFailure(400, "Не указан корректный курс.");
            var course = state.Courses.FirstOrDefault(item => item.Id == courseId.ToString())
                ?? throw new ApiFailure(404, "Курс не найден или архивирован.");
            if (!ApiOperations.Editor(state, user, course.Id)) throw new ApiFailure(403, "Нет прав на изменение занятий этого курса.");
            if (lesson is not null && lesson.CourseId != course.Id)
                throw new ApiFailure(409, "Перенос занятия в другой курс запрещён. Создайте новое занятие.");
            var topic = request.Topic?.Trim() ?? "";
            var room = request.Room?.Trim() ?? "";
            if (topic.Length is < 1 or > 300 || room.Length is < 1 or > 100 || request.StartsAt == default || request.StartsAt == DateTime.MaxValue)
                throw new ApiFailure(400, "Проверьте тему, аудиторию и дату занятия.");
            var startsAt = DateTime.SpecifyKind(request.StartsAt, DateTimeKind.Unspecified);
            var id = lesson is null ? Guid.NewGuid() : Guid.Parse(lesson.Id);
            if (lesson is null)
            {
                await LmsRepository.Execute(connection, transaction, """
                    INSERT INTO "Attendance"."Lessons" ("ID","CourseID","Year","Topic","Room","StartsAt")
                    SELECT @id,c."CourseID",c."ActiveYear",@topic,@room,@starts
                    FROM "Lms"."CourseSettings" c WHERE c."CourseID"=@course AND NOT c."IsArchived"
                    """, ("id", id), ("course", courseId), ("topic", topic), ("room", room), ("starts", startsAt));
                await repository.FreezeRoster(connection, transaction, id, Guid.Parse(user.Id));
            }
            else
            {
                // The visit-identity FK cascades StartsAt to VisitDate without replacing visits or their audit history.
                await LmsRepository.Execute(connection, transaction, """
                    UPDATE "Attendance"."Lessons" SET "Topic"=@topic,"Room"=@room,"StartsAt"=@starts,"Revision"="Revision"+1
                    WHERE "ID"=@id
                    """, ("topic", topic), ("room", room), ("starts", startsAt), ("id", id));
            }
            return id.ToString();
        });
        return result.State.Attendance.Lessons.Single(lesson => lesson.Id == result.Result);
    }

    public async Task<AttendanceLessonModel> CancelLesson(ClaimsPrincipal actor, string id, long revision)
    {
        var result = await repository.Mutate(async (state, connection, transaction) =>
        {
            var user = ApiOperations.Actor(state, actor);
            var lesson = ApiOperations.RequireLesson(state, user, LessonId(id), revision);
            RequireOpen(lesson);
            await LmsRepository.Execute(connection, transaction, """
                UPDATE "Attendance"."Lessons" SET "IsCancelled"=true,"Revision"="Revision"+1 WHERE "ID"=@id
                """, ("id", Guid.Parse(lesson.Id)));
            return lesson.Id;
        });
        return result.State.Attendance.Lessons.Single(lesson => lesson.Id == result.Result);
    }

    public async Task<IReadOnlyList<AttendanceAuditItem>> History(ClaimsPrincipal actor, string id)
    {
        var result = await repository.Mutate(async (state, connection, transaction) =>
        {
            var user = ApiOperations.Actor(state, actor);
            var lessonId = LessonId(id);
            var lesson = state.Attendance.Lessons.FirstOrDefault(item => item.Id == lessonId)
                ?? throw new ApiFailure(404, "Занятие не найдено.");
            var staff = ApiOperations.StaffReader(state, user, lesson.CourseId);
            if (!staff && !lesson.Entries.Any(entry => entry.StudentId == user.Id))
                throw new ApiFailure(403, "Нет доступа к истории этого занятия.");
            var history = new List<AttendanceAuditItem>();
            await using var command = new NpgsqlCommand("""
                SELECT h."ID",v."ID",r."StudentPersonID",r."FullName",r."RecordBookNumber",
                       h."ActorPersonID",COALESCE(u."DisplayName",NULLIF(concat_ws(' ',p."FirstName",p."LastName",p."MiddleName"),''),h."ActorPersonID"::text),
                       h."ChangedAt",h."OldStatus",h."NewStatus",h."OldLateMinutes",h."NewLateMinutes",h."OldComment",h."NewComment",h."DataSourceName",
                       v."Status",v."LateMinutes",v."Comment",v."UpdatedAt"
                FROM "Attendance"."VisitHistory" h
                JOIN "CourseLabs"."StudentVisits" v ON v."ID"=h."VisitID"
                JOIN "Attendance"."LessonRosters" r ON r."LessonID"=v."LessonID" AND r."StudentPersonID"=v."StudentPersonID"
                LEFT JOIN "Lms"."UserAccounts" u ON u."PersonID"=h."ActorPersonID"
                LEFT JOIN "Person"."Persons" p ON p."ID"=h."ActorPersonID"
                WHERE v."LessonID"=@lesson AND (@staff OR v."StudentPersonID"=@actor)
                ORDER BY h."ChangedAt" DESC,h."ID" DESC
                """, connection, transaction);
            command.Parameters.AddWithValue("lesson", Guid.Parse(lessonId));
            command.Parameters.AddWithValue("staff", staff);
            command.Parameters.AddWithValue("actor", Guid.Parse(user.Id));
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                history.Add(new AttendanceAuditItem(
                    Id: reader.GetInt64(0), LessonId: lessonId, VisitId: reader.GetGuid(1).ToString(),
                    StudentId: reader.GetGuid(2).ToString(), StudentName: reader.GetString(3), RecordBookNumber: reader.GetString(4),
                    ActorId: reader.GetGuid(5).ToString(), ActorName: reader.GetString(6), ChangedAt: reader.GetDateTime(7),
                    OldStatus: reader.IsDBNull(8) ? null : Enum.Parse<AttendanceStatus>(reader.GetString(8)),
                    NewStatus: Enum.Parse<AttendanceStatus>(reader.GetString(9)),
                    OldLateMinutes: reader.IsDBNull(10) ? null : reader.GetInt32(10), NewLateMinutes: reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    OldComment: reader.IsDBNull(12) ? null : reader.GetString(12), NewComment: reader.IsDBNull(13) ? null : reader.GetString(13),
                    DataSourceName: reader.IsDBNull(14) ? null : reader.GetString(14), CurrentStatus: Enum.Parse<AttendanceStatus>(reader.GetString(15)),
                    CurrentLateMinutes: reader.IsDBNull(16) ? null : reader.GetInt32(16), CurrentComment: reader.IsDBNull(17) ? null : reader.GetString(17),
                    CurrentUpdatedAt: reader.IsDBNull(18) ? null : reader.GetDateTime(18)));
            return (IReadOnlyList<AttendanceAuditItem>)history;
        });
        return result.Result;
    }

    private static string LessonId(string id) => Guid.TryParse(id, out var value)
        ? value.ToString() : throw new ApiFailure(404, "Занятие не найдено.");

    private static void RequireOpen(AttendanceLessonModel lesson)
    {
        if (lesson.IsCancelled) throw new ApiFailure(409, "Занятие отменено.");
        if (lesson.IsCompleted) throw new ApiFailure(409, "Журнал завершён. Сначала возобновите его.");
    }
}
