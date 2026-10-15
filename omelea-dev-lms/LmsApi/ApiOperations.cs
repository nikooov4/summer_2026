using System.Security.Claims;
using LMSDashboardBlazor.Models;
using LMSDashboardBlazor.Models.Attendance;
using LMSDashboardBlazor.Services.Admin;
using LMSDashboardBlazor.Services.Attendance;
using LMSDashboardBlazor.Services.Statements;

namespace LmsApi;

public sealed class ApiFailure(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}

public sealed class ApiOperations(LmsRepository repository)
{
    internal static LmsUserModel Actor(LmsSnapshot state, ClaimsPrincipal principal) =>
        state.Users.FirstOrDefault(user => user.Id == principal.FindFirstValue("sub"))
        ?? throw new ApiFailure(401, "Пользователь не найден или отключён.");

    internal static bool Editor(LmsSnapshot state, LmsUserModel actor, string courseId) => actor.IsAdmin ||
        state.Enrollments.Any(item => item.UserId == actor.Id && item.CourseId == courseId && item.Role is LmsCourseRole.Teacher or LmsCourseRole.Assistant);

    internal static bool StaffReader(LmsSnapshot state, LmsUserModel actor, string courseId) => actor.IsAdmin ||
        state.Enrollments.Any(item => item.UserId == actor.Id && item.CourseId == courseId && item.Role is LmsCourseRole.Teacher or LmsCourseRole.Assistant or LmsCourseRole.Curator);

    public static LmsSnapshot Filter(LmsSnapshot state, LmsUserModel actor)
    {
        if (actor.IsAdmin) return state;
        var visibleCourses = state.Enrollments.Where(item => item.UserId == actor.Id).Select(item => item.CourseId)
            .Concat(state.Attendance.Lessons.Where(lesson => lesson.Entries.Any(entry => entry.StudentId == actor.Id)).Select(lesson => lesson.CourseId)).ToHashSet();
        var staffCourses = visibleCourses.Where(id => StaffReader(state, actor, id)).ToHashSet();
        state.Attendance.Lessons = state.Attendance.Lessons.Where(lesson => visibleCourses.Contains(lesson.CourseId)
            && (staffCourses.Contains(lesson.CourseId) || lesson.Entries.Any(entry => entry.StudentId == actor.Id))).ToList();
        foreach (var lesson in state.Attendance.Lessons)
            if (!staffCourses.Contains(lesson.CourseId))
            {
                lesson.Entries = lesson.Entries.Where(entry => entry.StudentId == actor.Id).ToList();
                lesson.Roster = lesson.Roster.Where(student => student.Id == actor.Id).ToList();
            }
        // Historical names belong to lesson snapshots, not to the current account/profile directory.
        var visibleUsers = state.Enrollments.Where(item => staffCourses.Contains(item.CourseId)).Select(item => item.UserId)
            .Append(actor.Id).ToHashSet();
        state.Users = state.Users.Where(user => visibleUsers.Contains(user.Id)).ToList();
        state.Courses = state.Courses.Where(course => visibleCourses.Contains(course.Id)).ToList();
        // A person visible in a taught course must not expose their enrollment in a student-only course.
        state.Enrollments = state.Enrollments.Where(item => visibleCourses.Contains(item.CourseId)
            && (staffCourses.Contains(item.CourseId) || item.UserId == actor.Id)).ToList();
        state.Attendance.Students = state.Attendance.Lessons.SelectMany(lesson => lesson.Roster).DistinctBy(student => student.Id).ToList();
        return state;
    }

    public async Task<LmsSnapshot> Bootstrap(ClaimsPrincipal principal)
    {
        var state = await repository.Get();
        var actor = Actor(state, principal);
        var draft = await repository.GetStatement(Guid.Parse(actor.Id));
        state.Statement = draft?.Statement; state.StatementRevision = draft?.Revision ?? 0;
        return Filter(state, actor);
    }

    public async Task<CatalogCommandResult> ChangeCatalog(ClaimsPrincipal principal, CatalogCommand request)
    {
        var result = await repository.Mutate(async (state, connection, transaction) =>
        {
            var actor = Actor(state, principal);
            if (!actor.IsAdmin) throw new ApiFailure(403, "Управление каталогом доступно только администратору.");
            if (request.Payload.Length > 1_000_000 || request.Rows.Count > 2000) throw new ApiFailure(400, "Слишком большой импорт.");
            var catalog = new LmsCatalogService();
            catalog.Load(state.Users, state.Courses, state.Enrollments);
            var count = 0; var created = 0; var enrolled = 0;
            switch (request.Kind)
            {
                case "add-user": catalog.AddUser(request.Username, request.FullName, request.RecordBookNumber, request.IsAdmin); break;
                case "update-user":
                    var userToEdit = catalog.GetUser(request.Id) ?? throw new ApiFailure(404, "Пользователь не найден.");
                    userToEdit.Username = request.Username.Trim(); userToEdit.FullName = request.FullName.Trim();
                    userToEdit.RecordBookNumber = request.RecordBookNumber.Trim(); userToEdit.IsAdmin = request.IsAdmin; break;
                case "toggle-admin": catalog.ToggleAdmin(request.Id); break;
                case "remove-user":
                    if (request.Id == actor.Id) throw new ApiFailure(400, "Нельзя удалить пользователя текущей сессии.");
                    catalog.RemoveUser(request.Id); break;
                case "import-users":
                    count = request.Format switch { "csv" => catalog.ImportUsersCsv(request.Payload), "json" => catalog.ImportUsersJson(request.Payload), _ => throw new ApiFailure(400, "Неизвестный формат.") }; break;
                case "add-course": catalog.AddCourse(request.Code, request.Name, request.GroupName, request.DefaultRoom); break;
                case "update-course":
                    var courseToEdit = catalog.GetCourse(request.Id) ?? throw new ApiFailure(404, "Курс не найден.");
                    if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.GroupName))
                        throw new ApiFailure(400, "Укажите код, название и группу курса.");
                    courseToEdit.Code = request.Code.Trim(); courseToEdit.Name = request.Name.Trim(); courseToEdit.GroupName = request.GroupName.Trim();
                    courseToEdit.DefaultRoom = string.IsNullOrWhiteSpace(request.DefaultRoom) ? "Не указана" : request.DefaultRoom.Trim(); break;
                case "remove-course": catalog.RemoveCourse(request.Id); break;
                case "add-enrollment":
                    if (!Enum.IsDefined(request.Role)) throw new ApiFailure(400, "Неизвестная роль.");
                    catalog.AddEnrollment(request.UserId, request.CourseId, request.Role); break;
                case "remove-enrollment": catalog.RemoveEnrollment(request.Id); break;
                case "ocr-import": (created, enrolled) = catalog.ImportOcrStudents(actor.Username, request.CourseId, request.Rows); break;
                default: throw new ApiFailure(400, "Неизвестная операция каталога.");
            }
            foreach (var user in catalog.Users)
                if (user.Username.Length > 100 || user.Username.Any(char.IsWhiteSpace) || user.FullName.Length is < 1 or > 200 || user.RecordBookNumber.Length > 100)
                    throw new ApiFailure(400, "Проверьте ФИО, логины и номера зачётных книжек.");
            foreach (var course in catalog.Courses)
                if (course.Name.Length > 300 || course.Code.Length > 100 || course.GroupName.Length > 100 || course.DefaultRoom.Length > 100)
                    throw new ApiFailure(400, "Поля курса слишком длинные.");
            state.Users = catalog.Users.ToList(); state.Courses = catalog.Courses.ToList(); state.Enrollments = catalog.Enrollments.ToList();
            await repository.WriteCatalog(connection, transaction, state);
            await using var availableAdmin = new Npgsql.NpgsqlCommand("""
                SELECT count(*) FROM "Lms"."UserAccounts" u JOIN "Lms"."AuthCredentials" c ON c."PersonID"=u."PersonID"
                WHERE u."IsAdmin" AND NOT u."IsArchived" AND c."IsEnabled"
                """, connection, transaction);
            if ((long)(await availableAdmin.ExecuteScalarAsync())! == 0)
                throw new ApiFailure(400, "Должен остаться хотя бы один администратор с действующим входом.");
            return (count, created, enrolled);
        });
        return new(result.State, result.Result.count, result.Result.created, result.Result.enrolled);
    }

    public async Task<AttendanceLessonModel> SaveJournal(ClaimsPrincipal principal, string lessonId, JournalSaveRequest request)
    {
        var result = await repository.Mutate(async (state, connection, transaction) =>
        {
            var actor = Actor(state, principal);
            var lesson = RequireLesson(state, actor, lessonId, request.Revision);
            if (lesson.IsCancelled) throw new ApiFailure(409, "Занятие отменено.");
            if (lesson.IsCompleted) throw new ApiFailure(409, "Журнал завершён. Сначала возобновите его.");
            if (request.Entries is null || request.Entries.Any(entry => entry is null)) throw new ApiFailure(400, "Не указан состав журнала.");
            var expected = lesson.Entries.Select(entry => entry.StudentId).ToHashSet();
            var ids = request.Entries.Select(entry => entry.StudentId).ToList();
            if (ids.Count != ids.Distinct().Count() || !expected.SetEquals(ids))
                throw new ApiFailure(409, "Состав журнала не соответствует сохранённому составу занятия.");
            foreach (var entry in request.Entries)
            {
                var error = AttendanceService.ValidateEntry(entry);
                if (error is not null) throw new ApiFailure(400, error);
            }
            lesson.Entries = request.Entries;
            await repository.WriteJournal(connection, transaction, lesson, Guid.Parse(actor.Id));
            await LmsRepository.Execute(connection, transaction, "UPDATE \"Attendance\".\"Lessons\" SET \"Revision\"=\"Revision\"+1 WHERE \"ID\"=@id", ("id", Guid.Parse(lesson.Id)));
            return lesson.Id;
        });
        return result.State.Attendance.Lessons.Single(lesson => lesson.Id == result.Result);
    }

    internal static AttendanceLessonModel RequireLesson(LmsSnapshot state, LmsUserModel actor, string lessonId, long revision)
    {
        var lesson = state.Attendance.Lessons.FirstOrDefault(item => item.Id == lessonId) ?? throw new ApiFailure(404, "Занятие не найдено.");
        if (!Editor(state, actor, lesson.CourseId)) throw new ApiFailure(403, "Нет прав на заполнение этого журнала.");
        if (lesson.Revision != revision) throw new ApiFailure(409, "Журнал изменён в другой вкладке. Обновите страницу перед сохранением.");
        return lesson;
    }

    public async Task<AttendanceLessonModel> SetJournalState(ClaimsPrincipal principal, string lessonId, long revision, bool complete)
    {
        var result = await repository.Mutate(async (state, connection, transaction) =>
        {
            var actor = Actor(state, principal);
            var lesson = RequireLesson(state, actor, lessonId, revision);
            if (lesson.IsCancelled) throw new ApiFailure(409, "Занятие отменено.");
            if (complete && lesson.Entries.Any(entry => entry.Status == AttendanceStatus.Unmarked || AttendanceService.ValidateEntry(entry) is not null))
                throw new ApiFailure(400, "Для завершения заполните все отметки и исправьте ошибки.");
            await LmsRepository.Execute(connection, transaction, """
                UPDATE "Attendance"."Lessons" SET "CompletedAt"=CASE WHEN @complete THEN now() AT TIME ZONE 'UTC' ELSE NULL END,"Revision"="Revision"+1 WHERE "ID"=@id
                """, ("complete", complete), ("id", Guid.Parse(lesson.Id)));
            return lesson.Id;
        });
        return result.State.Attendance.Lessons.Single(lesson => lesson.Id == result.Result);
    }

    public async Task<StatementSaveResult> SaveStatement(ClaimsPrincipal principal, StatementSaveRequest request)
    {
        var result = await repository.Mutate(async (state, connection, transaction) =>
        {
            var actor = Actor(state, principal);
            if (!actor.IsAdmin && !state.Enrollments.Any(item => item.UserId == actor.Id && item.Role is LmsCourseRole.Teacher or LmsCourseRole.Assistant))
                throw new ApiFailure(403, "Сохранение ведомостей доступно преподавателю или администратору.");
            var statement = request.Statement;
            if (statement.Name.Length is < 1 or > 300 || statement.Subjects.Count is < 1 or > 100 || statement.Students.Count is < 1 or > 2000)
                throw new ApiFailure(400, "Проверьте название и размер ведомости.");
            var calculator = new StatementService();
            foreach (var student in statement.Students)
            {
                if (student.FullName.Length is < 1 or > 200 || student.Sections.Count != statement.Subjects.Count || student.Sections.Any(score => score is < 0 or > 10000) || student.Exam is < 0 or > 10000)
                    throw new ApiFailure(400, "Проверьте ФИО, количество разделов и баллы.");
                student.SectionSum = student.Sections.Sum(); student.Total = student.SectionSum + student.Exam;
                var grade = calculator.CalculateGrade(student.Total); student.Mark = grade.Text; student.Code = grade.Code; student.ECTS = grade.ECTS;
            }
            var revision = await repository.WriteStatement(connection, transaction, Guid.Parse(actor.Id), request);
            return new StatementSaveResult(statement, revision);
        });
        return result.Result;
    }
}
