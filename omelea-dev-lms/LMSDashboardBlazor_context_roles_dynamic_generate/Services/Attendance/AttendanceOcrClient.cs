using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LMSDashboardBlazor.Services.Attendance;

using Models.Attendance;
using Models;

public sealed class AttendanceOcrClient(IConfiguration configuration, AuthSessionService session) : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private Uri Endpoint(string path) => new(new Uri(
        (configuration["LmsApi:BaseUrl"] ?? "http://localhost:8085").TrimEnd('/') + "/"), path);

    public async Task<OcrJob> RecognizeAsync(byte[] bytes, string filename, string contentType,
        string mode, LmsCourseModel course, AttendanceLessonModel? lesson,
        IEnumerable<LmsUserModel> students, string? teacherId, CancellationToken cancellation)
    {
        // Keep the component contract; identity and roster are resolved exclusively by the gateway.
        using var upload = new MultipartFormDataContent();
        using var image = new ByteArrayContent(bytes);
        image.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        upload.Add(image, "file", filename);
        var path = $"api/ocr/recognize?courseId={Uri.EscapeDataString(course.Id)}&mode={Uri.EscapeDataString(mode)}";
        if (lesson is not null) path += $"&lessonId={Uri.EscapeDataString(lesson.Id)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint(path))
        {
            Content = upload
        };
        using var response = await session.SendRequestAsync(request, cancellation);
        await EnsureSuccess(response, cancellation);
        var job = await ReadJob(response, cancellation);
        if (job.Status == "failed" || job.Result is null)
            throw new InvalidOperationException("Распознавание не завершено.");
        return job;
    }

    public async Task ConfirmAsync(string jobId, IReadOnlyList<OcrImportRow> rows, CancellationToken cancellation)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            Endpoint($"api/ocr/jobs/{Uri.EscapeDataString(jobId)}/confirm"))
        {
            Content = JsonContent.Create(rows, options: JsonOptions)
        };
        using var response = await session.SendRequestAsync(request, cancellation);
        await EnsureSuccess(response, cancellation);
    }

    private static async Task<OcrJob> ReadJob(HttpResponseMessage response, CancellationToken cancellation) =>
        await response.Content.ReadFromJsonAsync<OcrJob>(JsonOptions, cancellation)
        ?? throw new InvalidOperationException("Пустой ответ OCR-сервиса.");

    private static async Task EnsureSuccess(HttpResponseMessage response, CancellationToken cancellation)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync(cancellation);
        string detail;
        try
        {
            using var document = JsonDocument.Parse(body);
            detail = document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("message", out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()! : "Не удалось выполнить запрос к OCR-шлюзу.";
        }
        catch (JsonException) { detail = "Не удалось выполнить запрос к OCR-шлюзу."; }
        if (detail.Contains("not configured", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("OCR-модель не настроена: нужны исходники и веса модели. Для проверки интерфейса доступен демонстрационный режим.");
        throw new InvalidOperationException($"OCR-сервис: {(int)response.StatusCode}. {detail[..Math.Min(detail.Length, 500)]}");
    }

    public void Dispose() { }
}
