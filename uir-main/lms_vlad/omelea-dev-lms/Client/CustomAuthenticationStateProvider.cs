using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;


public class TokenProvider
{
    private string _token;

    public void SetToken(string token)
    {
        _token = token;
    }

    public string GetToken()
    {
        return _token;
    }
}

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;


    public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var expiry = GetExpiryFromToken(token);
        
        // Check if token is expired
        if (!expiry.HasValue || expiry.Value <= DateTime.UtcNow)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Check if token is about to expire (within 1 hour) and try to refresh
        if (expiry.Value <= DateTime.UtcNow.AddHours(1))
        {
            try
            {
                var refreshedToken = await RefreshTokenAsync(token);
                if (!string.IsNullOrEmpty(refreshedToken))
                {
                    token = refreshedToken;
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
                }
            }
            catch
            {
                // If refresh fails, continue with existing token if still valid
            }
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        return await Task.FromResult(new AuthenticationState(user));
    }

    private async Task<string?> RefreshTokenAsync(string currentToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentToken);
            
            var response = await _httpClient.PostAsync("adm_refresh", null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch
        {
            // Refresh failed, return null to use existing token
        }
        return null;
    }

    private DateTime? GetExpiryFromToken(string token)
    {
        var payload = token.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null && keyValuePairs.TryGetValue("exp", out var exp))
        {
            if (exp is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                var secondsSinceEpoch = jsonElement.GetInt64();
                var expiry = DateTimeOffset.FromUnixTimeSeconds(secondsSinceEpoch).UtcDateTime;
                return expiry;
            }
            else if (exp is long expLong)
            {
                var expiry = DateTimeOffset.FromUnixTimeSeconds(expLong).UtcDateTime;
                return expiry;
            }
            else if (long.TryParse(exp.ToString(), out var expParsed))
            {
                var expiry = DateTimeOffset.FromUnixTimeSeconds(expParsed).UtcDateTime;
                return expiry;
            }
        }

        return null;
    }

    public void NotifyUserAuthentication(string token)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);


        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles);

        if (roles != null)
        {
            if (roles.ToString().Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                foreach (var parsedRole in parsedRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
            }

            keyValuePairs.Remove(ClaimTypes.Role);
        }

        claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }
}