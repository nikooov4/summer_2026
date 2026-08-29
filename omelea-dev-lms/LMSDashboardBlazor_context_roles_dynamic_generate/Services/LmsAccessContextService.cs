using LMSDashboardBlazor.Models;
using LMSDashboardBlazor.Services.Admin;

namespace LMSDashboardBlazor.Services;

public class LmsAccessContextService
{
    private readonly LmsCatalogService catalogService;

    public LmsAccessContextService(LmsCatalogService catalogService)
    {
        this.catalogService = catalogService;
    }

    public IReadOnlyList<CourseAccessContext> GetAccessContexts(string? username)
    {
        return catalogService.GetAccessContexts(username);
    }
}
