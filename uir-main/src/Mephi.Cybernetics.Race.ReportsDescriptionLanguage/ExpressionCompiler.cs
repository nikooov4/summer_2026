using System.Collections.Immutable;
using System.Linq.Expressions;
using Irony.Ast;
using Irony.Parsing;
// TODO: Upcast к более общему типу при бинарных операциях с числами
// TODO: Приведение к nullable типу при проверке на null (также в BuildBinOp)
// TODO: Переместить в Omelea?


namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{
    using AttrCompFunc = Func<object[], object>;

    public class ExpressionCompiler : IExpressionCompiler
    {
        private ConceptMappingExpressionGrammar _grammar = new ConceptMappingExpressionGrammar();
        private Parser _parser;
        public ExpressionCompiler()
        {
            _parser = new Parser(_grammar);
        }

        public ICompiledExpression Compile(IEnumerable<ExpressionCompilerArgInfo> context, string Source)
        {
            var node = _parser.Parse(Source).Root;
            return new CompiledExpression(node, context, Source);
        }
    }

    public class CompiledExpression : ICompiledExpression
    {
        private record ArgInfoWithIndex(ExpressionCompilerArgInfo argInfo, int index);
        public string SourceString { get; }
        public Type ReturnCSType { get; }
        private Dictionary<List<string>, ArgInfoWithIndex> _attributeDefArgs;
        private ParameterExpression _attrValuesList = Expression.Parameter(typeof(object[]));
        private int _nextAtrrValuesListIndex = 0;
        public ImmutableList<ExpressionCompilerArgInfo> Args {get;}
        public AttrCompFunc Func { get; }
        private IReadOnlyDictionary<string, Func<ParseTreeNode, Expression>> _funcByTokenName;
        private Dictionary<List<string>, ExpressionCompilerArgInfo> _context;

        private static IReadOnlyDictionary<string, Func<Expression, Expression, BinaryExpression>> _binExprByString = new Dictionary<string, Func<Expression, Expression, BinaryExpression>>()
        {
            {"+", Expression.Add},
            {"-", Expression.Subtract},
            {"*", Expression.Multiply},
            {"/", Expression.Divide},
        };
        
        private class PathEqualityComparer : IEqualityComparer<List<string>>
        {
            public bool Equals(List<string>? x, List<string>? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null || x.Count != y.Count)
                {
                    return false;
                }

                return x.SequenceEqual(y);
            }

            public int GetHashCode(List<string> list)
            {
                unchecked
                {
                    int hash = 17;
                    foreach (var item in list)
                    {
                        hash = hash * 31 + (item?.GetHashCode() ?? 0);
                    }
                    return hash;
                }
            }
        }

        public CompiledExpression(ParseTreeNode node, IEnumerable<ExpressionCompilerArgInfo> context, string source)
        {
            _funcByTokenName = new Dictionary<string, Func<ParseTreeNode, Expression>>()
            {
                {ConceptDSLTokenNames.binOpExpr, BuildBinOp},
                {ConceptDSLTokenNames.attribute, BuildAttribute},
                {ConceptDSLTokenNames.numberLiteral, BuildNumberLiteral}
            };

            _attributeDefArgs = new Dictionary<List<string>, ArgInfoWithIndex>(new PathEqualityComparer());
            _context = context.ToDictionary(x => x.Path, new PathEqualityComparer());

            SourceString = source;
            var concept = Expression.Parameter(typeof(ConceptInstance), "concept");
            var expr = BuildExpression(node);
            ReturnCSType = expr.Type;
            Func = Expression.Lambda<AttrCompFunc>(
                Expression.Convert(expr, typeof(object)),
                _attrValuesList
                ).Compile();

            Args = _attributeDefArgs.Values.Select(x => x.argInfo).ToImmutableList();
        }

        private Expression BuildExpression(ParseTreeNode node)
        {
            return _funcByTokenName[node.Term.Name].Invoke(node);
        }

        private static Type GetCommonNumericType(Type t1, Type t2)
        {
            // Поддерживаем только int? и double? (можно расширить при необходимости)
            if (t1 == typeof(double?) || t2 == typeof(double?))
                return typeof(double?);
            if (t1 == typeof(int?) && t2 == typeof(int?))
                return typeof(int?);
            // fallback
            return typeof(double?);
        }

        private Expression BuildBinOp(ParseTreeNode node)
        {
            var childNodes = node.ChildNodes;
            var operand1 = BuildExpression(childNodes[0]);
            var operand2 = BuildExpression(childNodes[2]);
            string binOpString = childNodes[1].Token.ValueString;

            // Если оба операнда - числа, привести к наиболее общему типу
            var t1 = operand1.Type;
            var t2 = operand2.Type;
            if ((t1 == typeof(int?) || t1 == typeof(double?)) && (t2 == typeof(int?) || t2 == typeof(double?)))
            {
                var commonType = GetCommonNumericType(t1, t2);
                operand1 = Expression.Convert(operand1, commonType);
                operand2 = Expression.Convert(operand2, commonType);
            }
            else if (binOpString == "/")
            {
                operand1 = Expression.Convert(operand1, typeof(double?));
                operand2 = Expression.Convert(operand2, typeof(double?));
            }
            
            var binOpExpr = _binExprByString[binOpString].Invoke(
                operand1,
                operand2
            );

            return Expression.Condition(
                Expression.OrElse(
                    Expression.Equal(operand1, Expression.Constant(null, typeof(object))),
                    Expression.Equal(operand2, Expression.Constant(null, typeof(object)))
                ),
                Expression.Constant(null, binOpExpr.Type),
                binOpExpr
            );
        }

        private Expression BuildNumberLiteral(ParseTreeNode node)
        {
            var value = node.Token.Value;
            var type = value.GetType();
            var nullableType = typeof(Nullable<>).MakeGenericType(type);
            return Expression.Constant(value, nullableType);
        }

        // private object GetAttribute(ParseTreeNode node, ConceptInstance concept)
        // {
        //     var curConcept = concept.GetDependency("Student");
        //     foreach (var childNode in node.ChildNodes.SkipLast(1))
        //     {
        //         curConcept = concept.GetDependency(childNode.Token.ValueString);
        //     }
        //     return curConcept.GetAttributeValue(node.ChildNodes.Last().Token.ValueString);
        // }

        private Expression BuildAttribute(ParseTreeNode node)
        {
            var childNodes = node.ChildNodes;
            var path = childNodes.Select(node => node.Token.ValueString).ToList();
            ArgInfoWithIndex? argInfoWithIndex = null;

            _attributeDefArgs.TryGetValue(path, out argInfoWithIndex);
            if (_context.TryGetValue(path, out var argInfo))
            {
                argInfoWithIndex = new ArgInfoWithIndex(argInfo, _nextAtrrValuesListIndex); 
            }

            if (argInfoWithIndex == null)
            {
                throw new VariableNotDefiendException(path.Skip(1).Aggregate(path[0], (string x, string y) => x + "." + y).ToString());
            }

            var attrValue = Expression.ArrayIndex(_attrValuesList, Expression.Constant(argInfoWithIndex.index));
            _attributeDefArgs.Add(path, argInfoWithIndex);
            _nextAtrrValuesListIndex++;

            return Expression.Convert(attrValue, argInfoWithIndex.argInfo.CSType);
        }
    }

}