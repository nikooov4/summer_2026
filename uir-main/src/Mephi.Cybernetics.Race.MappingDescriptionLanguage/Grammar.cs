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


namespace Mephi.Cybernetics.Race.MappingDescriptionLanguage
{

    internal class TokenConsts
    {
        public const string numberLiteral = "numberLiteral";
        public const string identifier = "identifier";
        public const string propertyAccess = "propertyAccess";
        public const string generalPropertyAccess = "generalPropertyAccess";
        public const string identifierPropertyAccess = "identifierPropertyAccess";
        public const string sourcePropertyAccess = "sourcePropertyAccess";
        public const string destPropertyAccess = "destPropertyAccess";
        public const string comparisonOperator = "comparisonOperator";
        public const string comparison = "comparison";

        public const string assignment = "assignment";
        public const string objectCreation = "objectCreation";
        public const string keyValuePairs = "keyValuePairs";
        public const string keyValue = "keyValue";
        public const string rightPart = "rightPart";

        public const string functionCall = "functionCall";
        public const string lambdaExpression = "lambdaExpression";
        public const string selectFunction = "selectFunction";
        public const string transposeFunction = "transposeFunction";
        public const string filterFunction = "filterFunction";
        public const string countFunction = "countFunction";

        public const string sourceKeyword = "@in";
        public const string destinationKeyword = "@out";
        public const string selectKeyword = "select";
        public const string transposeKeyword = "transpose";
        public const string filterKeyword = "filter";
        public const string countKeyword = "count";

        public const string equalsOperator = "==";
        public const string notEqualsOperator = "!=";
    }
    
    public class MappingDSLGrammar : Grammar
    {
        public MappingDSLGrammar() : base() 
        {
            NonTerminal assignment = new NonTerminal(TokenConsts.assignment);
            var numberLiteral = new NumberLiteral(TokenConsts.numberLiteral);
            numberLiteral.DefaultIntTypes = [TypeCode.Int32, TypeCode.Int64];
            numberLiteral.DefaultFloatType = TypeCode.Double;

            var identifier = new IdentifierTerminal(TokenConsts.identifier);
            var generalPropertyAccess = new NonTerminal(TokenConsts.generalPropertyAccess);
            var identifierPropertyAccess = new NonTerminal(TokenConsts.identifierPropertyAccess);
            var sourcePropertyAccess = new NonTerminal(TokenConsts.sourcePropertyAccess);
            var destPropertyAccess = new NonTerminal(TokenConsts.destPropertyAccess);
            var comparisonOperator = new NonTerminal(TokenConsts.comparisonOperator);

            var objectCreation = new NonTerminal(TokenConsts.objectCreation);
            var keyValuePairs = new NonTerminal(TokenConsts.keyValuePairs);
            var keyValue = new NonTerminal(TokenConsts.keyValue);
            var rightPart = new NonTerminal(TokenConsts.rightPart);

            var functionCall = new NonTerminal(TokenConsts.functionCall);
            var lambdaExpression = new NonTerminal(TokenConsts.lambdaExpression);
            var selectFunction = new NonTerminal(TokenConsts.selectFunction);
            var transposeFunction = new NonTerminal(TokenConsts.transposeFunction);
            var filterFunction = new NonTerminal(TokenConsts.filterFunction);
            var countFunction = new NonTerminal(TokenConsts.countFunction);

            var sourceKeyword = ToTerm(TokenConsts.sourceKeyword);
            var destinationKeyword = ToTerm(TokenConsts.destinationKeyword);
            var selectKeyword = ToTerm(TokenConsts.selectKeyword);
            var transposeKeyword = ToTerm(TokenConsts.transposeKeyword);
            var filterKeyword = ToTerm(TokenConsts.filterKeyword);
            var countKeyword = ToTerm(TokenConsts.countKeyword);


            var equalsOperator = ToTerm(TokenConsts.equalsOperator);
            //var greaterOperator = ToTerm(">");
            //var greaterOrEqualOperator = ToTerm(">=");
            var notEqualsOperator = ToTerm(TokenConsts.notEqualsOperator);
            //var lessOperator = ToTerm("<");
            //var lessOrEqualOperator = ToTerm("<=");
            var comparison = new NonTerminal(TokenConsts.comparison);            


            identifierPropertyAccess.Rule = identifier | identifier + "." + identifierPropertyAccess;
            sourcePropertyAccess.Rule = sourceKeyword | sourceKeyword + "." + identifierPropertyAccess;
            destPropertyAccess.Rule = destinationKeyword | destinationKeyword + "." + identifierPropertyAccess;
            generalPropertyAccess.Rule = identifierPropertyAccess | sourcePropertyAccess | destPropertyAccess;

            keyValue.Rule = identifier + ":" + rightPart;
            keyValuePairs.Rule = MakePlusRule(keyValuePairs, ToTerm(","), keyValue);
            objectCreation.Rule = "{" + keyValuePairs + "}";
            rightPart.Rule = generalPropertyAccess | objectCreation | functionCall | numberLiteral;
            assignment.Rule = destPropertyAccess + "=" + rightPart;
            functionCall.Rule = selectFunction | transposeFunction | comparison | filterFunction | countFunction;
            lambdaExpression.Rule = identifier + ToTerm("=>") + rightPart;
            selectFunction.Rule = selectKeyword + "(" + rightPart + "," + lambdaExpression + ")";
            transposeFunction.Rule = transposeKeyword + "(" + rightPart + ")";
            filterFunction.Rule = filterKeyword + "(" + rightPart + "," + lambdaExpression + ")";
            countFunction.Rule = countKeyword + "(" + rightPart + ")";

            this.Root = assignment;


            comparisonOperator.Rule = equalsOperator | notEqualsOperator;
            comparison.Rule = rightPart + comparisonOperator + rightPart;
            MarkPunctuation("=", ".", "{", "}", ":", ",", "(", ")", ";", "..", "[", "]");
        }
    }

}

