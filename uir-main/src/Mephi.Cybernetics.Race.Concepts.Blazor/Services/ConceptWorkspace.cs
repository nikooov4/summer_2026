using System.Globalization;
using System.Text.Json;
using Mephi.Cybernetics.Race.Concepts.Blazor.Models;

namespace Mephi.Cybernetics.Race.Concepts.Blazor.Services;

public sealed class ConceptWorkspace
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public List<ConceptEditorModel> Concepts { get; } = new();
    public string? SelectedConceptId { get; private set; }
    public string? LastError { get; private set; }

    public ConceptEditorModel? SelectedConcept =>
        Concepts.FirstOrDefault(concept => concept.Id == SelectedConceptId) ?? Concepts.FirstOrDefault();

    public ConceptWorkspace()
    {
        LoadSample();
    }

    public void LoadSample()
    {
        Concepts.Clear();

        var student = AddConcept("Student");
        AddAttribute(student, "id", ConceptTypeNames.Uuid, isComputable: false);
        AddAttribute(student, "lastName", ConceptTypeNames.String, isComputable: false);
        AddAttribute(student, "firstName", ConceptTypeNames.String, isComputable: false);
        AddAttribute(student, "middleName", ConceptTypeNames.String, isComputable: false);
        AddAttribute(student, "birthDate", ConceptTypeNames.DateTime, isComputable: false);
        student.EnsureRows(3);
        SetCell(student, 0, "lastName", "Иванов");
        SetCell(student, 0, "firstName", "Иван");
        SetCell(student, 1, "lastName", "Петрова");
        SetCell(student, 1, "firstName", "Анна");
        SetCell(student, 2, "lastName", "Сидоров");
        SetCell(student, 2, "firstName", "Павел");

        var appraisal = AddConcept("AppraisalList");
        AddAttribute(appraisal, "labScore", ConceptTypeNames.Int, isComputable: false);
        AddAttribute(appraisal, "examScore", ConceptTypeNames.Int, isComputable: false);
        AddAttribute(appraisal, "totalScore", ConceptTypeNames.Int, isComputable: true, "labScore + examScore");
        appraisal.EnsureRows(3);
        SetCell(appraisal, 0, "labScore", "35");
        SetCell(appraisal, 0, "examScore", "55");
        SetCell(appraisal, 1, "labScore", "30");
        SetCell(appraisal, 1, "examScore", "48");
        SetCell(appraisal, 2, "labScore", "40");
        SetCell(appraisal, 2, "examScore", "50");

        SelectConcept(appraisal.Id);
        ClearError();
    }

    public ConceptEditorModel AddConcept(string name)
    {
        var normalizedName = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidOperationException("Название концепта не может быть пустым.");
        }

        if (Concepts.Any(concept => concept.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Концепт \"{normalizedName}\" уже существует.");
        }

        var concept = new ConceptEditorModel { Name = normalizedName };
        Concepts.Add(concept);
        SelectedConceptId = concept.Id;
        return concept;
    }

    public bool RemoveConcept(string conceptId)
    {
        var concept = Concepts.FirstOrDefault(item => item.Id == conceptId);
        if (concept is null)
        {
            return false;
        }

        Concepts.Remove(concept);
        SelectedConceptId = Concepts.FirstOrDefault()?.Id;
        return true;
    }

    public void UpdateConceptName(ConceptEditorModel concept, string name)
    {
        var normalizedName = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidOperationException("Название концепта не может быть пустым.");
        }

        var hasDuplicate = Concepts.Any(item =>
            item.Id != concept.Id &&
            item.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase));

        if (hasDuplicate)
        {
            throw new InvalidOperationException($"Концепт \"{normalizedName}\" уже существует.");
        }

        concept.Name = normalizedName;
    }

    public void SelectConcept(string conceptId)
    {
        if (Concepts.Any(concept => concept.Id == conceptId))
        {
            SelectedConceptId = conceptId;
        }
    }

    public AttributeEditorModel AddAttribute(
        ConceptEditorModel concept,
        string name,
        string type,
        bool isComputable,
        string? expression = null)
    {
        var normalizedName = NormalizeName(name);
        ValidateAttribute(concept, normalizedName, type, isComputable, expression);

        var attribute = new AttributeEditorModel
        {
            Name = normalizedName,
            Type = type,
            IsComputable = isComputable,
            Expression = isComputable ? NormalizeNullable(expression) : null
        };

        concept.Attributes.Add(attribute);
        foreach (var row in concept.Rows)
        {
            row.Values.TryAdd(attribute.Id, null);
        }

        return attribute;
    }

    public void UpdateAttribute(
        ConceptEditorModel concept,
        AttributeEditorModel attribute,
        string name,
        string type,
        bool isComputable,
        string? expression)
    {
        var normalizedName = NormalizeName(name);
        ValidateAttribute(concept, normalizedName, type, isComputable, expression, attribute.Id);

        if (!attribute.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase))
        {
            EnsureAttributeCanBeRemovedOrRenamed(concept, attribute);
        }

        attribute.Name = normalizedName;
        attribute.Type = type;
        attribute.IsComputable = isComputable;
        attribute.Expression = isComputable ? NormalizeNullable(expression) : null;
    }

    public bool RemoveAttribute(ConceptEditorModel concept, string attributeId)
    {
        var attribute = concept.Attributes.FirstOrDefault(item => item.Id == attributeId);
        if (attribute is null)
        {
            return false;
        }

        EnsureAttributeCanBeRemovedOrRenamed(concept, attribute);

        concept.Attributes.Remove(attribute);
        foreach (var row in concept.Rows)
        {
            row.Values.Remove(attributeId);
        }

        return true;
    }

    public void AddRow(ConceptEditorModel concept)
    {
        var row = new ConceptRowModel();
        foreach (var attribute in concept.Attributes)
        {
            row.Values.TryAdd(attribute.Id, null);
        }

        concept.Rows.Add(row);
    }

    public bool RemoveRow(ConceptEditorModel concept, string rowId)
    {
        var row = concept.Rows.FirstOrDefault(item => item.Id == rowId);
        return row is not null && concept.Rows.Remove(row);
    }

    public void SetCell(ConceptEditorModel concept, int rowIndex, string attributeName, string? value)
    {
        var attribute = concept.Attributes.FirstOrDefault(item => item.Name == attributeName);
        if (attribute is null || rowIndex < 0 || rowIndex >= concept.Rows.Count)
        {
            return;
        }

        concept.Rows[rowIndex].SetValue(attribute.Id, value);
    }

    public string GetComputedCellText(
        ConceptEditorModel concept,
        int rowIndex,
        AttributeEditorModel attribute)
    {
        var result = EvaluateAttribute(concept, rowIndex, attribute, new HashSet<string>());

        if (!result.Success)
        {
            return $"! {result.Error}";
        }

        if (result.Value is null)
        {
            return string.Empty;
        }

        return FormatComputedValue(attribute, result.Value.Value);
    }

    public string ExportJson()
    {
        return JsonSerializer.Serialize(ToDto(), JsonOptions);
    }

    public bool TryImportJson(string json)
    {
        try
        {
            var dto = JsonSerializer.Deserialize<ConceptDefCollectionDto>(json, JsonOptions);
            if (dto is null)
            {
                LastError = "JSON пустой или не соответствует формату.";
                return false;
            }

            ReplaceFromDto(dto);
            ClearError();
            return true;
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            LastError = exception.Message;
            return false;
        }
    }

    public void ClearError()
    {
        LastError = null;
    }

    public IReadOnlyList<string> ValidateCurrentModel()
    {
        var errors = new List<string>();
        var conceptNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var concept in Concepts)
        {
            if (string.IsNullOrWhiteSpace(concept.Name))
            {
                errors.Add("Найден концепт без названия.");
                continue;
            }

            if (!conceptNames.Add(concept.Name))
            {
                errors.Add($"Концепт \"{concept.Name}\" повторяется.");
            }

            var attributeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var attribute in concept.Attributes)
            {
                if (string.IsNullOrWhiteSpace(attribute.Name))
                {
                    errors.Add($"В концепте \"{concept.Name}\" найден столбец без названия.");
                    continue;
                }

                if (!attributeNames.Add(attribute.Name))
                {
                    errors.Add($"В концепте \"{concept.Name}\" столбец \"{attribute.Name}\" повторяется.");
                }

                if (!ConceptTypeNames.All.Contains(attribute.Type))
                {
                    errors.Add($"У столбца \"{concept.Name}.{attribute.Name}\" неизвестный тип \"{attribute.Type}\".");
                }

                if (attribute.IsComputable && string.IsNullOrWhiteSpace(attribute.Expression))
                {
                    errors.Add($"У вычисляемого столбца \"{concept.Name}.{attribute.Name}\" нет формулы.");
                }

                if (attribute.IsComputable && !string.IsNullOrWhiteSpace(attribute.Expression))
                {
                    var analysis = FormulaEvaluator.Analyze(attribute.Expression);
                    if (!analysis.Success)
                    {
                        errors.Add($"Формула \"{concept.Name}.{attribute.Name}\" записана неверно: {analysis.Error}");
                        continue;
                    }

                    foreach (var dependency in analysis.Dependencies)
                    {
                        if (ResolveDependencyDefinition(concept, dependency.Path) is null)
                        {
                            errors.Add($"Формула \"{concept.Name}.{attribute.Name}\" ссылается на несуществующий столбец \"{dependency.DisplayPath}\".");
                        }
                    }
                }
            }
        }

        return errors;
    }

    private ConceptDefCollectionDto ToDto()
    {
        return new ConceptDefCollectionDto
        {
            Concepts = Concepts
                .Select(concept => new ConceptDefDto
                {
                    Name = concept.Name,
                    Attributes = concept.Attributes
                        .Select(attribute => new AttributeDefDto
                        {
                            Name = attribute.Name,
                            Type = attribute.Type,
                            IsComputable = attribute.IsComputable,
                            Expression = attribute.IsComputable ? NormalizeNullable(attribute.Expression) : null
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private void ReplaceFromDto(ConceptDefCollectionDto dto)
    {
        Concepts.Clear();

        foreach (var conceptDto in dto.Concepts)
        {
            var concept = AddConcept(conceptDto.Name);

            foreach (var attributeDto in conceptDto.Attributes)
            {
                AddAttribute(
                    concept,
                    attributeDto.Name,
                    attributeDto.Type,
                    attributeDto.IsComputable,
                    attributeDto.Expression);
            }

            concept.EnsureRows(3);
        }

        SelectedConceptId = Concepts.FirstOrDefault()?.Id;
    }

    private void ValidateAttribute(
        ConceptEditorModel concept,
        string name,
        string type,
        bool isComputable,
        string? expression,
        string? currentAttributeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Название столбца не может быть пустым.");
        }

        if (!ConceptTypeNames.All.Contains(type))
        {
            throw new InvalidOperationException($"Тип \"{type}\" не поддерживается.");
        }

        var hasDuplicate = concept.Attributes.Any(attribute =>
            attribute.Id != currentAttributeId &&
            attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (hasDuplicate)
        {
            throw new InvalidOperationException($"Столбец \"{name}\" уже есть в концепте \"{concept.Name}\".");
        }

        if (isComputable && string.IsNullOrWhiteSpace(expression))
        {
            throw new InvalidOperationException("Для вычисляемого столбца нужна формула.");
        }

        if (isComputable && !string.IsNullOrWhiteSpace(expression))
        {
            var analysis = FormulaEvaluator.Analyze(expression);
            if (!analysis.Success)
            {
                throw new InvalidOperationException($"Формула записана неверно: {analysis.Error}");
            }

            foreach (var dependency in analysis.Dependencies)
            {
                if (IsSelfReference(concept, dependency, name, currentAttributeId))
                {
                    throw new InvalidOperationException("Столбец не может ссылаться в формуле сам на себя.");
                }

                if (ResolveDependencyDefinition(concept, dependency.Path) is null)
                {
                    throw new InvalidOperationException($"Формула ссылается на несуществующий столбец \"{dependency.DisplayPath}\".");
                }
            }
        }
    }

    private void EnsureAttributeCanBeRemovedOrRenamed(
        ConceptEditorModel concept,
        AttributeEditorModel attribute)
    {
        var dependents = GetDependentComputedAttributes(concept, attribute).ToList();
        if (dependents.Count == 0)
        {
            return;
        }

        var dependentNames = string.Join(", ", dependents.Select(item => $"{item.Concept.Name}.{item.Attribute.Name}"));
        throw new InvalidOperationException(
            $"Нельзя удалить или переименовать столбец \"{concept.Name}.{attribute.Name}\": он используется в формулах {dependentNames}.");
    }

    private IEnumerable<(ConceptEditorModel Concept, AttributeEditorModel Attribute)> GetDependentComputedAttributes(
        ConceptEditorModel targetConcept,
        AttributeEditorModel targetAttribute)
    {
        foreach (var concept in Concepts)
        {
            foreach (var attribute in concept.Attributes.Where(item => item.IsComputable))
            {
                var analysis = FormulaEvaluator.Analyze(attribute.Expression);
                if (!analysis.Success)
                {
                    continue;
                }

                var referencesTarget = analysis.Dependencies.Any(dependency =>
                    ReferencesAttribute(concept, dependency, targetConcept, targetAttribute));

                if (referencesTarget)
                {
                    yield return (concept, attribute);
                }
            }
        }
    }

    private FormulaEvaluationResult EvaluateAttribute(
        ConceptEditorModel concept,
        int rowIndex,
        AttributeEditorModel attribute,
        HashSet<string> visiting)
    {
        var visitKey = $"{concept.Id}:{attribute.Id}";
        if (!visiting.Add(visitKey))
        {
            return FormulaEvaluationResult.FromError("Обнаружена циклическая зависимость.");
        }

        try
        {
            if (!attribute.IsComputable)
            {
                var row = concept.Rows.ElementAtOrDefault(rowIndex);
                var rawValue = row?.GetValue(attribute.Id);
                return ParseNumericCellValue(concept, attribute, rawValue);
            }

            return FormulaEvaluator.Evaluate(
                attribute.Expression,
                dependencyPath => ResolveDependencyValue(concept, rowIndex, dependencyPath, visiting));
        }
        finally
        {
            visiting.Remove(visitKey);
        }
    }

    private FormulaEvaluationResult ResolveDependencyValue(
        ConceptEditorModel ownerConcept,
        int rowIndex,
        IReadOnlyList<string> dependencyPath,
        HashSet<string> visiting)
    {
        var resolved = ResolveDependencyDefinition(ownerConcept, dependencyPath);
        if (resolved is null)
        {
            return FormulaEvaluationResult.FromError(
                $"Не найден столбец \"{string.Join(".", dependencyPath)}\".");
        }

        return EvaluateAttribute(resolved.Value.Concept, rowIndex, resolved.Value.Attribute, visiting);
    }

    private FormulaEvaluationResult ParseNumericCellValue(
        ConceptEditorModel concept,
        AttributeEditorModel attribute,
        string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return FormulaEvaluationResult.FromValue(null);
        }

        if (attribute.Type is not (ConceptTypeNames.Int or ConceptTypeNames.Float))
        {
            return FormulaEvaluationResult.FromError(
                $"Столбец \"{concept.Name}.{attribute.Name}\" не числовой.");
        }

        if (TryParseNumber(rawValue, out var number))
        {
            return FormulaEvaluationResult.FromValue(number);
        }

        return FormulaEvaluationResult.FromError(
            $"Значение \"{rawValue}\" в столбце \"{concept.Name}.{attribute.Name}\" не похоже на число.");
    }

    private (ConceptEditorModel Concept, AttributeEditorModel Attribute)? ResolveDependencyDefinition(
        ConceptEditorModel ownerConcept,
        IReadOnlyList<string> dependencyPath)
    {
        if (dependencyPath.Count == 1)
        {
            var attribute = FindAttribute(ownerConcept, dependencyPath[0]);
            return attribute is null ? null : (ownerConcept, attribute);
        }

        if (dependencyPath.Count == 2)
        {
            var concept = Concepts.FirstOrDefault(item =>
                item.Name.Equals(dependencyPath[0], StringComparison.OrdinalIgnoreCase));
            var attribute = concept is null ? null : FindAttribute(concept, dependencyPath[1]);
            return concept is null || attribute is null ? null : (concept, attribute);
        }

        return null;
    }

    private static bool ReferencesAttribute(
        ConceptEditorModel ownerConcept,
        FormulaDependency dependency,
        ConceptEditorModel targetConcept,
        AttributeEditorModel targetAttribute)
    {
        var path = dependency.Path;
        if (path.Count == 1)
        {
            return ownerConcept.Id == targetConcept.Id &&
                path[0].Equals(targetAttribute.Name, StringComparison.OrdinalIgnoreCase);
        }

        if (path.Count == 2)
        {
            return path[0].Equals(targetConcept.Name, StringComparison.OrdinalIgnoreCase) &&
                path[1].Equals(targetAttribute.Name, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsSelfReference(
        ConceptEditorModel concept,
        FormulaDependency dependency,
        string attributeName,
        string? currentAttributeId)
    {
        var path = dependency.Path;
        if (path.Count == 1 && path[0].Equals(attributeName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (path.Count == 2 &&
            path[0].Equals(concept.Name, StringComparison.OrdinalIgnoreCase) &&
            path[1].Equals(attributeName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (currentAttributeId is null)
        {
            return false;
        }

        var referencedAttribute = path.Count switch
        {
            1 => FindAttribute(concept, path[0]),
            2 when path[0].Equals(concept.Name, StringComparison.OrdinalIgnoreCase)
                => FindAttribute(concept, path[1]),
            _ => null
        };

        return referencedAttribute?.Id == currentAttributeId;
    }

    private static AttributeEditorModel? FindAttribute(ConceptEditorModel concept, string attributeName)
    {
        return concept.Attributes.FirstOrDefault(item =>
            item.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryParseNumber(string rawValue, out double value)
    {
        return double.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value) ||
            double.TryParse(rawValue, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
    }

    private static string FormatComputedValue(AttributeEditorModel attribute, double value)
    {
        var format = attribute.Type == ConceptTypeNames.Int && Math.Abs(value % 1) < 0.000001
            ? "0"
            : "0.####";

        return value.ToString(format, CultureInfo.CurrentCulture);
    }

    private static string NormalizeName(string value)
    {
        return value.Trim();
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
