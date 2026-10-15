using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LMSDashboardBlazor.Models;
using LMSDashboardBlazor.Models.Attendance;
using LMSDashboardBlazor.Services.Admin;
using LMSDashboardBlazor.Services.Attendance;
using LMSDashboardBlazor.Services.Statements;
using Npgsql;

namespace LmsApi;

public sealed class LmsRepository(NpgsqlDataSource source)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public static Guid Id(string value) => Guid.TryParse(value, out var id) ? id : new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(value)).AsSpan(0, 16));
    private static DateTime Wall(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    private static object Db(object? value) => value ?? DBNull.Value;

    internal static async Task Execute(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql, params (string Key, object? Value)[] parameters)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        foreach (var parameter in parameters) command.Parameters.AddWithValue(parameter.Key, Db(parameter.Value));
        await command.ExecuteNonQueryAsync();
    }

    public async Task Seed()
    {
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await Execute(connection, transaction, "SELECT pg_advisory_xact_lock(2026100802)");
        await using var count = new NpgsqlCommand("SELECT count(*) FROM \"Lms\".\"UserAccounts\"", connection, transaction);
        if ((long)(await count.ExecuteScalarAsync())! > 0) return;
        var catalog = new LmsCatalogService();
        var attendance = new AttendanceService(catalog);
        foreach (var user in catalog.Users) user.Id = Id(user.Id).ToString();
        foreach (var course in catalog.Courses) course.Id = Id(course.Id).ToString();
        foreach (var enrollment in catalog.Enrollments)
        {
            enrollment.Id = Id(enrollment.Id).ToString(); enrollment.UserId = Id(enrollment.UserId).ToString(); enrollment.CourseId = Id(enrollment.CourseId).ToString();
        }
        foreach (var student in attendance.Data.Students) student.Id = Id(student.Id).ToString();
        foreach (var lesson in attendance.Data.Lessons)
        {
            lesson.Id = Id(lesson.Id).ToString(); lesson.CourseId = Id(lesson.CourseId).ToString();
            foreach (var entry in lesson.Entries) entry.StudentId = Id(entry.StudentId).ToString();
        }
        var snapshot = new LmsSnapshot { Users = catalog.Users.ToList(), Courses = catalog.Courses.ToList(), Enrollments = catalog.Enrollments.ToList(), Attendance = attendance.Data };
        await WriteCatalog(connection, transaction, snapshot);
        await WriteNewLessons(connection, transaction, snapshot, Id("u-admin"));
        // Initial sample marks are inserted once, not regenerated at every launch.
        foreach (var lesson in snapshot.Attendance.Lessons)
            await WriteJournal(connection, transaction, lesson, Id("u-admin"), audit: false);
        await transaction.CommitAsync();
    }

    public async Task<LmsSnapshot> Read(NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
    {
        var snapshot = new LmsSnapshot();
        await using (var command = new NpgsqlCommand("""
            SELECT "PersonID","Username","DisplayName","RecordBookNumber","IsAdmin"
            FROM "Lms"."UserAccounts" WHERE NOT "IsArchived" ORDER BY "DisplayName"
            """, connection, transaction))
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync()) snapshot.Users.Add(new() { Id = reader.GetGuid(0).ToString(), Username = reader.GetString(1), FullName = reader.GetString(2), RecordBookNumber = reader.GetString(3), IsAdmin = reader.GetBoolean(4) });
        await using (var command = new NpgsqlCommand("""
            SELECT c."ID",s."Code",c."Name",s."GroupName",s."DefaultRoom"
            FROM "Course"."Courses" c JOIN "Lms"."CourseSettings" s ON s."CourseID"=c."ID" WHERE NOT s."IsArchived" ORDER BY s."Code"
            """, connection, transaction))
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync()) snapshot.Courses.Add(new() { Id = reader.GetGuid(0).ToString(), Code = reader.GetString(1), Name = reader.GetString(2), GroupName = reader.GetString(3), DefaultRoom = reader.GetString(4) });
        await using (var command = new NpgsqlCommand("""
            SELECT e."ID",e."PersonID",e."CourseID",e."Role" FROM "Lms"."Enrollments" e
            JOIN "Lms"."UserAccounts" u ON u."PersonID"=e."PersonID" JOIN "Lms"."CourseSettings" c ON c."CourseID"=e."CourseID"
            WHERE e."EndDate" IS NULL AND NOT u."IsArchived" AND NOT c."IsArchived" AND e."Year"=c."ActiveYear"
            """, connection, transaction))
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync()) snapshot.Enrollments.Add(new() { Id = reader.GetGuid(0).ToString(), UserId = reader.GetGuid(1).ToString(), CourseId = reader.GetGuid(2).ToString(), Role = Enum.Parse<LmsCourseRole>(reader.GetString(3)) });
        var courseById = snapshot.Courses.ToDictionary(course => course.Id);
        await using (var command = new NpgsqlCommand("""
            SELECT l."ID",l."CourseID",l."Topic",l."Room",l."StartsAt",l."CompletedAt",l."Revision",l."IsCancelled"
            FROM "Attendance"."Lessons" l JOIN "Lms"."CourseSettings" c ON c."CourseID"=l."CourseID"
            WHERE NOT c."IsArchived" AND l."Year"=c."ActiveYear" ORDER BY l."StartsAt" DESC
            """, connection, transaction))
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync())
            {
                var course = courseById[reader.GetGuid(1).ToString()];
                snapshot.Attendance.Lessons.Add(new() { Id = reader.GetGuid(0).ToString(), CourseId = course.Id, CourseName = course.Code, GroupName = course.GroupName, Topic = reader.GetString(2), Room = reader.GetString(3), StartsAt = reader.GetDateTime(4), CompletedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5), Revision = reader.GetInt64(6), IsCancelled = reader.GetBoolean(7) });
            }
        var lessonById = snapshot.Attendance.Lessons.ToDictionary(lesson => lesson.Id);
        await using (var command = new NpgsqlCommand("""
            SELECT r."LessonID",r."StudentPersonID",COALESCE(v."Status",'Unmarked'),v."LateMinutes",v."Comment",v."UpdatedAt",v."DataSourceName",
                   r."FullName",r."RecordBookNumber"
            FROM "Attendance"."LessonRosters" r
            LEFT JOIN "CourseLabs"."StudentVisits" v ON v."LessonID"=r."LessonID" AND v."StudentPersonID"=r."StudentPersonID"
            JOIN "Attendance"."Lessons" l ON l."ID"=r."LessonID"
            JOIN "Lms"."CourseSettings" c ON c."CourseID"=l."CourseID"
            WHERE NOT c."IsArchived" AND l."Year"=c."ActiveYear"
            ORDER BY l."StartsAt" DESC, l."ID", r."StudentPersonID"
            """, connection, transaction))
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync())
                if (lessonById.TryGetValue(reader.GetGuid(0).ToString(), out var lesson))
                {
                    lesson.Entries.Add(new() { StudentId = reader.GetGuid(1).ToString(), Status = Enum.Parse<AttendanceStatus>(reader.GetString(2)), LateMinutes = reader.IsDBNull(3) ? null : reader.GetInt32(3), Comment = reader.IsDBNull(4) ? null : reader.GetString(4), UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5), DataSourceName = reader.IsDBNull(6) ? null : reader.GetString(6) });
                    lesson.Roster.Add(new() { Id = reader.GetGuid(1).ToString(), FullName = reader.GetString(7), RecordBookNumber = reader.GetString(8) });
                }
        snapshot.Attendance.Students = snapshot.Attendance.Lessons.SelectMany(lesson => lesson.Roster).DistinctBy(student => student.Id).ToList();
        return snapshot;
    }

    internal async Task WriteCatalog(NpgsqlConnection connection, NpgsqlTransaction transaction, LmsSnapshot snapshot)
    {
        foreach (var user in snapshot.Users)
        {
            var words = user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            await Execute(connection, transaction, """
                INSERT INTO "Person"."Persons" ("ID","FirstName","LastName","MiddleName","RegisterDate") VALUES (@id,@first,@last,@middle,now() AT TIME ZONE 'UTC')
                ON CONFLICT ("ID") DO UPDATE SET "FirstName"=EXCLUDED."FirstName","LastName"=EXCLUDED."LastName","MiddleName"=EXCLUDED."MiddleName";
                INSERT INTO "Lms"."UserAccounts" ("PersonID","Username","DisplayName","RecordBookNumber","IsAdmin") VALUES (@id,@login,@name,@record,@admin)
                ON CONFLICT ("PersonID") DO UPDATE SET "Username"=EXCLUDED."Username","DisplayName"=EXCLUDED."DisplayName","RecordBookNumber"=EXCLUDED."RecordBookNumber","IsAdmin"=EXCLUDED."IsAdmin"
                """, ("id", Id(user.Id)), ("first", words.ElementAtOrDefault(0) ?? ""), ("last", words.ElementAtOrDefault(1) ?? ""), ("middle", string.Join(' ', words.Skip(2))), ("login", user.Username), ("name", user.FullName), ("record", user.RecordBookNumber), ("admin", user.IsAdmin));
        }
        await Execute(connection, transaction, "UPDATE \"Lms\".\"UserAccounts\" SET \"IsArchived\"=true WHERE NOT (\"PersonID\"=ANY(@ids))", ("ids", snapshot.Users.Select(user => Id(user.Id)).ToArray()));
        foreach (var course in snapshot.Courses)
        {
            var courseId = Id(course.Id);
            await Execute(connection, transaction, """
                INSERT INTO "Education"."EducationalGroups" ("GroupName","GroupCaption","EdProgramID","AdmissionYear","StartDate")
                VALUES (@group,@group,'00000000-0000-0000-0000-000000000023',2023,'2023-09-01') ON CONFLICT DO NOTHING;
                INSERT INTO "Lms"."GroupNames" VALUES (@group,'2023-09-01') ON CONFLICT DO NOTHING;
                INSERT INTO "Content"."ContentScopes" ("ID") VALUES (@scope),(@instanceScope) ON CONFLICT DO NOTHING;
                INSERT INTO "Course"."Courses" ("ID","Name","ContentScopeID") VALUES (@id,@name,@scope)
                ON CONFLICT ("ID") DO UPDATE SET "Name"=EXCLUDED."Name";
                INSERT INTO "Course"."CourseInstances" ("CourseID","Year","ContentScopeID") VALUES (@id,2026,@instanceScope) ON CONFLICT DO NOTHING;
                INSERT INTO "Lms"."CourseSettings" ("CourseID","Code","GroupName","DefaultRoom","ActiveYear") VALUES (@id,@code,@group,@room,2026)
                ON CONFLICT ("CourseID") DO UPDATE SET "Code"=EXCLUDED."Code","GroupName"=EXCLUDED."GroupName","DefaultRoom"=EXCLUDED."DefaultRoom"
                """, ("id", courseId), ("scope", Id("scope:" + courseId)), ("instanceScope", Id("scope:2026:" + courseId)), ("name", course.Name), ("code", course.Code), ("group", course.GroupName), ("room", course.DefaultRoom));
        }
        await Execute(connection, transaction, "UPDATE \"Lms\".\"CourseSettings\" SET \"IsArchived\"=true WHERE NOT (\"CourseID\"=ANY(@ids))", ("ids", snapshot.Courses.Select(course => Id(course.Id)).ToArray()));
        foreach (var enrollment in snapshot.Enrollments)
            await Execute(connection, transaction, """
                INSERT INTO "Lms"."Enrollments" ("ID","PersonID","CourseID","Year","Role")
                SELECT @id,@person,@course,c."ActiveYear",@role FROM "Lms"."CourseSettings" c WHERE c."CourseID"=@course AND NOT c."IsArchived"
                ON CONFLICT ("ID") DO NOTHING
                """, ("id", Id(enrollment.Id)), ("person", Id(enrollment.UserId)), ("course", Id(enrollment.CourseId)), ("role", enrollment.Role.ToString()));
        await Execute(connection, transaction, """
            UPDATE "Lms"."Enrollments" SET "EndDate"=GREATEST(now() AT TIME ZONE 'UTC',"StartDate" + interval '1 microsecond')
            WHERE "EndDate" IS NULL AND NOT ("ID"=ANY(@ids));
            INSERT INTO "Course"."CourseInstanceStudents" ("CourseID","Year","PersonID")
            SELECT DISTINCT "CourseID","Year","PersonID" FROM "Lms"."Enrollments" WHERE "Role"='Student' AND "EndDate" IS NULL
            ON CONFLICT ("CourseID","Year","PersonID") DO UPDATE SET "EndDate"=NULL;
            INSERT INTO "Course"."CourseInstanceTeachers" ("CourseID","Year","PersonID")
            SELECT DISTINCT "CourseID","Year","PersonID" FROM "Lms"."Enrollments" WHERE "Role" IN ('Teacher','Assistant') AND "EndDate" IS NULL
            ON CONFLICT ("CourseID","Year","PersonID") DO UPDATE SET "EndDate"=NULL;
            UPDATE "Course"."CourseInstanceStudents" t SET "EndDate"=GREATEST(now() AT TIME ZONE 'UTC',t."StartDate"+interval '1 microsecond')
            WHERE t."EndDate" IS NULL AND NOT EXISTS (SELECT 1 FROM "Lms"."Enrollments" e WHERE e."CourseID"=t."CourseID" AND e."Year"=t."Year" AND e."PersonID"=t."PersonID" AND e."Role"='Student' AND e."EndDate" IS NULL);
            UPDATE "Course"."CourseInstanceTeachers" t SET "EndDate"=GREATEST(now() AT TIME ZONE 'UTC',t."StartDate"+interval '1 microsecond')
            WHERE t."EndDate" IS NULL AND NOT EXISTS (SELECT 1 FROM "Lms"."Enrollments" e WHERE e."CourseID"=t."CourseID" AND e."Year"=t."Year" AND e."PersonID"=t."PersonID" AND e."Role" IN ('Teacher','Assistant') AND e."EndDate" IS NULL);
            INSERT INTO "Course"."CourseCurators" ("CourseID","PersonID","StartDate")
            SELECT DISTINCT "CourseID","PersonID",now() AT TIME ZONE 'UTC' FROM "Lms"."Enrollments" WHERE "Role"='Curator' AND "EndDate" IS NULL
            ON CONFLICT ("CourseID","PersonID") WHERE "EndDate" IS NULL DO NOTHING;
            UPDATE "Course"."CourseCurators" t SET "EndDate"=GREATEST(now() AT TIME ZONE 'UTC',t."StartDate"+interval '1 microsecond')
            WHERE t."EndDate" IS NULL AND NOT EXISTS (SELECT 1 FROM "Lms"."Enrollments" e WHERE e."CourseID"=t."CourseID" AND e."PersonID"=t."PersonID" AND e."Role"='Curator' AND e."EndDate" IS NULL);
            INSERT INTO "Organization"."GroupStudents" ("PersonID","EdGroupID","EdGroupStartDate","StartDate")
            SELECT DISTINCT e."PersonID",c."GroupName",g."GroupStartDate",now() AT TIME ZONE 'UTC' FROM "Lms"."Enrollments" e
            JOIN "Lms"."CourseSettings" c ON c."CourseID"=e."CourseID" JOIN "Lms"."GroupNames" g ON g."Name"=c."GroupName"
            WHERE e."Role"='Student' AND e."EndDate" IS NULL AND NOT EXISTS
            (SELECT 1 FROM "Organization"."GroupStudents" s WHERE s."PersonID"=e."PersonID" AND s."EdGroupID"=c."GroupName" AND s."EndDate" IS NULL);
            UPDATE "Organization"."GroupStudents" s SET "EndDate"=GREATEST(now() AT TIME ZONE 'UTC',s."StartDate"+interval '1 microsecond')
            WHERE s."EndDate" IS NULL AND NOT EXISTS (SELECT 1 FROM "Lms"."Enrollments" e JOIN "Lms"."CourseSettings" c ON c."CourseID"=e."CourseID" WHERE e."PersonID"=s."PersonID" AND c."GroupName"=s."EdGroupID" AND e."Role"='Student' AND e."EndDate" IS NULL)
            """, ("ids", snapshot.Enrollments.Select(enrollment => Id(enrollment.Id)).ToArray()));
    }

    internal async Task WriteNewLessons(NpgsqlConnection connection, NpgsqlTransaction transaction, LmsSnapshot snapshot, Guid actor)
    {
        foreach (var lesson in snapshot.Attendance.Lessons)
        {
            await Execute(connection, transaction, """
                INSERT INTO "Attendance"."Lessons" ("ID","CourseID","Year","Topic","Room","StartsAt","CompletedAt","Revision")
                SELECT @id,@course,c."ActiveYear",@topic,@room,@starts,@completed,@revision
                FROM "Lms"."CourseSettings" c WHERE c."CourseID"=@course AND NOT c."IsArchived"
                """, ("id", Id(lesson.Id)), ("course", Id(lesson.CourseId)), ("topic", lesson.Topic), ("room", lesson.Room), ("starts", Wall(lesson.StartsAt)), ("completed", lesson.CompletedAt is null ? null : Wall(lesson.CompletedAt.Value)), ("revision", lesson.Revision));
            await FreezeRoster(connection, transaction, Id(lesson.Id), actor);
        }
    }

    internal async Task FreezeRoster(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid lessonId, Guid actor)
    {
        // Called once, in the same locked transaction that inserts the lesson (including initial seeds).
        await Execute(connection, transaction, """
            INSERT INTO "Attendance"."LessonRosters" ("LessonID","StudentPersonID","FullName","RecordBookNumber")
            SELECT DISTINCT l."ID",u."PersonID",u."DisplayName",u."RecordBookNumber"
            FROM "Attendance"."Lessons" l
            JOIN "Lms"."Enrollments" e ON e."CourseID"=l."CourseID" AND e."Year"=l."Year"
            JOIN "Lms"."UserAccounts" u ON u."PersonID"=e."PersonID"
            WHERE l."ID"=@lesson AND e."Role"='Student' AND e."EndDate" IS NULL AND NOT u."IsArchived";
            INSERT INTO "CourseLabs"."StudentVisits" ("CourseID","Year","StudentPersonID","VisitDate","TeacherPersonID","LessonID","Status")
            SELECT l."CourseID",l."Year",r."StudentPersonID",l."StartsAt",@actor,l."ID",'Unmarked'
            FROM "Attendance"."Lessons" l JOIN "Attendance"."LessonRosters" r ON r."LessonID"=l."ID"
            WHERE l."ID"=@lesson
            """, ("lesson", lessonId), ("actor", actor));
    }

    internal async Task WriteJournal(NpgsqlConnection connection, NpgsqlTransaction transaction, AttendanceLessonModel lesson, Guid actor, bool audit = true)
    {
        foreach (var entry in lesson.Entries)
        {
            var sourceName = entry.DataSourceName == "attendance-ocr-service" ? entry.DataSourceName : "lms-web";
            if (audit)
                await Execute(connection, transaction, """
                    INSERT INTO "Attendance"."VisitHistory" ("VisitID","ActorPersonID","OldStatus","NewStatus","OldLateMinutes","NewLateMinutes","OldComment","NewComment","DataSourceName")
                    SELECT "ID",@actor,"Status",@status,"LateMinutes",@late,"Comment",@comment,@source FROM "CourseLabs"."StudentVisits"
                    WHERE "LessonID"=@lesson AND "StudentPersonID"=@student AND ("Status", "LateMinutes", "Comment") IS DISTINCT FROM (@status::text,@late::integer,@comment::text)
                    """, ("actor", actor), ("status", entry.Status.ToString()), ("late", entry.LateMinutes), ("comment", entry.Comment), ("source", sourceName), ("lesson", Id(lesson.Id)), ("student", Id(entry.StudentId)));
            await Execute(connection, transaction, """
                UPDATE "CourseLabs"."StudentVisits" SET "Status"=@status,"LateMinutes"=@late,"Comment"=@comment,"TeacherPersonID"=@actor,
                "UpdatedAt"=now() AT TIME ZONE 'UTC',"DataSourceName"=@source WHERE "LessonID"=@lesson AND "StudentPersonID"=@student
                AND (("Status","LateMinutes","Comment") IS DISTINCT FROM (@status::text,@late::integer,@comment::text) OR "UpdatedAt" IS NULL)
                """, ("status", entry.Status.ToString()), ("late", entry.LateMinutes), ("comment", entry.Comment), ("actor", actor), ("source", sourceName), ("lesson", Id(lesson.Id)), ("student", Id(entry.StudentId)));
        }
    }

    public async Task<(LmsSnapshot State, T Result)> Mutate<T>(Func<LmsSnapshot, NpgsqlConnection, NpgsqlTransaction, Task<T>> action)
    {
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await Execute(connection, transaction, "SELECT pg_advisory_xact_lock(2026100802)");
        var state = await Read(connection, transaction);
        var result = await action(state, connection, transaction);
        var updated = await Read(connection, transaction);
        await transaction.CommitAsync();
        return (updated, result);
    }

    public async Task<LmsSnapshot> Get()
    {
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        var snapshot = await Read(connection, transaction);
        await transaction.CommitAsync();
        return snapshot;
    }

    public async Task<StatementSaveResult?> GetStatement(Guid actor)
    {
        await using var connection = await source.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT \"Payload\"::text,\"Revision\" FROM \"Lms\".\"StatementDrafts\" WHERE \"PersonID\"=@id", connection);
        command.Parameters.AddWithValue("id", actor);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? new(JsonSerializer.Deserialize<StatementModel>(reader.GetString(0), Json)!, reader.GetInt64(1)) : null;
    }

    public async Task<long> WriteStatement(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid actor, StatementSaveRequest request)
    {
        await using var command = new NpgsqlCommand("""
            INSERT INTO "Lms"."StatementDrafts" ("PersonID","Payload","Revision") SELECT @id,@payload::jsonb,1 WHERE @revision=0
            ON CONFLICT ("PersonID") DO UPDATE SET "Payload"=EXCLUDED."Payload","Revision"="StatementDrafts"."Revision"+1,"UpdatedAt"=now() AT TIME ZONE 'UTC'
            WHERE "StatementDrafts"."Revision"=@revision RETURNING "Revision"
            """, connection, transaction);
        command.Parameters.AddWithValue("id", actor); command.Parameters.AddWithValue("payload", JsonSerializer.Serialize(request.Statement, Json)); command.Parameters.AddWithValue("revision", request.Revision);
        // INSERT's revision filter is only for new drafts; existing drafts use an explicit update.
        if (request.Revision != 0)
            command.CommandText = """
                UPDATE "Lms"."StatementDrafts" SET "Payload"=@payload::jsonb,"Revision"="Revision"+1,"UpdatedAt"=now() AT TIME ZONE 'UTC'
                WHERE "PersonID"=@id AND "Revision"=@revision RETURNING "Revision"
                """;
        return await command.ExecuteScalarAsync() is long revision ? revision : throw new ApiFailure(409, "Ведомость изменена в другой вкладке. Обновите данные перед сохранением.");
    }
}
