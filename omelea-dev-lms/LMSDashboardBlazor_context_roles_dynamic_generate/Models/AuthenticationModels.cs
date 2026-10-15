namespace LMSDashboardBlazor.Models;

public sealed record LoginRequest(string Username, string Password);
public sealed record SessionUser(string Id, string Username, string FullName, bool IsAdmin, bool MustChangePassword);
public sealed record PasswordChangeRequest(string CurrentPassword, string NewPassword);
public sealed record PasswordSetRequest(string Password, bool MustChangePassword = true);
public sealed record AccountAccessRequest(bool Enabled);
public sealed record CsrfTokenResponse(string Token);
public sealed record AuthOptionsResponse(bool DemoLoginEnabled);
