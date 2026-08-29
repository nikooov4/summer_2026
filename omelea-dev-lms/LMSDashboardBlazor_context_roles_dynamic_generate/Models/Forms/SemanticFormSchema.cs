namespace LMSDashboardBlazor.Models.Forms;

public sealed class SemanticFormSchema
{
    public string Id { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DefaultColumns { get; set; } = 12;
    public List<FormSectionSchema> Sections { get; set; } = new();
}

public sealed class FormSectionSchema
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Columns { get; set; } = 12;
    public List<FormFieldSchema> Fields { get; set; } = new();
}

public sealed class FormFieldSchema
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public string SemanticRole { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public string HelpText { get; set; } = string.Empty;
    public bool Required { get; set; }
    public bool ReadOnly { get; set; }
    public int Span { get; set; } = 6;
    public string? DefaultValue { get; set; }
    public List<string> VisibleForRoles { get; set; } = new();
    public List<string> EditableForRoles { get; set; } = new();
    public List<FormOptionSchema> Options { get; set; } = new();
}

public sealed class FormOptionSchema
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class FormSchemaCatalogItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string IconCssClass { get; set; } = "fas fa-window-maximize";
    public string SchemaPath { get; set; } = string.Empty;
}
