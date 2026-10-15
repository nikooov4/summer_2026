using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LMSDashboardBlazor.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace LMSDashboardBlazor.Services;

public sealed class AuthSessionService(HttpClient http, IConfiguration configuration)
{
    public event Action? Changed;
    public SessionUser? CurrentUser { get; private set; }
    private Task? loading;
    private bool initialized;
    public Uri Endpoint(string path) => new(new Uri((configuration["LmsApi:BaseUrl"] ?? "http://localhost:8085").TrimEnd('/') + "/"), path);

    public async Task RefreshAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint("api/auth/me"));
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        using var response = await http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) CurrentUser = null;
        else { await EnsureSuccess(response); CurrentUser = await response.Content.ReadFromJsonAsync<SessionUser>(); }
        initialized = true;
    }

    private async Task Load()
    {
        if (initialized) return;
        loading ??= RefreshAsync();
        try { await loading; } finally { loading = null; }
    }
    public async Task<bool> IsAuthenticatedAsync() { await Load(); return CurrentUser is not null; }
    public async Task<string?> GetUsernameAsync() { await Load(); return CurrentUser?.Username; }

    public async Task LoginAsync(string username, string password)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/auth/login", new LoginRequest(username, password));
        await AcceptLogin(response);
    }

    public async Task<bool> IsDemoLoginEnabledAsync()
    {
        using var response = await SendAsync(HttpMethod.Get, "api/auth/options");
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<AuthOptionsResponse>())?.DemoLoginEnabled == true;
    }

    public async Task DemoLoginAsync(string username)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/auth/demo/" + Uri.EscapeDataString(username));
        await AcceptLogin(response);
    }

    private async Task AcceptLogin(HttpResponseMessage response)
    {
        await EnsureSuccess(response);
        CurrentUser = await response.Content.ReadFromJsonAsync<SessionUser>(); initialized = true;
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        using var response = await SendAsync(HttpMethod.Post, "api/auth/logout");
        if (response.StatusCode != HttpStatusCode.Unauthorized) await EnsureSuccess(response);
        CurrentUser = null; initialized = true; Changed?.Invoke();
    }

    public async Task ChangePasswordAsync(string current, string password)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/auth/change-password", new PasswordChangeRequest(current, password));
        await EnsureSuccess(response);
        CurrentUser = null; initialized = true; Changed?.Invoke();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null, CancellationToken cancellation = default)
    {
        using var request = new HttpRequestMessage(method, Endpoint(path));
        if (body is not null) request.Content = JsonContent.Create(body, body.GetType());
        return await SendRequestAsync(request, cancellation);
    }

    public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellation = default)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Head)
        {
            using var csrfRequest = new HttpRequestMessage(HttpMethod.Get, Endpoint("api/auth/csrf"));
            csrfRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            using var csrf = await http.SendAsync(csrfRequest, cancellation);
            await EnsureSuccess(csrf);
            var token = await csrf.Content.ReadFromJsonAsync<CsrfTokenResponse>(cancellationToken: cancellation);
            request.Headers.Add("X-CSRF-TOKEN", token?.Token ?? throw new InvalidOperationException("Не получен токен проверки запроса."));
        }
        return await http.SendAsync(request, cancellation);
    }

    public static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new SessionExpiredException();
        if ((int)response.StatusCode == 429) throw new InvalidOperationException("Слишком много попыток. Повторите позже.");
        try
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (document.RootElement.TryGetProperty("message", out var message)) throw new InvalidOperationException(message.GetString());
        }
        catch (JsonException) { }
        throw new InvalidOperationException($"Сервер вернул ошибку {(int)response.StatusCode}.");
    }
}
