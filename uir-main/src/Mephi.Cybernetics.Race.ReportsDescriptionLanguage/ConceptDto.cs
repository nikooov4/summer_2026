//TODO: Возможно нужно добавить возможность задания своих маппингов для типов (или просто переместить это из Race в Omelea)

using System.Text.Json;

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{

    public class ConceptDtoConverter
    {
        public static Type StringToTypeMapping(string type)
        {
            switch (type)
            {
                case "Int": return typeof(int?);
                case "String": return typeof(string);
                case "Float": return typeof(double?);
                case "UUID": return typeof(Guid);
                case "DateTime": return typeof(DateTime);
            }
            throw new CompilationException($"Unknown basic type name \"{type}\"");
        }

        private static readonly Dictionary<Type, string> _typeToString = new Dictionary<Type, string>()
        {
            { typeof(int?), "Int" },
            { typeof(string), "String" },
            { typeof(double?), "Float" },
            { typeof(Guid), "UUID"},
            { typeof(DateTime), "DateTime"},
            
        };

        public static string TypeToStringMapping(Type type)
        {
            if (!_typeToString.TryGetValue(type, out var result))
            {
                throw new CompilationException($"Unknown type \"{type}\"");
            }
            return result;
        }

        public ConceptManager AddToExisting(ConceptManager conceptManager, ConceptDefCollectionDto dto)
        {
            var expressionCompiler = new ExpressionCompiler();

            // First pass: create all concepts and add non-computable attributes
            foreach (var conceptDto in dto.Concepts)
            {
                var conceptDef = conceptManager.DefineConcept(conceptDto.Name);

                // Add all attributes first as non-computable
                foreach (var attrDto in conceptDto.Attributes)
                {
                    var type = StringToTypeMapping(attrDto.Type);
                    conceptDef.AddAttribute(attrDto.Name, type);
                }
            }

            // Second pass: add computable attributes
            foreach (var conceptDto in dto.Concepts)
            {
                var conceptDef = conceptManager.GetConceptDef(conceptDto.Name);

                // Add computable attributes
                foreach (var attrDto in conceptDto.Attributes.Where(a => a.IsComputable))
                {
                    if (string.IsNullOrEmpty(attrDto.Expression))
                    {
                        throw new CompilationException($"Computable attribute {attrDto.Name} in concept {conceptDto.Name} has no expression");
                    }

                    var type = StringToTypeMapping(attrDto.Type);
                    var compiledExpr = expressionCompiler.Compile(
                        conceptDef.GetContext(),
                        attrDto.Expression
                    );
                    // if (conceptDto.Name == "Total" && attrDto.Name == "score")
                    // {
                    //     compiledExpr.Args.ForEach(x => Console.WriteLine(string.Join(".", x.Path)));
                    //     Console.WriteLine("");
                    // }
                    conceptDef.UpdateCompAttribute(attrDto.Name, type, compiledExpr);
                }
            }

            return conceptManager;
        }

        public ConceptManager DtoToDefinitions(ConceptDefCollectionDto dto)
        {
            var conceptManager = new ConceptManager();
            return AddToExisting(conceptManager, dto);
        }

        public ConceptDefCollectionDto DefinitionsToDto(ConceptManager definitions)
        {
            var result = new ConceptDefCollectionDto();

            foreach (var conceptDef in definitions.ConceptDefs.Values)
            {
                var conceptDto = new ConceptDefDto
                {
                    Name = conceptDef.Name
                };

                foreach (var attrDef in conceptDef.Attributes.Values)
                {
                    var attrDto = new AttributeDefDto
                    {
                        Name = attrDef.Name,
                        Type = TypeToStringMapping(attrDef.CSType),
                        IsComputable = attrDef is ComputableAttributeDef
                    };

                    if (attrDef is ComputableAttributeDef compAttr)
                    {
                        attrDto.Expression = compAttr.Expr.SourceString;
                    }

                    conceptDto.Attributes.Add(attrDto);
                }

                result.Concepts.Add(conceptDto);
            }

            return result;
        }

        public ApprListMarksDto ToApprListMarksDto(ConceptDef conceptDef)
        {
            var apprList = new ApprListMarksDto();
            foreach (var attr in conceptDef.Attributes.Values)
            {
                var marks = new List<MarkDto>();
                foreach (var instance in conceptDef.GetInstances())
                {
                    var studentId = (Guid)instance.Group.GetInstance("Student").GetAttributeValue("id");
                    var value = instance.GetAttributeValue(attr.Name);
                    marks.Add(new MarkDto
                    {
                        StudentId = studentId,
                        Value = value
                    });
                }
                apprList.Columns[attr.Name] = marks;
            }
            return apprList;
        }

        public void ApplyApprListMarksDto(ConceptDef conceptDef, ApprListMarksDto apprList)
        {
            foreach (var column in apprList.Columns)
            {
                var attrName = column.Key;
                var sortedMarks = column.Value.OrderBy(mark => mark.StudentId);
                foreach (var (mark, instance) in sortedMarks.Zip(conceptDef.GetInstances()))
                {
                    object? value = mark.Value;
                    Type type = instance.Definition.GetAttribute(attrName).CSType;
                    if (value is JsonElement jsonElement)
                    {
                        if (type == typeof(int) || type ==  typeof(int?))
                            value = jsonElement.GetInt32();
                        else if (type == typeof(double) || type == typeof(double?))
                            value = jsonElement.GetDouble();
                        else if (type == typeof(string))
                            value = jsonElement.GetString();
                        else if (type == typeof(Guid) || type == typeof(Guid?))
                            value = jsonElement.GetGuid();
                        else if (type == typeof(bool) || type == typeof(bool?))
                            value = jsonElement.GetBoolean();
                        else if (type == typeof(DateTime) || type == typeof(DateTime?))
                            value = jsonElement.GetDateTime();
                        else if (jsonElement.ValueKind == JsonValueKind.Null)
                            value = null;
                        else
                            throw new InvalidOperationException($"Cannot convert JsonElement to {type}");
                    }
                    instance.SetAttribute(attrName, value);
                }
            }
        }

        public List<StudentDto> ToStudentDtos(ConceptManager manager)
        {
            var studentConcept = manager.GetConceptDef("Student");
            var result = new List<StudentDto>();
            foreach (var instance in studentConcept.GetInstances())
            {
                result.Add(new StudentDto
                {
                    Id = (Guid)instance.GetAttributeValue("id"),
                    FirstName = (string?)instance.GetAttributeValue("firstName"),
                    MiddleName = (string?)instance.GetAttributeValue("middleName"),
                    LastName = (string?)instance.GetAttributeValue("lastName"),
                    BirthDate = (DateTime?)instance.GetAttributeValue("birthDate")
                });
            }
            return result;
        }

        public void ApplyStudentDtos(ConceptManager manager, List<StudentDto> students)
        {
            var studentConcept = manager.GetConceptDef("Student");
            foreach (var student in students)
            {
                var instance = studentConcept.GetInstances().FirstOrDefault(i => (Guid)i.GetAttributeValue("id") == student.Id);
                if (instance == null)
                {
                    var group = manager.CreateInstanceGroup();
                    instance = group.CreateInstance("Student");
                }
                instance.SetAttribute("id", student.Id);
                instance.SetAttribute("firstName", student.FirstName);
                instance.SetAttribute("middleName", student.MiddleName);
                instance.SetAttribute("lastName", student.LastName);
                instance.SetAttribute("birthDate", student.BirthDate);
            }
        }
    }
    public class ConceptDefCollectionDto
    {
        public List<ConceptDefDto> Concepts { get; set; } = new List<ConceptDefDto>();
    }

    public class ConceptDefDto
    {
        public string Name { get; set; }
        public List<AttributeDefDto> Attributes { get; set; } = new List<AttributeDefDto>();
    }

    public class AttributeDefDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsComputable { get; set; }
        public string? Expression { get; set; }
    }

    public class MarkDto
    {
        public Guid StudentId { get; set; }
        public object? Value { get; set; }
    }

    public class ApprListMarksDto
    {
        public Dictionary<string, List<MarkDto>> Columns { get; set; } = new Dictionary<string, List<MarkDto>>();
    }

    public class StudentDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}