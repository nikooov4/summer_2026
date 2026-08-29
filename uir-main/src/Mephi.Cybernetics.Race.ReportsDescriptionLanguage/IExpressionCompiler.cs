using System.Collections.Immutable;
using System.Linq.Expressions;
//TODO: переделать ICompiledExpression в запись/структуру?

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{
    using AttrCompFunc = Func<object[], object>;
    
    public interface IExpressionCompiler
    {
        ICompiledExpression Compile(IEnumerable<ExpressionCompilerArgInfo> context, string SourceString);
    }

    public interface ICompiledExpression
    {
        public Type ReturnCSType { get; }
        public string SourceString { get; }
        public ImmutableList<ExpressionCompilerArgInfo> Args { get; }
        public AttrCompFunc Func { get; }
    }

    public interface IExpressionCompilerTypeInfoProvider
    {
        bool HasCSType {get;}
        Type? CSType {get;}
        IExpressionCompilerTypeInfoProvider TryGetMemeber(string name);
        object? CSObject {get;}
    }

    public class TypeInfoCSObjWrapper<T> : IExpressionCompilerTypeInfoProvider
    {
        public bool HasCSType => throw new NotImplementedException();
        public Type? CSType => typeof(T);
        public object? CSObject {get; private set;}
        public IExpressionCompilerTypeInfoProvider TryGetMemeber(string name)
        {
            throw new NotImplementedException();
        }
        public TypeInfoCSObjWrapper(T obj)
        {
            CSObject = obj;
        }
    }

    public record ExpressionCompilerArgInfo(Type CSType, List<string> Path);
    public record ExpressionCompilerMethodInfo(string Name, Type objType, List<Type> argTypes, Type returnType, Delegate func);
}