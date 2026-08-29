using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace LMSDashboardBlazor.Services;

public class AuthSessionService
{
    private const string TokenKey = "lms.auth.token";
    private readonly IJSRuntime _jsRuntime;

    public event Action? Changed;

    public AuthSessionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("lmsAuth.getToken", TokenKey);
    }

    public async Task SetTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync("lmsAuth.setToken", TokenKey, token);
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("lmsAuth.removeToken", TokenKey);
        Changed?.Invoke();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<string?> GetUsernameAsync()
    {
        var token = await GetTokenAsync();
        return GetUsernameFromToken(token);
    }

    public string? GetUsernameFromToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("username", out var username))
            {
                return username.GetString();
            }

            if (document.RootElement.TryGetProperty("unique_name", out var uniqueName))
            {
                return uniqueName.GetString();
            }

            if (document.RootElement.TryGetProperty("name", out var name))
            {
                return name.GetString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
