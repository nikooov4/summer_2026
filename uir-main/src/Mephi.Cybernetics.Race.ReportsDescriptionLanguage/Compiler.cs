using Irony.Parsing;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
//TODO: Переместить в Omelea?
//FIXME: Строка 

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{
    public class ConceptMappingDSLCompiler
    {
        public ExpressionCompiler ExprCompiler { get; } = new ExpressionCompiler();
        private ConceptDtoConverter _dtoToConceptConverter = new ConceptDtoConverter();

        public ConceptMappingDSLCompiler() { }

        public ConceptManager Compile(ConceptManager conceptManager, string code)
        {
            var grammar = new ConceptMappingDSLGrammar();
            var parser = new Parser(grammar);
            var parseTree = parser.Parse(code);
            if (parseTree.Root == null)
            {
                parseTree.ParserMessages.Where(m => m.Level == Irony.ErrorLevel.Error).ToList().ForEach(m => Console.WriteLine(m.Message));
                return null;
            }

            // try 
            // {
                var conceptsDto = ConvertParseTreeToDto(code, parseTree);
                conceptManager = _dtoToConceptConverter.AddToExisting(conceptManager, conceptsDto);
                // Console.WriteLine(JsonSerializer.Serialize(
                //     conceptsDto,
                //     new JsonSerializerOptions 
                //     { 
                //         WriteIndented = true 
                //     }
                // ));
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine(ex.Message);
            // }
            return conceptManager;
        }

        public ConceptManager Compile(string code)
        {
            ConceptManager conceptManager = new ConceptManager();
            return Compile(conceptManager, code);
        }

        private ConceptDefCollectionDto ConvertParseTreeToDto(string Source, ParseTree parseTree)
        {
            var result = new ConceptDefCollectionDto();

            // Process each concept definition in the program
            foreach (var conceptNode in parseTree.Root.ChildNodes)
            {
                if (conceptNode.Term.Name != ConceptDSLTokenNames.conceptDef)
                    continue;

                var conceptDict = conceptNode.GetChildrenDict();
                var conceptDto = new ConceptDefDto
                {
                    Name = conceptDict[ConceptDSLTokenNames.identifier].Token.ValueString
                };

                // Process attribute definitions
                var attributeList = conceptDict[ConceptDSLTokenNames.attributeDefList];
                foreach (var attrNode in attributeList.ChildNodes)
                {
                    if (attrNode.Term.Name != ConceptDSLTokenNames.attributeDef)
                        continue;

                    var attrDict = attrNode.GetChildrenDict();
                    var attrName = attrDict[ConceptDSLTokenNames.identifier].Token.ValueString;
                    var attrType = attrDict[ConceptDSLTokenNames.typeName].Token.ValueString;

                    var attrDto = new AttributeDefDto
                    {
                        Name = attrName,
                        Type = attrType,
                        IsComputable = attrDict.ContainsKey(ConceptDSLTokenNames.AttributeDefExpr)
                    };

                    // If it's a computable attribute, get the expression
                    if (attrDto.IsComputable)
                    {
                        var exprNode = attrDict[ConceptDSLTokenNames.AttributeDefExpr];
                        attrDto.Expression = Source.Substring(exprNode.Span.Location.Position, exprNode.Span.Length);
                        // attrDto.Expression = exprNode.Token.Text;
                    }

                    conceptDto.Attributes.Add(attrDto);
                }

                result.Concepts.Add(conceptDto);
            }

            return result;
        }
    }
}