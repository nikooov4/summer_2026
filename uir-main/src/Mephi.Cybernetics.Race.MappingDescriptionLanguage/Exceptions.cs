using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mephi.Cybernetics.Race.MappingDescriptionLanguage
{
    internal class ExceptionsUtils
    {
        public static string GetShortName(Type type)
        {
            string result = type.Name;
            if (type.IsGenericType)
            {
                // remove genric indication (e.g. `1)
                result = result.Substring(0, result.LastIndexOf('`'));

                result = string.Format(
                    "{0}<{1}>",
                    result,
                    string.Join(", ",
                                type.GetGenericArguments().Select(t => GetShortName(t))));
            }

            return result;
        }
    }

    public class CompilationException : Exception 
    {
        public CompilationException(string msg) : base($"Compile error: {msg}") { }
    }

    public class TypeHasNoPropertyException : CompilationException
    {
        public TypeHasNoPropertyException(Type type, string propertyName) : base($"Type '{ExceptionsUtils.GetShortName(type)}' has no property '{propertyName}'") { }

    }

    public class VariableNotDefiendException : CompilationException
    {
        public VariableNotDefiendException(string variableName) : base($"Variable '{variableName}' is not defined") { }
    }

    public class CantConvertException : CompilationException
    {
        public CantConvertException(Type from, Type to) : base($"Can't convert from type '{ExceptionsUtils.GetShortName(from)}' to type '{ExceptionsUtils.GetShortName(to)}'") { }
    }

    public class ConceptNotDefinedException : CompilationException
    {
        public ConceptNotDefinedException(string conceptName) : base($"Concept '{conceptName}' is not defined") { }
    }
}
