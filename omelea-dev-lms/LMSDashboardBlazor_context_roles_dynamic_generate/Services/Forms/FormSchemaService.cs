using System.Net.Http.Json;
using LMSDashboardBlazor.Models.Forms;

namespace LMSDashboardBlazor.Services.Forms;

public sealed class FormSchemaService
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, SemanticFormSchema> _cache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyList<FormSchemaCatalogItem> Catalog = new List<FormSchemaCatalogItem>
    {
        new()
        {
            Id = "course-passport",
            Title = "Паспорт дисциплины",
            //Description = "Форма описания дисциплины с семантическими атрибутами, составом курса и учебной нагрузкой.",
            EntityName = "Course",
            IconCssClass = "fas fa-book-open",
            SchemaPath = "form-schemas/course-passport.schema.json"
        },
        new()
        {
            Id = "assignment-review",
            Title = "Проверка задания",
            //Description = "Форма сопровождения, проверки и оценки учебного задания с разграничением доступа по роли.",
            EntityName = "Assignment",
            IconCssClass = "fas fa-list-check",
            SchemaPath = "form-schemas/assignment-review.schema.json"
        },
        new()
        {
            Id = "grade-journal",
            Title = "Журнал оценивания",
            //Description = "Гибкая форма журнала с вычисляемыми и семантическими полями для преподавателя и ассистента.",
            EntityName = "GradeJournal",
            IconCssClass = "fas fa-clipboard-check",
            SchemaPath = "form-schemas/grade-journal.schema.json"
        }
    };

    public FormSchemaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public IReadOnlyList<FormSchemaCatalogItem> GetCatalog() => Catalog;

    public async Task<SemanticFormSchema?> GetSchemaAsync(string schemaId, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(schemaId, out var cached))
        {
            return cached;
        }

        var catalogItem = Catalog.FirstOrDefault(item => string.Equals(item.Id, schemaId, StringComparison.OrdinalIgnoreCase));
        if (catalogItem is null)
        {
            return null;
        }

        var schema = await _httpClient.GetFromJsonAsync<SemanticFormSchema>(catalogItem.SchemaPath, cancellationToken);
        if (schema is not null)
        {
            _cache[schemaId] = schema;
        }

        return schema;
    }
}
