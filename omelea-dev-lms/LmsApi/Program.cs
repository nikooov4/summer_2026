using System.Security.Claims;
using LmsApi;
using LMSDashboardBlazor.Models;
using Npgsql;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();
var root = new DirectoryInfo(builder.Environment.ContentRootPath);
while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "database"))) root = root.Parent;
var workspace = root?.FullName ?? throw new InvalidOperationException("Repository database directory not found.");
var connectionFile = Path.Combine(workspace, ".local", "lms-connection.txt");
var connectionString = builder.Configuration.GetConnectionString("Lms")
    ?? (File.Exists(connectionFile) ? File.ReadAllText(connectionFile).Trim() : null)
    ?? throw new InvalidOperationException("Set ConnectionStrings__Lms or run scripts/start-local-database.ps1 first.");
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<LmsRepository>();
builder.Services.AddSingleton<ApiOperations>();
builder.Services.AddSingleton<AttendanceOperations>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();
builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210_000);
builder.Services.AddHttpClient("ocr", client => client.Timeout = TimeSpan.FromMinutes(4))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseProxy = false });
builder.Services.AddSingleton<OcrGateway>();
var keys = builder.Services.AddDataProtection().SetApplicationName("LmsAttendance")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(workspace, ".local", "auth-keys")));
if (OperatingSystem.IsWindows()) keys.ProtectKeysWithDpapi();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "Lms.Attendance.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.SlidingExpiration = false;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    options.Events.OnValidatePrincipal = async context =>
    {
        var user = await context.HttpContext.RequestServices.GetRequiredService<AuthService>().GetUser(context.Principal!);
        if (user is null) context.RejectPrincipal();
        else context.HttpContext.Items["session-user"] = user;
    };
});
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "Lms.Attendance.Csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context => RateLimitPartition.GetSlidingWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new SlidingWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 6, QueueLimit = 0 }));
});
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins((builder.Configuration["Lms:AllowedOrigins"] ?? "http://localhost:5293,http://127.0.0.1:5293").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .WithMethods("GET", "POST", "PUT").WithHeaders("Content-Type", "X-CSRF-TOKEN").AllowCredentials()));
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 14 * 1024 * 1024);
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options => options.MultipartBodyLengthLimit = 13 * 1024 * 1024);
var app = builder.Build();
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (Exception error) when (error is ApiFailure or InvalidOperationException or System.Text.Json.JsonException or PostgresException)
    {
        var status = error is ApiFailure failure ? failure.Status : error is PostgresException pg && pg.SqlState == "23505" ? 409 : 400;
        var message = error is PostgresException ? "Операция нарушает ограничения базы данных. Проверьте повторы и связанные записи." : error.Message;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { message });
    }
    catch (Exception error) when (error is not OperationCanceledException)
    {
        app.Logger.LogError(error, "API request failed: {TraceId}", context.TraceIdentifier);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { message = "Внутренняя ошибка сервера.", requestId = context.TraceIdentifier });
    }
});
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.Headers.CacheControl = "no-store";
        if (context.Request.ContentLength > 2 * 1024 * 1024 && context.Request.Path != "/api/ocr/recognize")
            throw new ApiFailure(413, "Слишком большой запрос.");
        if (context.Items["session-user"] is SessionUser { MustChangePassword: true } && !context.Request.Path.StartsWithSegments("/api/auth"))
            throw new ApiFailure(403, "Сначала смените временный пароль.");
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) && !HttpMethods.IsOptions(context.Request.Method))
        {
            try { await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException) { throw new ApiFailure(400, "Не удалось проверить запрос. Обновите страницу и повторите действие."); }
        }
    }
    await next(context);
});
var database = app.Services.GetRequiredService<NpgsqlDataSource>();
await DatabaseMigration.Apply(database, Path.Combine(workspace, "database"));
await app.Services.GetRequiredService<LmsRepository>().Seed();
await app.Services.GetRequiredService<AuthService>().Bootstrap(workspace, app.Environment.IsDevelopment(), app.Configuration);
app.MapGet("/health", async (NpgsqlDataSource source) =>
{
    await using var connection = await source.OpenConnectionAsync();
    return Results.Ok(new { status = "ok", database = "postgresql" });
});
var auth = app.MapGroup("/api/auth");
bool DemoLoginAllowed(HttpContext context)
{
    var address = context.Connection.RemoteIpAddress;
    return app.Environment.IsDevelopment() && app.Configuration.GetValue("Auth:EnableDemoLogin", true)
        && address is not null && System.Net.IPAddress.IsLoopback(address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address);
}
auth.MapGet("/options", (HttpContext context) => new AuthOptionsResponse(DemoLoginAllowed(context)));
if (app.Environment.IsDevelopment() && app.Configuration.GetValue("Auth:EnableDemoLogin", true))
    auth.MapPost("/demo/{username}", async (HttpContext context, string username, AuthService service) =>
    {
        if (!DemoLoginAllowed(context)) throw new ApiFailure(404, "Тестовый вход недоступен.");
        return await service.DemoLogin(context, username, workspace);
    }).RequireRateLimiting("login");
auth.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
{
    context.Response.Headers.CacheControl = "no-store";
    return new CsrfTokenResponse(antiforgery.GetAndStoreTokens(context).RequestToken!);
});
auth.MapPost("/login", (HttpContext context, LoginRequest request, AuthService service) => service.Login(context, request)).RequireRateLimiting("login");
auth.MapGet("/me", async (ClaimsPrincipal actor, AuthService service) => await service.GetUser(actor) ?? throw new ApiFailure(401, "Войдите в систему.")).RequireAuthorization();
auth.MapPost("/logout", async (HttpContext context, AuthService service) => { await service.Logout(context); return Results.NoContent(); }).RequireAuthorization();
auth.MapPost("/change-password", async (HttpContext context, PasswordChangeRequest request, AuthService service) =>
{
    await service.ChangePassword(context.User, request); await service.Logout(context); return Results.NoContent();
}).RequireAuthorization().RequireRateLimiting("login");
var api = app.MapGroup("/api").RequireAuthorization();
api.MapPost("/users/{id}/password", async (string id, ClaimsPrincipal actor, PasswordSetRequest request, AuthService service) => { await service.Provision(actor, id, request); return Results.NoContent(); });
api.MapPost("/users/{id}/access", async (string id, ClaimsPrincipal actor, AccountAccessRequest request, AuthService service) => { await service.SetAccess(actor, id, request); return Results.NoContent(); });
api.MapGet("/bootstrap", (ClaimsPrincipal actor, ApiOperations service) => service.Bootstrap(actor));
api.MapPost("/catalog/commands", (ClaimsPrincipal actor, CatalogCommand request, ApiOperations service) => service.ChangeCatalog(actor, request));
api.MapPut("/attendance/lessons/{id}/entries", (string id, ClaimsPrincipal actor, JournalSaveRequest request, ApiOperations service) => service.SaveJournal(actor, id, request));
api.MapPost("/attendance/lessons/{id}/complete", (string id, ClaimsPrincipal actor, JournalStateRequest request, ApiOperations service) => service.SetJournalState(actor, id, request.Revision, true));
api.MapPost("/attendance/lessons/{id}/reopen", (string id, ClaimsPrincipal actor, JournalStateRequest request, ApiOperations service) => service.SetJournalState(actor, id, request.Revision, false));
api.MapPut("/statements/current", (ClaimsPrincipal actor, StatementSaveRequest request, ApiOperations service) => service.SaveStatement(actor, request));
api.MapPost("/attendance/lessons", (ClaimsPrincipal actor, LMSDashboardBlazor.Models.Attendance.LessonEditRequest request, AttendanceOperations service) => service.SaveLesson(actor, null, request));
api.MapPut("/attendance/lessons/{id}", (string id, ClaimsPrincipal actor, LMSDashboardBlazor.Models.Attendance.LessonEditRequest request, AttendanceOperations service) => service.SaveLesson(actor, id, request));
api.MapPost("/attendance/lessons/{id}/cancel", (string id, ClaimsPrincipal actor, LMSDashboardBlazor.Models.Attendance.JournalRevisionRequest request, AttendanceOperations service) => service.CancelLesson(actor, id, request.Revision));
api.MapGet("/attendance/lessons/{id}/history", (string id, ClaimsPrincipal actor, AttendanceOperations service) => service.History(actor, id));
api.MapPost("/ocr/recognize", (HttpRequest request, ClaimsPrincipal actor, string courseId, string? lessonId, string mode, OcrGateway service, CancellationToken cancellation) => service.Recognize(actor, request, courseId, lessonId, mode, cancellation));
api.MapPost("/ocr/jobs/{jobId}/confirm", async (string jobId, ClaimsPrincipal actor, List<LMSDashboardBlazor.Models.Attendance.OcrImportRow> rows, OcrGateway service, CancellationToken cancellation) => { await service.Confirm(actor, jobId, rows, cancellation); return Results.NoContent(); });
await app.RunAsync();
