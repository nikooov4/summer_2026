using System;
using Irony;
using Irony.Parsing;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Xml.Linq;
using System.Reflection.Metadata;
using System.Diagnostics;
using System.Net.Sockets;
using System.Linq;
//TODO: Переместить в Omelea?

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{

    internal static class ConceptDSLTokenNames
    {
        public const string program = "program";
        public const string conceptDef = "conceptDef";
        public const string relationshipDef = "relationshipDef";
        public const string attributeDef = "attributeDef";
        public const string computedDef = "computedDef";
        public const string relatedConceptList = "relatedConceptList";
        public const string relatedConcept = "relatedConcept";
        public const string type = "type";
        public const string annotation = "annotation";
        public const string expression = "expression";
        public const string dependencies = "dependencies";
        public const string attributeDefList = "attributeDefList";
        public const string uintLiteral = "uintLiteral";
        public const string cardinality = "cardinality";
        public const string definition = "definition";
        public const string relDefBodyItem = "relDefBodyItem";
        // public const string cardinalityAny = "*";
        public const string typeName = "typeName";
        public const string binOp = "binOp";
        public const string term = "term";
        public const string binOpExpr = "binOpExpr";
        public const string numberLiteral = "numberLiteral";
        public const string identifier = "identifier";
        public const string attribute = "attribute";
        public const string AttributeDefExpr = "AttributeDefExpr";
    }

    public class ConceptMappingDSLGrammar : Grammar
    {
        protected NonTerminal _expression = new NonTerminal(ConceptDSLTokenNames.expression);
        public ConceptMappingDSLGrammar()
        {
            var numberLiteral = new NumberLiteral(ConceptDSLTokenNames.numberLiteral);
            numberLiteral.DefaultIntTypes = [TypeCode.Int32, TypeCode.Int64];
            numberLiteral.DefaultFloatType = TypeCode.Double;
            var identifier = new IdentifierTerminal(ConceptDSLTokenNames.identifier);
            var program = new NonTerminal(ConceptDSLTokenNames.program);
            var uintLiteral = new NumberLiteral(ConceptDSLTokenNames.uintLiteral, NumberOptions.IntOnly);
            var conceptDef = new NonTerminal(ConceptDSLTokenNames.conceptDef);
            var relationshipDef = new NonTerminal(ConceptDSLTokenNames.relationshipDef);
            var attributeDef = new NonTerminal(ConceptDSLTokenNames.attributeDef);
            // var relatedConceptList = new NonTerminal(ConceptDSLTokenNames.relatedConceptList);
            // var relatedConcept = new NonTerminal(ConceptDSLTokenNames.relatedConcept);
            var basicType = new NonTerminal(ConceptDSLTokenNames.type);
            var annotation = new NonTerminal(ConceptDSLTokenNames.annotation);
            
            var attributeDefExpr = new NonTerminal(ConceptDSLTokenNames.AttributeDefExpr);
            var dependencies = new NonTerminal(ConceptDSLTokenNames.dependencies);
            // var cardinality = new NonTerminal(ConceptDSLTokenNames.cardinality);
            var attributeDefList = new NonTerminal(ConceptDSLTokenNames.attributeDefList);
            var binOp = new NonTerminal(ConceptDSLTokenNames.binOp);
            var typeName = new IdentifierTerminal(ConceptDSLTokenNames.typeName);
            var definition = new NonTerminal(ConceptDSLTokenNames.definition);
            // var cardinalityAny = ToTerm(ConceptDSLTokenNames.cardinalityAny);
            var binOpExpr = new NonTerminal(ConceptDSLTokenNames.binOpExpr);
            var attribute = new NonTerminal(ConceptDSLTokenNames.attribute);

            MarkTransient(definition, basicType, binOp, _expression);

            program.Rule = MakePlusRule(program, definition);
            attributeDefList.Rule = MakeStarRule(attributeDefList, ToTerm(","), attributeDef);
            // relatedConceptList.Rule = MakeStarRule(relatedConceptList, ToTerm(","), relatedConcept);
            definition.Rule = conceptDef;

            conceptDef.Rule =
                "concept" + identifier +
                // "(" + relatedConceptList + ")" +
                "{" + attributeDefList + "}";

            attributeDef.Rule =
                identifier + ":" + basicType |
                identifier + ":" + basicType + "=>" + attributeDefExpr;
            attributeDefExpr.Rule = _expression;

            // relatedConcept.Rule =
            //     typeName + "[" + cardinality + "]" |
            //     typeName;

            basicType.Rule =
                ToTerm("Int", ConceptDSLTokenNames.typeName) |
                ToTerm("String", ConceptDSLTokenNames.typeName) |
                ToTerm("Float", ConceptDSLTokenNames.typeName);

            // cardinality.Rule =
            //     uintLiteral + ".." + uintLiteral |
            //     uintLiteral + ".." + cardinalityAny;

            _expression.Rule = binOpExpr | attribute | numberLiteral | "(" + _expression + ")";
            binOpExpr.Rule = _expression + binOp + _expression;
            attribute.Rule = MakeStarRule(attribute, ToTerm("."), identifier);
            binOp.Rule = ToTerm("+") | "-" | "*" | "/";

            RegisterOperators(1, "+", "-");
            RegisterOperators(2, "*", "/");


            this.Root = program;
            MarkPunctuation("=", ".", "{", "}", ":", ",", ";", "..", "[", "]", "(", ")",
            "concept", "rel", "=>");
        }
    }

    public class ConceptMappingExpressionGrammar : ConceptMappingDSLGrammar
    {
        public ConceptMappingExpressionGrammar() : base()
        {
            this.Root = _expression;
        }
    }

}

