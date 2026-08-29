using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Data;
using System.Xml.Linq;




namespace Mephi.Cybernetics.Race.MappingDescriptionLanguage
{
    public abstract class AstNode
    {
        public Type ExpressionType { get; protected set; }

        public abstract Type EvaluateType();
        public Stack<Dictionary<string, Type>> ContextTypes { get; protected set; }
        public AstNode? Parent { get; protected set; }

        public abstract string TranslateToCSharp();
    }



    public class AssignmentNode : AstNode
    {
        public GeneralPropertyAccessNode Destination { get; set; }
        public RightPartNode RightPart { get; set; }

        public AssignmentNode(Type source, Type destination)
        {
            Parent = null; 
            ContextTypes = new ([new(){{ TokenConsts.sourceKeyword, source }, {TokenConsts.destinationKeyword, destination}}]);
        }

        public override Type EvaluateType()
        {
            var destType = Destination.EvaluateType();
            ExpressionType = destType;
            var rightPartType = RightPart.EvaluateType();
            if(destType != rightPartType) {
                throw new CantConvertException(rightPartType, destType);
            }
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var sb = new StringBuilder();
            sb.Append("(@a, @b) => ");
            var leftPart = Destination.TranslateToCSharp();
            sb.Append(leftPart).Append(" = ");
            string rightPart = RightPart.TranslateToCSharp();
            sb.Append(rightPart);
            return sb.ToString();
        }
    }

    public class RightPartNode : AstNode
    {
        public GeneralPropertyAccessNode? PropertyAccess { get; set; }
        public ObjectCreationNode? ObjectCreation { get; set; }
        public FunctionCallNode? FunctionCall { get; set; }
        public NumberLiteralNode? NumberLiteral { get; set; }

        private AstNode Node => GetNode();

        public RightPartNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        private AstNode GetNode()
        {
            if (PropertyAccess != null)
                return PropertyAccess;
            else if (FunctionCall != null)
                return FunctionCall;
            else if (ObjectCreation != null)
                return ObjectCreation;
            else if (NumberLiteral != null)
                return NumberLiteral;
            else
                throw new NotImplementedException();
        }

        public override Type EvaluateType()
        {
            var node = Node;
            if (node is ObjectCreationNode)
            {
                var path = new List<AstNode>();
                var parent = Parent;
                while (parent != null)
                {
                    path.Add(parent);
                    if (parent.ExpressionType != null)
                    {
                        int asjfdl = 0;
                        var type = parent.ExpressionType;
                        foreach (var parametrizedNode in path.Where(n => n is SelectFunctionNode || n is FilterFunctionNode))
                        {
                            type = type.GetGenericArguments()[0];
                        }
                        ExpressionType = type;
                        break;
                    }
                    parent = parent.Parent;
                }
                if (ExpressionType == null)
                    throw new NotImplementedException();
            }
            ExpressionType = Node.EvaluateType();
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            return Node.TranslateToCSharp();
        }
    }

    public class GeneralPropertyAccessNode : AstNode
    {
        public string Identifier { get; set; }
        public GeneralPropertyAccessNode? NextProperty { get; set; }

        public Type IdentifierType { get; set; }
        public GeneralPropertyAccessNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        public override Type EvaluateType()
        {
            if(Parent is GeneralPropertyAccessNode accessParentNode)
            {
                var property = accessParentNode.IdentifierType.GetProperty(Identifier);
                if(property != null)
                    IdentifierType = property.PropertyType;
                else
                    throw new TypeHasNoPropertyException(accessParentNode.IdentifierType, Identifier);
            }
            else
            {
                var foundTypeDict = ContextTypes.FirstOrDefault(d => d.ContainsKey(Identifier));
                if (foundTypeDict != default)
                    IdentifierType = foundTypeDict[Identifier];
                else
                    throw new VariableNotDefiendException(Identifier);
            }
            ExpressionType = NextProperty == null ? IdentifierType : NextProperty.EvaluateType();
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var sb = new StringBuilder();
            if (Parent is GeneralPropertyAccessNode accessParentNode)
            {
                sb.Append('.').Append(Identifier);
            }
            else
            {
                var id = Identifier == TokenConsts.sourceKeyword ? "@a" : (Identifier == TokenConsts.destinationKeyword ? "@b" : Identifier);
                sb.Append(id);
            }
            return NextProperty == null ? sb.ToString() : sb.Append(NextProperty.TranslateToCSharp()).ToString();
        }
    }

    public class NameValuePair : AstNode
    {
        public string Name { get; set; }
        public RightPartNode Value { get; set; }

        public NameValuePair(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }

        public override Type EvaluateType()
        {
            var expectedProperty = Parent.ExpressionType.GetProperty(Name);
            if (expectedProperty == null)
                throw new TypeHasNoPropertyException(Parent.ExpressionType, Name);
            var expectedFieldType = expectedProperty.PropertyType;
            ExpressionType = expectedFieldType;
            var actualFieldType = Value.EvaluateType();
            if (expectedFieldType != actualFieldType)
                throw new CantConvertException(actualFieldType, expectedFieldType);
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var sb = new StringBuilder();
            sb.Append(Name).Append("=");
            var value = Value.TranslateToCSharp();
            sb.Append(value).Append(',');
            return sb.ToString();
        }
    }

    public class ObjectCreationNode : AstNode
    {
        public List<NameValuePair> KeyValuePairs { get; set; }
        public ObjectCreationNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        public override Type EvaluateType()
        {
            ExpressionType = Parent.ExpressionType;
            foreach (var pair in KeyValuePairs)
            {
                pair.EvaluateType();
            }
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var sb = new StringBuilder();

            sb.Append($"new {ExpressionType.Namespace}.{ExceptionsUtils.GetShortName(ExpressionType)}() {{ ");

            foreach (var pair in KeyValuePairs)
            {
                sb.Append(pair.TranslateToCSharp());
            }

            sb.Append(" }");
            return sb.ToString();
        }
    }

    public abstract class FunctionCallNode : AstNode
    {
    }

    public class SelectFunctionNode : FunctionCallNode
    {
        public Type InputListType { get; protected set; }
        public RightPartNode InputList { get; set; }
        public LambdaExpressionNode LambdaExpression { get; set; }

        public SelectFunctionNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        public override Type EvaluateType()
        {
            var inputListType = InputList.EvaluateType();
            InputListType = inputListType;
            var lambdaExpType = LambdaExpression.EvaluateType();
            ExpressionType = LanguageUtils.CreateListType(lambdaExpType);
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var sb = new StringBuilder();
            var inputList = InputList.TranslateToCSharp();

            sb.Append(inputList).Append(".Select(");
            var lambda = LambdaExpression.TranslateToCSharp();
            sb.Append(lambda);
            sb.Append(").ToList()");
            return sb.ToString();
        }
    }

    public class TransposeFunctionNode : FunctionCallNode
    {
        public Type InputListType { get; protected set; }
        public RightPartNode InputList { get; set; }

        public TransposeFunctionNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        public override Type EvaluateType()
        {
            var inputListType = InputList.EvaluateType();
            InputListType = inputListType;
            ExpressionType = inputListType;
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var sb = new StringBuilder();
            var inputList = InputList.TranslateToCSharp();
            sb.Append("LanguageUtils.Transpose(").Append(inputList).Append(")");
            return sb.ToString();
        }
    }

    public class FilterFunctionNode : FunctionCallNode
    {
        public Type InputListType { get; protected set; }
        public RightPartNode InputList { get; set; }
        public LambdaExpressionNode LambdaExpression { get; set; }

        public FilterFunctionNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        public override Type EvaluateType()
        {
            var inputListType = InputList.EvaluateType();
            InputListType = inputListType;
            ExpressionType = inputListType;
            LambdaExpression.EvaluateType();
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var sb = new StringBuilder();
            var inputList = InputList.TranslateToCSharp();

            sb.Append(inputList).Append(".Where(");
            var lambda = LambdaExpression.TranslateToCSharp();
            sb.Append(lambda);
            sb.Append(").ToList()");
            return sb.ToString();
        }
    }

    public class CountFunctionNode : FunctionCallNode
    {
        public Type InputListType { get; protected set; }
        public RightPartNode InputList { get; set; }

        public CountFunctionNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        public override Type EvaluateType()
        {
            var inputListType = InputList.EvaluateType();
            InputListType = inputListType;
            ExpressionType = typeof(int);
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var inputList = InputList.TranslateToCSharp();          
            return $"{inputList}.Count";
        }
    }

    public class ComparisonNode : FunctionCallNode
    {
        public string Operator { get; set; }
        public RightPartNode LeftPart { get; set; }
        public RightPartNode RightPart { get; set; }

        public ComparisonNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        public override Type EvaluateType()
        {
            ExpressionType = typeof(bool);
            LeftPart.EvaluateType();
            RightPart.EvaluateType();

            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            return $"{LeftPart.TranslateToCSharp()} {Operator} {RightPart.TranslateToCSharp()}";
        }
    }

    public class NumberLiteralNode : AstNode
    {
        public string Number { get; set; }
        public NumberLiteralNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }

        public override Type EvaluateType()
        {
            if(int.TryParse(Number, out var _))
                ExpressionType = typeof(int);
            else if(long.TryParse(Number, out var _))
                ExpressionType = typeof(long);
            else if (double.TryParse(Number, out var _))
                ExpressionType = typeof(double);
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            return Number;
        }
    }

    public class LambdaExpressionNode : AstNode
    {
        public string Parameter { get; set; }
        public RightPartNode Body { get; set; }
        public LambdaExpressionNode(AstNode parent)
        {
            Parent = parent;
            ContextTypes = parent.ContextTypes;
        }
        public override Type EvaluateType()
        {
            Type inputListType;
            if(Parent is SelectFunctionNode selectParentNode)
                inputListType = selectParentNode.InputListType.GetGenericArguments()[0];
            else if (Parent is FilterFunctionNode filterParentNode)
                inputListType = filterParentNode.InputListType.GetGenericArguments()[0];
            else
                throw new NotImplementedException();
            ContextTypes.Push(new Dictionary<string, Type>() { { Parameter, inputListType } });
            ExpressionType = Body.EvaluateType();
            ContextTypes.Pop();
            return ExpressionType;
        }

        public override string TranslateToCSharp()
        {
            var sb = new StringBuilder();
            sb.Append(Parameter).Append(" => ");
            var body = Body.TranslateToCSharp();
            sb.Append(body);
            return sb.ToString();
        }
    }

    public class Ast<S,D>
    {
        public AstNode Root { get; private set; }
        public ParseTreeNode ParseTree { get; private set; }
        public string SourceCode { get; private set; }

        public Ast(AstNode root, ParseTreeNode parseTree, string code)
        {
            Root = root;
            ParseTree = parseTree;
            SourceCode = code;
        }

        public Action<S, D> Compile()
        {
            var code = Root.TranslateToCSharp();
            var options = ScriptOptions.Default.AddReferences(typeof(S).Assembly, typeof(D).Assembly).AddImports("System.Linq", "Mephi.Cybernetics.Race.MappingDescriptionLanguage");
            var lam = CSharpScript.EvaluateAsync<Action<S, D>>(code, options).Result;
            return lam;
        }
    }

    public class AstBuilder
    {
        public Ast<S,D> BuildAst<S, D>(ParseTreeNode parseTree, string sourceCode)
        {
            var res = VisitAssignment<S, D>(parseTree);
            res.EvaluateType();
            return new Ast<S,D>(res, parseTree, sourceCode);
        }

        private AssignmentNode VisitAssignment<S, D>(ParseTreeNode node)
        {
            var assignmentNode = new AssignmentNode(typeof(S), typeof(D));

            var destinationNode = node.ChildNodes[0];
            assignmentNode.Destination = VisitGeneralPropertyAccess(destinationNode, assignmentNode);
            assignmentNode.RightPart = VisitRightPart(node.ChildNodes[1], assignmentNode);
            return assignmentNode;
        }

        private GeneralPropertyAccessNode VisitGeneralPropertyAccess(ParseTreeNode node, AstNode parent)
        {
            if (node.Term.Name == TokenConsts.generalPropertyAccess)
                return VisitGeneralPropertyAccess(node.ChildNodes[0], parent);
            var propertyAccessNode = new GeneralPropertyAccessNode(parent);
            propertyAccessNode.Identifier = node.ChildNodes[0].Token.Text;

            if (node.ChildNodes.Count > 1 && node.ChildNodes[1].Term.Name == TokenConsts.identifierPropertyAccess)
                propertyAccessNode.NextProperty = VisitGeneralPropertyAccess(node.ChildNodes[1], propertyAccessNode);

            return propertyAccessNode;
        }

        private ObjectCreationNode VisitObjectCreation(ParseTreeNode node, AstNode parent)
        {
            var objectCreationNode = new ObjectCreationNode(parent);
            objectCreationNode.KeyValuePairs = new List<NameValuePair>();

            foreach (var keyValueNode in node.ChildNodes[0].ChildNodes)
            {
                var pairNode = new NameValuePair(objectCreationNode);
                var identifierNode = keyValueNode.ChildNodes[0];
                var rightPartNode = VisitRightPart(keyValueNode.ChildNodes[1], pairNode);
                pairNode.Name = identifierNode.Token.Text;
                pairNode.Value = rightPartNode;
                objectCreationNode.KeyValuePairs.Add(pairNode);
            }

            return objectCreationNode;
        }

        private FunctionCallNode VisitFunctionCall(ParseTreeNode node, AstNode parent)
        {
            if (node.ChildNodes[0].Term.Name == TokenConsts.selectFunction)
                return VisitSelectFunction(node.ChildNodes[0], parent);
            else if (node.ChildNodes[0].Term.Name == TokenConsts.transposeFunction)
                return VisitTransposeFunction(node.ChildNodes[0], parent);
            else if (node.ChildNodes[0].Term.Name == TokenConsts.comparison)
                return VisitComparison(node.ChildNodes[0], parent);
            else if (node.ChildNodes[0].Term.Name == TokenConsts.filterFunction)
                return VisitFilterFunction(node.ChildNodes[0], parent);
            else if (node.ChildNodes[0].Term.Name == TokenConsts.countFunction)
                return VisitCountFunction(node.ChildNodes[0], parent);
            else
                throw new NotImplementedException();
        }

        private SelectFunctionNode VisitSelectFunction(ParseTreeNode node, AstNode parent)
        {
            var selectFunctionNode = new SelectFunctionNode(parent);
            selectFunctionNode.InputList = VisitRightPart(node.ChildNodes[1], selectFunctionNode);
            selectFunctionNode.LambdaExpression = VisitLambdaExpression(node.ChildNodes[2], selectFunctionNode);

            return selectFunctionNode;
        }

        private TransposeFunctionNode VisitTransposeFunction(ParseTreeNode node, AstNode parent)
        {
            var transposeFunctionNode = new TransposeFunctionNode(parent);
            transposeFunctionNode.InputList = VisitRightPart(node.ChildNodes[1], transposeFunctionNode);

            return transposeFunctionNode;
        }

        private FilterFunctionNode VisitFilterFunction(ParseTreeNode node, AstNode parent)
        {
            var filterFunctionNode = new FilterFunctionNode(parent);
            filterFunctionNode.InputList = VisitRightPart(node.ChildNodes[1], filterFunctionNode);
            filterFunctionNode.LambdaExpression = VisitLambdaExpression(node.ChildNodes[2], filterFunctionNode);

            return filterFunctionNode;
        }

        private CountFunctionNode VisitCountFunction(ParseTreeNode node, AstNode parent)
        {
            var countFunctionNode = new CountFunctionNode(parent);
            countFunctionNode.InputList = VisitRightPart(node.ChildNodes[1], countFunctionNode);

            return countFunctionNode;
        }
        

        private ComparisonNode VisitComparison(ParseTreeNode node, AstNode parent)
        {
            var comparisonNode = new ComparisonNode(parent);
            comparisonNode.LeftPart = VisitRightPart(node.ChildNodes[0], comparisonNode);
            comparisonNode.Operator = node.ChildNodes[1].ChildNodes[0].Token.Text;
            comparisonNode.RightPart = VisitRightPart(node.ChildNodes[2], comparisonNode);
            return comparisonNode;
        }

        private LambdaExpressionNode VisitLambdaExpression(ParseTreeNode node, AstNode parent)
        {
            var lambdaExpressionNode = new LambdaExpressionNode(parent);
            lambdaExpressionNode.Parameter = node.ChildNodes[0].Token.Text;
            lambdaExpressionNode.Body = VisitRightPart(node.ChildNodes[2], lambdaExpressionNode);

            return lambdaExpressionNode;
        }

        private NumberLiteralNode VisitNumericLiteral(ParseTreeNode node, AstNode parent)
        {
            var numericLiteral = new NumberLiteralNode(parent);
            numericLiteral.Number = node.Token.Text;
            return numericLiteral;
        }
        

        private RightPartNode VisitRightPart(ParseTreeNode node, AstNode parent)
        {
            var rightPartNode = new RightPartNode(parent);
            switch (node.ChildNodes[0].Term.Name)
            {
                case TokenConsts.generalPropertyAccess:
                    rightPartNode.PropertyAccess = VisitGeneralPropertyAccess(node.ChildNodes[0], rightPartNode);
                    break;
                case TokenConsts.functionCall:
                    rightPartNode.FunctionCall = VisitFunctionCall(node.ChildNodes[0], rightPartNode);
                    break;
                case TokenConsts.objectCreation:
                    rightPartNode.ObjectCreation = VisitObjectCreation(node.ChildNodes[0], rightPartNode);
                    break;
                case TokenConsts.numberLiteral:
                    rightPartNode.NumberLiteral = VisitNumericLiteral(node.ChildNodes[0], rightPartNode);
                    break;
                default: throw new NotImplementedException();
            }
            return rightPartNode;
        }
    }
}
