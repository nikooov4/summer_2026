namespace LMSDashboardBlazor.Services;

public class AuthOptions
{
    public string AuthServiceBaseUrl { get; set; } = "http://localhost:8080";
    public string ProjectName { get; set; } = "lms-dashboard";
    public string CasLoginBaseUrl { get; set; } = "https://auth.mephi.ru/login";
}
