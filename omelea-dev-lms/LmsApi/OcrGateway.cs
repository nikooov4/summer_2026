using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using LMSDashboardBlazor.Models;
using LMSDashboardBlazor.Models.Attendance;
using Npgsql;
using NpgsqlTypes;

namespace LmsApi;

public sealed class OcrGateway(LmsRepository repository, NpgsqlDataSource source,
    IHttpClientFactory clients, IConfiguration configuration)
{
    private const int MaxFileBytes = 12 * 1024 * 1024;
    private const int MaxResponseBytes = 4 * 1024 * 1024;
    private const int MaxRows = 2000;
    private const string NotConfigured = "OCR-модель не настроена: нужны исходники и веса модели. Для проверки интерфейса доступен демонстрационный режим.";
    private static readonly JsonSerializerOptions PythonJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<OcrJob> Recognize(ClaimsPrincipal actor, HttpRequest request, string courseId,
        string? lessonId, string mode, CancellationToken cancellation)
    {
        if (mode is not ("nasledie" or "legacy_v1" or "demo"))
            throw new ApiFailure(400, "Неизвестный режим OCR.");
        var context = RequireAccess(await repository.Get(), actor, courseId, lessonId);
        var image = await ReadImage(request, cancellation);
        using var http = CreateClient();
        using var upload = new MultipartFormDataContent();
        using var content = new ByteArrayContent(image.Bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
        upload.Add(content, "file", image.ContentType == "image/png" ? "photo.png" : "photo.jpg");
        var stored = await Send<StoredImage>(http, HttpMethod.Post, "attendance/ocr/images", upload, cancellation);
        if (!SafeId(stored.ImageId)) throw InvalidUpstream();
        using var create = JsonContent.Create(new
        {
            ImageIds = new[] { stored.ImageId },
            Lesson = new
            {
                LessonId = context.Lesson?.Id,
                Date = context.Lesson?.StartsAt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                Subject = context.Course.Name,
                Group = context.Course.GroupName,
                TeacherId = context.User.Id
            },
            Students = context.Roster.Select(student => new
            {
                StudentId = student.Id, student.FullName, Group = context.Course.GroupName
            }).ToArray()
        }, options: PythonJson);
        var job = await Send<OcrJob>(http, HttpMethod.Post, "attendance/ocr/jobs", create, cancellation);
        if (!SafeId(job.JobId)) throw InvalidUpstream();
        var upstreamId = job.JobId;
        var localId = Guid.NewGuid();
        await InsertJob(localId, upstreamId, context, cancellation);
        try
        {
            job = await Send<OcrJob>(http, HttpMethod.Post,
                $"attendance/ocr/jobs/{upstreamId}/process?mode={mode}", null, cancellation);
            ValidateJob(job, upstreamId, context);
            await SetStatus(localId, job.Status, cancellation);
            job.JobId = localId.ToString();
            return job;
        }
        catch
        {
            // Preserve the original, sanitized failure even if the client disconnected or the DB went away.
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try { await SetStatus(localId, "failed", cleanup.Token); }
            catch (Exception error) when (error is NpgsqlException or OperationCanceledException) { }
            throw;
        }
    }

    public async Task<OcrJob> Confirm(ClaimsPrincipal actor, string jobId, List<OcrImportRow> rows,
        CancellationToken cancellation)
    {
        if (!Guid.TryParse(jobId, out var id)) throw new ApiFailure(404, "Задание OCR не найдено.");
        ValidateRows(rows);
        await using var connection = await source.OpenConnectionAsync(cancellation);
        await using var transaction = await connection.BeginTransactionAsync(cancellation);
        string upstreamId, ownerId, courseId, status;
        string? lessonId;
        // Only this job is locked during review, not the shared catalog/journal mutation lock.
        await using (var command = new NpgsqlCommand("""
            SELECT "UpstreamJobID","OwnerPersonID","CourseID","LessonID","Status"
            FROM "Lms"."OcrJobs" WHERE "ID"=@id FOR UPDATE
            """, connection, transaction))
        {
            command.Parameters.AddWithValue("id", id);
            await using var reader = await command.ExecuteReaderAsync(cancellation);
            if (!await reader.ReadAsync(cancellation)) throw new ApiFailure(404, "Задание OCR не найдено.");
            upstreamId = reader.GetString(0);
            ownerId = reader.GetGuid(1).ToString();
            courseId = reader.GetGuid(2).ToString();
            lessonId = reader.IsDBNull(3) ? null : reader.GetGuid(3).ToString();
            status = reader.GetString(4);
        }
        var state = await repository.Read(connection, transaction);
        var user = RequireActor(state, actor);
        if (ownerId != user.Id) throw new ApiFailure(403, "Нет доступа к этому заданию OCR.");
        var context = RequireAccess(state, actor, courseId, lessonId);
        if (status is not ("completed" or "needs_review"))
            throw new ApiFailure(409, "Задание уже подтверждено или не готово к проверке. Повторите распознавание.");
        var rosterIds = context.Roster.Select(student => student.Id).ToHashSet(StringComparer.Ordinal);
        if (rows.Any(row => row.Include && !rosterIds.Contains(row.StudentId)))
            throw new ApiFailure(400, "Выбранный студент отсутствует в составе занятия.");
        if (!SafeId(upstreamId)) throw InvalidUpstream();
        using var http = CreateClient();
        var review = await Send<UpstreamReview>(http, HttpMethod.Get,
            $"attendance/ocr/jobs/{upstreamId}/review", null, cancellation, reviewConflict: true);
        if (review.JobId != upstreamId || review.Decisions is null ||
            review.Decisions.Any(decision => decision is null || !SafeId(decision.DecisionId)))
            throw InvalidUpstream();
        var decisionIds = review.Decisions.Select(decision => decision.DecisionId).ToHashSet(StringComparer.Ordinal);
        if (decisionIds.Count != review.Decisions.Count) throw InvalidUpstream();
        if (!decisionIds.SetEquals(rows.Select(row => row.DecisionId)))
            throw new ApiFailure(400, "Строки проверки не соответствуют заданию OCR. Повторите распознавание.");
        using var corrections = JsonContent.Create(new
        {
            Corrections = rows.Select(row => new
            {
                row.DecisionId,
                Action = row.Include ? "select_student" : "ignore",
                SelectedStudentId = row.Include ? row.StudentId : null
            }).ToArray()
        }, options: PythonJson);
        await Send<UpstreamReview>(http, HttpMethod.Patch,
            $"attendance/ocr/jobs/{upstreamId}/review", corrections, cancellation, reviewConflict: true);
        var confirmed = await Send<OcrJob>(http, HttpMethod.Post,
            $"attendance/ocr/jobs/{upstreamId}/review/confirm", null, cancellation, reviewConflict: true);
        ValidateJob(confirmed, upstreamId, context);
        if (confirmed.Status != "completed") throw InvalidUpstream();
        await using (var command = new NpgsqlCommand("""
            UPDATE "Lms"."OcrJobs" SET "Status"='confirmed',"UpdatedAt"=now() AT TIME ZONE 'UTC' WHERE "ID"=@id
            """, connection, transaction))
        {
            command.Parameters.AddWithValue("id", id);
            await command.ExecuteNonQueryAsync(cancellation);
        }
        await transaction.CommitAsync(cancellation);
        // Confirmation only updates OCR review. Journal entries remain a normal Save/Revert draft.
        confirmed.JobId = id.ToString();
        return confirmed;
    }

    private static LmsUserModel RequireActor(LmsSnapshot state, ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) throw new ApiFailure(401, "Войдите в LMS.");
        return state.Users.FirstOrDefault(user => user.Id == principal.FindFirstValue("sub"))
            ?? throw new ApiFailure(401, "Пользователь не найден или отключён.");
    }

    private static AccessContext RequireAccess(LmsSnapshot state, ClaimsPrincipal principal,
        string courseId, string? lessonId)
    {
        var user = RequireActor(state, principal);
        var course = state.Courses.FirstOrDefault(course => course.Id == courseId)
            ?? throw new ApiFailure(404, "Курс не найден.");
        if (!user.IsAdmin && !state.Enrollments.Any(item => item.UserId == user.Id && item.CourseId == course.Id &&
            item.Role is LmsCourseRole.Teacher or LmsCourseRole.Assistant))
            throw new ApiFailure(403, "Нет прав на заполнение этого журнала.");
        AttendanceLessonModel? lesson = null;
        if (!string.IsNullOrWhiteSpace(lessonId))
        {
            lesson = state.Attendance.Lessons.FirstOrDefault(item => item.Id == lessonId && item.CourseId == course.Id)
                ?? throw new ApiFailure(404, "Занятие не найдено в этом курсе.");
            if (lesson.IsCancelled) throw new ApiFailure(409, "Занятие отменено.");
            if (lesson.IsCompleted) throw new ApiFailure(409, "Журнал завершён. Сначала возобновите его.");
        }
        else if (!user.IsAdmin) throw new ApiFailure(400, "Для распознавания выберите занятие.");
        List<RosterStudent> roster;
        if (lesson is not null)
        {
            var frozenIds = lesson.Entries.Select(entry => entry.StudentId).ToHashSet(StringComparer.Ordinal);
            var frozenStudents = lesson.Roster.Count > 0 ? lesson.Roster : state.Attendance.Students;
            roster = frozenStudents.Where(student => frozenIds.Contains(student.Id))
                .Select(student => new RosterStudent(student.Id, student.FullName)).ToList();
            if (roster.Count != frozenIds.Count) throw new ApiFailure(409, "Состав занятия недоступен. Обновите страницу.");
        }
        else roster = state.Users.Select(student => new RosterStudent(student.Id, student.FullName)).ToList();
        if (roster.Count > MaxRows) throw new ApiFailure(400, "Слишком большой состав для OCR: не более 2000 студентов.");
        return new(user, course, lesson, roster);
    }

    private static void ValidateRows(List<OcrImportRow>? rows)
    {
        if (rows is null || rows.Count is < 1 or > MaxRows || rows.Any(row => row is null))
            throw new ApiFailure(400, "Укажите от 1 до 2000 строк проверки OCR.");
        foreach (var row in rows)
            if (!SafeId(row.DecisionId) || row.StudentId is null || row.StudentId.Length > 128 ||
                row.SourceText is null || row.SourceText.Length > 4096 || row.FullName is null || row.FullName.Length > 200 ||
                row.Username is null || row.Username.Length > 100 || row.RecordBookNumber is null || row.RecordBookNumber.Length > 100 ||
                !double.IsFinite(row.Confidence) || row.Confidence is < 0 or > 1)
                throw new ApiFailure(400, "Проверьте поля строк OCR и идентификаторы решений.");
        var included = rows.Where(row => row.Include).ToList();
        if (included.Count == 0) throw new ApiFailure(400, "Выберите хотя бы одного студента.");
        if (rows.Select(row => row.DecisionId).Distinct(StringComparer.Ordinal).Count() != rows.Count ||
            included.Select(row => row.StudentId).Distinct(StringComparer.Ordinal).Count() != included.Count)
            throw new ApiFailure(400, "Один студент или строка проверки выбраны несколько раз.");
    }

    private static async Task<UploadedImage> ReadImage(HttpRequest request, CancellationToken cancellation)
    {
        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var type) ||
            !string.Equals(type.MediaType, "multipart/form-data", StringComparison.OrdinalIgnoreCase))
            throw new ApiFailure(400, "Отправьте одно фото JPG или PNG.");
        try
        {
            var form = await request.ReadFormAsync(cancellation);
            if (form.Files.Count != 1 || form.Count != 0 || form.Files[0].Name != "file")
                throw new ApiFailure(400, "Отправьте только один файл в поле file.");
            var file = form.Files[0];
            if (file.Length > MaxFileBytes) throw new ApiFailure(413, "Размер фото не должен превышать 12 МБ.");
            if (file.Length == 0 || file.FileName.Length > 255) throw new ApiFailure(400, "Выберите непустое фото JPG или PNG.");
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var png = extension == ".png" && string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase);
            var jpg = extension is ".jpg" or ".jpeg" && string.Equals(file.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase);
            if (!png && !jpg) throw new ApiFailure(400, "Разрешены только файлы JPG и PNG.");
            await using var stream = file.OpenReadStream();
            var bytes = await ReadBounded(stream, MaxFileBytes, cancellation, upload: true);
            if (!(png && bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) &&
                !(jpg && bytes.AsSpan().StartsWith(new byte[] { 255, 216, 255 })))
                throw new ApiFailure(400, "Содержимое файла не соответствует формату JPG или PNG.");
            return new(bytes, png ? "image/png" : "image/jpeg");
        }
        catch (BadHttpRequestException error) { throw new ApiFailure(error.StatusCode == 413 ? 413 : 400, "Некорректная или слишком большая загрузка фото."); }
        catch (Exception error) when (error is InvalidDataException or IOException)
        { throw new ApiFailure(400, "Не удалось прочитать фото. Отправьте JPG или PNG размером до 12 МБ."); }
    }

    private HttpClient CreateClient()
    {
        var configured = configuration["AttendanceOcr:BaseUrl"] ?? "http://localhost:8010";
        if (!Uri.TryCreate(configured.TrimEnd('/') + "/", UriKind.Absolute, out var endpoint) ||
            endpoint.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(endpoint.UserInfo) ||
            !string.IsNullOrEmpty(endpoint.Query) || !string.IsNullOrEmpty(endpoint.Fragment))
            throw new ApiFailure(503, NotConfigured);
        var client = clients.CreateClient("ocr");
        client.BaseAddress = endpoint;
        return client;
    }

    private static async Task<T> Send<T>(HttpClient http, HttpMethod method, string path, HttpContent? content,
        CancellationToken cancellation, bool reviewConflict = false) where T : class
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromMinutes(5));
        try
        {
            using var request = new HttpRequestMessage(method, path) { Content = content };
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            var bytes = await ReadBounded(stream, MaxResponseBytes, timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                var body = System.Text.Encoding.UTF8.GetString(bytes);
                if (body.Contains("not configured", StringComparison.OrdinalIgnoreCase)) throw new ApiFailure(503, NotConfigured);
                if (reviewConflict && response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity)
                    throw new ApiFailure(409, "OCR-проверка не может быть подтверждена. Обновите выбор или повторите распознавание.");
                if (response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests or HttpStatusCode.GatewayTimeout)
                    throw new ApiFailure(503, "OCR-сервис временно недоступен. Повторите позже.");
                throw new ApiFailure(502, "OCR-сервис не смог обработать запрос. Проверьте фото и повторите распознавание.");
            }
            return JsonSerializer.Deserialize<T>(bytes, PythonJson) ?? throw InvalidUpstream();
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        { throw new ApiFailure(503, "OCR-сервис не ответил вовремя. Повторите позже."); }
        catch (HttpRequestException) { throw new ApiFailure(503, "OCR-сервис недоступен. Повторите позже."); }
        catch (IOException) { throw new ApiFailure(502, "Не удалось прочитать ответ OCR-сервиса."); }
        catch (JsonException) { throw InvalidUpstream(); }
    }

    private static async Task<byte[]> ReadBounded(Stream stream, int limit, CancellationToken cancellation, bool upload = false)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(chunk.AsMemory(0, Math.Min(chunk.Length, limit + 1 - (int)buffer.Length)), cancellation)) > 0)
        {
            if (buffer.Length + read > limit)
                throw new ApiFailure(upload ? 413 : 502, upload ? "Размер фото не должен превышать 12 МБ." : "Слишком большой ответ OCR-сервиса.");
            buffer.Write(chunk, 0, read);
        }
        return buffer.ToArray();
    }

    private static void ValidateJob(OcrJob job, string upstreamId, AccessContext context)
    {
        if (job.Status == "failed")
            throw job.Error?.Contains("not configured", StringComparison.OrdinalIgnoreCase) == true
                ? new ApiFailure(503, NotConfigured)
                : new ApiFailure(502, "OCR-сервис не смог распознать фото. Попробуйте другое изображение.");
        if (job.JobId != upstreamId || job.Status is not ("completed" or "needs_review") || job.Result?.Students is null ||
            job.Review?.Decisions is null || job.Result.Students.Count > MaxRows || job.Review.Decisions.Count > MaxRows ||
            job.Result.Students.Any(student => student is null || student.SourceText is null || student.SourceText.Length > 4096 ||
                student.RegionIds is null || !double.IsFinite(student.Confidence) || student.Confidence is < 0 or > 1) ||
            job.Review.Decisions.Any(decision => decision is null || !SafeId(decision.DecisionId) || decision.RegionIds is null) ||
            job.Review.Decisions.Select(decision => decision.DecisionId).Distinct(StringComparer.Ordinal).Count() != job.Review.Decisions.Count)
            throw InvalidUpstream();
        var rosterIds = context.Roster.Select(student => student.Id).ToHashSet(StringComparer.Ordinal);
        if (job.Result.Students.Any(student => student.StudentId is not null && !rosterIds.Contains(student.StudentId)))
            throw InvalidUpstream();
        job.Error = null;
    }

    private async Task InsertJob(Guid id, string upstreamId, AccessContext context, CancellationToken cancellation)
    {
        await using var connection = await source.OpenConnectionAsync(cancellation);
        await using var command = new NpgsqlCommand("""
            INSERT INTO "Lms"."OcrJobs" ("ID","UpstreamJobID","OwnerPersonID","CourseID","LessonID","Status")
            VALUES (@id,@upstream,@owner,@course,@lesson,'processing')
            """, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("upstream", upstreamId);
        command.Parameters.AddWithValue("owner", Guid.Parse(context.User.Id));
        command.Parameters.AddWithValue("course", Guid.Parse(context.Course.Id));
        command.Parameters.Add("lesson", NpgsqlDbType.Uuid).Value = context.Lesson is null ? DBNull.Value : Guid.Parse(context.Lesson.Id);
        await command.ExecuteNonQueryAsync(cancellation);
    }

    private async Task SetStatus(Guid id, string status, CancellationToken cancellation)
    {
        await using var connection = await source.OpenConnectionAsync(cancellation);
        await using var command = new NpgsqlCommand("""
            UPDATE "Lms"."OcrJobs" SET "Status"=@status,"UpdatedAt"=now() AT TIME ZONE 'UTC' WHERE "ID"=@id
            """, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("status", status);
        await command.ExecuteNonQueryAsync(cancellation);
    }

    private static bool SafeId(string? value) => value is { Length: > 0 and <= 128 } &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-');
    private static ApiFailure InvalidUpstream() => new(502, "OCR-сервис вернул некорректный ответ. Повторите распознавание.");
    private sealed class StoredImage { public string ImageId { get; set; } = ""; }
    private sealed class UpstreamReview
    {
        public string JobId { get; set; } = "";
        public List<OcrDecision> Decisions { get; set; } = [];
    }
    private sealed record UploadedImage(byte[] Bytes, string ContentType);
    private sealed record RosterStudent(string Id, string FullName);
    private sealed record AccessContext(LmsUserModel User, LmsCourseModel Course,
        AttendanceLessonModel? Lesson, List<RosterStudent> Roster);
}
