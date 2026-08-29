using System.Text.Json.Serialization;

namespace Mephi.Cybernetics.Race.Concepts.Blazor.Models;

public sealed class ConceptEditorModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public List<AttributeEditorModel> Attributes { get; } = new();
    public List<ConceptRowModel> Rows { get; } = new();

    public int WritableAttributeCount => Attributes.Count(attribute => !attribute.IsComputable);
    public int ComputableAttributeCount => Attributes.Count(attribute => attribute.IsComputable);

    public void EnsureRows(int count)
    {
        while (Rows.Count < count)
        {
            Rows.Add(new ConceptRowModel());
        }
    }
}

public sealed class AttributeEditorModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = ConceptTypeNames.String;
    public bool IsComputable { get; set; }
    public string? Expression { get; set; }
}

public sealed class ConceptRowModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public Dictionary<string, string?> Values { get; } = new();

    public string? GetValue(string attributeId)
    {
        return Values.TryGetValue(attributeId, out var value) ? value : null;
    }

    public void SetValue(string attributeId, string? value)
    {
        Values[attributeId] = string.IsNullOrWhiteSpace(value) ? null : value;
    }
}

public static class ConceptTypeNames
{
    public const string Int = "Int";
    public const string String = "String";
    public const string Float = "Float";
    public const string Uuid = "UUID";
    public const string DateTime = "DateTime";

    public static readonly IReadOnlyList<string> All =
    [
        Int,
        String,
        Float,
        Uuid,
        DateTime
    ];
}

public sealed class ConceptDefCollectionDto
{
    public List<ConceptDefDto> Concepts { get; set; } = new();
}

public sealed class ConceptDefDto
{
    public string Name { get; set; } = string.Empty;
    public List<AttributeDefDto> Attributes { get; set; } = new();
}

public sealed class AttributeDefDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = ConceptTypeNames.String;
    public bool IsComputable { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Expression { get; set; }
}
