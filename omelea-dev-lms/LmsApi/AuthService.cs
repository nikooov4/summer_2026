using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using LMSDashboardBlazor.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace LmsApi;

public sealed class AuthService(NpgsqlDataSource source, LmsRepository repository, IPasswordHasher<string> hasher)
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);
    private static void ValidatePassword(string? password)
    {
        if (password is null || password.Length is < 12 or > 128 || string.IsNullOrWhiteSpace(password))
            throw new ApiFailure(400, "Пароль должен содержать от 12 до 128 символов.");
    }

    public async Task Bootstrap(string workspace, bool development, IConfiguration configuration)
    {
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await LmsRepository.Execute(connection, transaction, "SELECT pg_advisory_xact_lock(2026101501)");
        await using var count = new NpgsqlCommand("SELECT count(*) FROM \"Lms\".\"AuthCredentials\"", connection, transaction);
        if ((long)(await count.ExecuteScalarAsync())! > 0) return;
        var state = await repository.Read(connection, transaction);
        var admin = state.Users.FirstOrDefault(user => user.Username == "admin" && user.IsAdmin)
            ?? state.Users.FirstOrDefault(user => user.IsAdmin) ?? throw new InvalidOperationException("Create an administrator first.");
        var configured = configuration["Auth:BootstrapAdminPassword"];
        if (!development && string.IsNullOrEmpty(configured))
            throw new InvalidOperationException("Set Auth__BootstrapAdminPassword for the first production launch.");
        if (!string.IsNullOrEmpty(configured)) ValidatePassword(configured);
        var bootstrap = development ? state.Users.Where(user => user.Username is "admin" or "teacher" or "assistant" or "student" or "student2" or "student3" or "student4" or "student5" or "student6").ToList() : [admin];
        if (bootstrap.All(user => user.Id != admin.Id)) bootstrap.Add(admin);
        var issued = new Dictionary<string, string>();
        foreach (var user in bootstrap)
        {
            var password = user.Id == admin.Id && !string.IsNullOrEmpty(configured) ? configured : Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
            var mustChange = !development;
            await LmsRepository.Execute(connection, transaction, """
                INSERT INTO "Lms"."AuthCredentials" ("PersonID","PasswordHash","MustChangePassword") VALUES (@id,@hash,@change)
                """, ("id", Guid.Parse(user.Id)), ("hash", hasher.HashPassword(user.Id, password)), ("change", mustChange));
            if (development) issued[user.Username] = password;
        }
        if (development)
        {
            var path = Path.Combine(workspace, ".local", "initial-credentials.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(issued, new JsonSerializerOptions { WriteIndented = true }));
        }
        await transaction.CommitAsync();
    }

    public async Task<SessionUser> DemoLogin(HttpContext context, string username, string workspace)
    {
        if (username is not ("admin" or "teacher" or "student")) throw new ApiFailure(404, "Тестовая учётная запись не найдена.");
        var path = Path.Combine(workspace, ".local", "initial-credentials.json");
        if (!File.Exists(path)) throw new ApiFailure(404, "Тестовый вход недоступен. Используйте логин и пароль.");
        var credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(path));
        if (credentials is null || !credentials.TryGetValue(username, out var password))
            throw new ApiFailure(404, "Тестовый вход недоступен. Используйте логин и пароль.");
        return await Login(context, new LoginRequest(username, password));
    }

    public async Task<SessionUser> Login(HttpContext context, LoginRequest request)
    {
        if (request.Username is null || request.Password is null || request.Username.Length > 100 || request.Password.Length > 128)
            throw new ApiFailure(401, "Неверный логин или пароль.");
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        SessionUser? user = null;
        string? hash = null;
        var failed = 0;
        var enabled = false;
        DateTime? locked = null;
        await using (var command = new NpgsqlCommand("""
            SELECT u."PersonID",u."Username",u."DisplayName",u."IsAdmin",c."MustChangePassword",c."PasswordHash",c."FailedAttempts",c."IsEnabled",c."LockoutUntil"
            FROM "Lms"."UserAccounts" u JOIN "Lms"."AuthCredentials" c ON c."PersonID"=u."PersonID"
            WHERE lower(u."Username")=lower(@login) AND NOT u."IsArchived" FOR UPDATE OF c
            """, connection, transaction))
        {
            command.Parameters.AddWithValue("login", request.Username.Trim());
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                user = new(reader.GetGuid(0).ToString(), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3), reader.GetBoolean(4));
                hash = reader.GetString(5); failed = reader.GetInt32(6); enabled = reader.GetBoolean(7);
                locked = reader.IsDBNull(8) ? null : reader.GetDateTime(8);
            }
        }
        // Unknown accounts still run the same password hasher instead of returning early.
        var verification = hash is null ? PasswordVerificationResult.Failed : hasher.VerifyHashedPassword(user!.Id, hash, request.Password);
        if (hash is null) hasher.HashPassword("unknown", request.Password);
        if (user is null || !enabled || locked > DateTime.UtcNow || verification == PasswordVerificationResult.Failed)
        {
            if (user is not null && enabled && !(locked > DateTime.UtcNow))
                await LmsRepository.Execute(connection, transaction, """
                    UPDATE "Lms"."AuthCredentials" SET "FailedAttempts"=@failed,"LockoutUntil"=CASE WHEN @failed>=5 THEN now()+interval '15 minutes' ELSE NULL END WHERE "PersonID"=@id
                    """, ("id", Guid.Parse(user.Id)), ("failed", locked is not null ? 1 : failed + 1));
            await transaction.CommitAsync();
            throw new ApiFailure(401, "Неверный логин или пароль. После нескольких ошибок вход временно блокируется.");
        }
        await LmsRepository.Execute(connection, transaction, """
            UPDATE "Lms"."AuthCredentials" SET "FailedAttempts"=0,"LockoutUntil"=NULL,"PasswordHash"=@hash WHERE "PersonID"=@id;
            DELETE FROM "Lms"."AuthSessions" WHERE "ExpiresAt"<now()-interval '7 days' OR "RevokedAt"<now()-interval '7 days'
            """, ("id", Guid.Parse(user.Id)), ("hash", verification == PasswordVerificationResult.SuccessRehashNeeded ? hasher.HashPassword(user.Id, request.Password) : hash));
        var session = Guid.NewGuid();
        var expires = DateTime.UtcNow.Add(SessionLifetime);
        await LmsRepository.Execute(connection, transaction, """
            INSERT INTO "Lms"."AuthSessions" ("ID","PersonID","ExpiresAt") VALUES (@session,@id,@expires)
            """, ("session", session), ("id", Guid.Parse(user.Id)), ("expires", expires));
        await transaction.CommitAsync();
        var identity = new ClaimsIdentity([new("sub", user.Id), new(ClaimTypes.NameIdentifier, user.Id), new("username", user.Username), new("sid", session.ToString())], CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = expires, AllowRefresh = false });
        return user;
    }

    public async Task<SessionUser?> GetUser(ClaimsPrincipal principal)
    {
        if (!Guid.TryParse(principal.FindFirstValue("sub"), out var id) || !Guid.TryParse(principal.FindFirstValue("sid"), out var session)) return null;
        await using var connection = await source.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("""
            SELECT u."PersonID",u."Username",u."DisplayName",u."IsAdmin",c."MustChangePassword"
            FROM "Lms"."UserAccounts" u JOIN "Lms"."AuthCredentials" c ON c."PersonID"=u."PersonID"
            JOIN "Lms"."AuthSessions" s ON s."PersonID"=u."PersonID"
            WHERE u."PersonID"=@id AND s."ID"=@session AND s."RevokedAt" IS NULL AND s."ExpiresAt">now() AND c."IsEnabled" AND NOT u."IsArchived"
            """, connection);
        command.Parameters.AddWithValue("id", id); command.Parameters.AddWithValue("session", session);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? new(reader.GetGuid(0).ToString(), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3), reader.GetBoolean(4)) : null;
    }

    public async Task Logout(HttpContext context)
    {
        if (Guid.TryParse(context.User.FindFirstValue("sid"), out var session))
        {
            await using var connection = await source.OpenConnectionAsync();
            await LmsRepository.Execute(connection, null, "UPDATE \"Lms\".\"AuthSessions\" SET \"RevokedAt\"=now() WHERE \"ID\"=@id", ("id", session));
        }
        await context.SignOutAsync();
    }

    public async Task ChangePassword(ClaimsPrincipal principal, PasswordChangeRequest request)
    {
        ValidatePassword(request.NewPassword);
        var user = await GetUser(principal) ?? throw new ApiFailure(401, "Войдите заново.");
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await using var command = new NpgsqlCommand("SELECT \"PasswordHash\",\"FailedAttempts\",\"LockoutUntil\" FROM \"Lms\".\"AuthCredentials\" WHERE \"PersonID\"=@id FOR UPDATE", connection, transaction);
        command.Parameters.AddWithValue("id", Guid.Parse(user.Id));
        string hash;
        int failed;
        DateTime? locked;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync()) throw new ApiFailure(401, "Войдите заново.");
            hash = reader.GetString(0); failed = reader.GetInt32(1); locked = reader.IsDBNull(2) ? null : reader.GetDateTime(2);
        }
        if (locked > DateTime.UtcNow) throw new ApiFailure(400, "Проверка пароля временно заблокирована. Повторите позже.");
        if (request.CurrentPassword is null || request.CurrentPassword.Length > 128 || hasher.VerifyHashedPassword(user.Id, hash, request.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            await LmsRepository.Execute(connection, transaction, """
                UPDATE "Lms"."AuthCredentials" SET "FailedAttempts"=@failed,"LockoutUntil"=CASE WHEN @failed>=5 THEN now()+interval '15 minutes' ELSE NULL END WHERE "PersonID"=@id
                """, ("id", Guid.Parse(user.Id)), ("failed", locked is not null ? 1 : failed + 1));
            await transaction.CommitAsync();
            throw new ApiFailure(400, "Текущий пароль неверен. После нескольких ошибок проверка временно блокируется.");
        }
        if (request.CurrentPassword == request.NewPassword) throw new ApiFailure(400, "Новый пароль должен отличаться от текущего.");
        await SetPassword(connection, transaction, user.Id, request.NewPassword, false);
        await transaction.CommitAsync();
    }

    private async Task RequireAdmin(ClaimsPrincipal principal)
    {
        var user = await GetUser(principal) ?? throw new ApiFailure(401, "Войдите заново.");
        if (!user.IsAdmin) throw new ApiFailure(403, "Доступно только администратору.");
    }

    public async Task Provision(ClaimsPrincipal principal, string personId, PasswordSetRequest request)
    {
        ValidatePassword(request.Password);
        if (!Guid.TryParse(personId, out var id)) throw new ApiFailure(404, "Пользователь не найден.");
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await LmsRepository.Execute(connection, transaction, "SELECT pg_advisory_xact_lock(2026100802)");
        await RequireAdmin(principal);
        await using var target = new NpgsqlCommand("SELECT count(*) FROM \"Lms\".\"UserAccounts\" WHERE \"PersonID\"=@id AND NOT \"IsArchived\"", connection, transaction);
        target.Parameters.AddWithValue("id", id);
        if ((long)(await target.ExecuteScalarAsync())! == 0) throw new ApiFailure(404, "Пользователь не найден.");
        if (personId == principal.FindFirstValue("sub")) throw new ApiFailure(400, "Свой пароль изменяйте в настройках учётной записи.");
        await SetPassword(connection, transaction, id.ToString(), request.Password, request.MustChangePassword);
        await transaction.CommitAsync();
    }

    private async Task SetPassword(NpgsqlConnection connection, NpgsqlTransaction transaction, string personId, string password, bool mustChange)
    {
        await LmsRepository.Execute(connection, transaction, """
            INSERT INTO "Lms"."AuthCredentials" ("PersonID","PasswordHash","MustChangePassword") VALUES (@id,@hash,@change)
            ON CONFLICT ("PersonID") DO UPDATE SET "PasswordHash"=EXCLUDED."PasswordHash","MustChangePassword"=EXCLUDED."MustChangePassword","FailedAttempts"=0,"LockoutUntil"=NULL,"UpdatedAt"=now();
            UPDATE "Lms"."AuthSessions" SET "RevokedAt"=now() WHERE "PersonID"=@id AND "RevokedAt" IS NULL
            """, ("id", Guid.Parse(personId)), ("hash", hasher.HashPassword(personId, password)), ("change", mustChange));
    }

    public async Task SetAccess(ClaimsPrincipal principal, string personId, AccountAccessRequest request)
    {
        if (!Guid.TryParse(personId, out var id)) throw new ApiFailure(404, "Пользователь не найден.");
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await LmsRepository.Execute(connection, transaction, "SELECT pg_advisory_xact_lock(2026100802)");
        await RequireAdmin(principal);
        if (personId == principal.FindFirstValue("sub")) throw new ApiFailure(400, "Нельзя заблокировать собственную учётную запись.");
        await using var admin = new NpgsqlCommand("SELECT \"IsAdmin\" FROM \"Lms\".\"UserAccounts\" WHERE \"PersonID\"=@id AND NOT \"IsArchived\"", connection, transaction);
        admin.Parameters.AddWithValue("id", id);
        var isAdmin = await admin.ExecuteScalarAsync();
        if (isAdmin is null) throw new ApiFailure(404, "Пользователь не найден.");
        if ((bool)isAdmin && !request.Enabled) throw new ApiFailure(400, "Сначала снимите роль администратора.");
        await using var command = new NpgsqlCommand("UPDATE \"Lms\".\"AuthCredentials\" SET \"IsEnabled\"=@enabled WHERE \"PersonID\"=@id", connection, transaction);
        command.Parameters.AddWithValue("enabled", request.Enabled); command.Parameters.AddWithValue("id", id);
        if (await command.ExecuteNonQueryAsync() == 0) throw new ApiFailure(400, "Сначала задайте пользователю пароль.");
        if (!request.Enabled) await LmsRepository.Execute(connection, transaction, "UPDATE \"Lms\".\"AuthSessions\" SET \"RevokedAt\"=now() WHERE \"PersonID\"=@id AND \"RevokedAt\" IS NULL", ("id", id));
        await transaction.CommitAsync();
    }
}
