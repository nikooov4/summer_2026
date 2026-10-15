namespace LMSDashboardBlazor.Services;

public sealed class SessionExpiredException() : InvalidOperationException("Сессия завершена. Войдите заново.");
