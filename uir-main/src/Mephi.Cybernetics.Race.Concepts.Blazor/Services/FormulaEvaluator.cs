using System.Globalization;

namespace Mephi.Cybernetics.Race.Concepts.Blazor.Services;

public sealed record FormulaDependency(IReadOnlyList<string> Path)
{
    public string DisplayPath => string.Join(".", Path);
}

public sealed record FormulaAnalysisResult(
    bool Success,
    IReadOnlyList<FormulaDependency> Dependencies,
    string? Error);

public sealed record FormulaEvaluationResult(
    bool Success,
    double? Value,
    string? Error)
{
    public static FormulaEvaluationResult FromValue(double? value)
    {
        return new FormulaEvaluationResult(true, value, null);
    }

    public static FormulaEvaluationResult FromError(string error)
    {
        return new FormulaEvaluationResult(false, null, error);
    }
}

public static class FormulaEvaluator
{
    public static FormulaAnalysisResult Analyze(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return new FormulaAnalysisResult(false, Array.Empty<FormulaDependency>(), "Формула пустая.");
        }

        var dependencies = new List<FormulaDependency>();
        var parser = new Parser(expression, path =>
        {
            dependencies.Add(new FormulaDependency(path));
            return FormulaEvaluationResult.FromValue(1);
        });

        var result = parser.TryEvaluate();
        return result.Success
            ? new FormulaAnalysisResult(true, MergeDependencies(dependencies), null)
            : new FormulaAnalysisResult(false, MergeDependencies(dependencies), result.Error);
    }

    public static FormulaEvaluationResult Evaluate(
        string? expression,
        Func<IReadOnlyList<string>, FormulaEvaluationResult> resolveValue)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return FormulaEvaluationResult.FromError("Формула пустая.");
        }

        return new Parser(expression, resolveValue).TryEvaluate();
    }

    private static IReadOnlyList<FormulaDependency> MergeDependencies(IEnumerable<FormulaDependency> dependencies)
    {
        return dependencies
            .GroupBy(dependency => dependency.DisplayPath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private sealed class Parser
    {
        private readonly string _expression;
        private readonly Func<IReadOnlyList<string>, FormulaEvaluationResult> _resolveValue;
        private int _position;

        public Parser(
            string expression,
            Func<IReadOnlyList<string>, FormulaEvaluationResult> resolveValue)
        {
            _expression = expression;
            _resolveValue = resolveValue;
        }

        public FormulaEvaluationResult TryEvaluate()
        {
            try
            {
                var value = ParseExpression();
                SkipWhitespace();

                if (!IsEnd)
                {
                    return Error($"Неожиданный символ \"{Current}\".");
                }

                return FormulaEvaluationResult.FromValue(value);
            }
            catch (FormulaParseException exception)
            {
                return Error(exception.Message);
            }
        }

        private double? ParseExpression()
        {
            var value = ParseTerm();

            while (true)
            {
                SkipWhitespace();
                if (!TryReadOperator('+') && !TryReadOperator('-'))
                {
                    return value;
                }

                var operation = _expression[_position - 1];
                var right = ParseTerm();
                value = ApplyBinary(value, right, operation);
            }
        }

        private double? ParseTerm()
        {
            var value = ParseUnary();

            while (true)
            {
                SkipWhitespace();
                if (!TryReadOperator('*') && !TryReadOperator('/'))
                {
                    return value;
                }

                var operation = _expression[_position - 1];
                var right = ParseUnary();
                value = ApplyBinary(value, right, operation);
            }
        }

        private double? ParseUnary()
        {
            SkipWhitespace();

            if (TryReadOperator('+'))
            {
                return ParseUnary();
            }

            if (TryReadOperator('-'))
            {
                var value = ParseUnary();
                return value is null ? null : -value.Value;
            }

            return ParsePrimary();
        }

        private double? ParsePrimary()
        {
            SkipWhitespace();

            if (TryReadOperator('('))
            {
                var value = ParseExpression();
                SkipWhitespace();

                if (!TryReadOperator(')'))
                {
                    throw new FormulaParseException("Не найдена закрывающая скобка.");
                }

                return value;
            }

            if (char.IsDigit(Current) || Current == '.')
            {
                return ParseNumber();
            }

            if (IsIdentifierStart(Current))
            {
                return ParseDependencyValue();
            }

            throw new FormulaParseException(IsEnd
                ? "Формула неожиданно закончилась."
                : $"Неожиданный символ \"{Current}\".");
        }

        private double ParseNumber()
        {
            var start = _position;
            var hasDecimalSeparator = false;

            while (!IsEnd)
            {
                if (char.IsDigit(Current))
                {
                    _position++;
                    continue;
                }

                if (Current == '.' && !hasDecimalSeparator)
                {
                    hasDecimalSeparator = true;
                    _position++;
                    continue;
                }

                break;
            }

            var text = _expression[start.._position];
            if (!double.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                throw new FormulaParseException($"Число \"{text}\" записано неверно.");
            }

            return value;
        }

        private double? ParseDependencyValue()
        {
            var path = ParsePath();
            var result = _resolveValue(path);

            if (!result.Success)
            {
                throw new FormulaParseException(result.Error ?? "Не удалось вычислить зависимость.");
            }

            return result.Value;
        }

        private IReadOnlyList<string> ParsePath()
        {
            var path = new List<string> { ParseIdentifier() };

            while (true)
            {
                SkipWhitespace();
                if (!TryReadOperator('.'))
                {
                    return path;
                }

                SkipWhitespace();
                path.Add(ParseIdentifier());
            }
        }

        private string ParseIdentifier()
        {
            if (!IsIdentifierStart(Current))
            {
                throw new FormulaParseException("Ожидалось имя столбца.");
            }

            var start = _position;
            _position++;

            while (!IsEnd && IsIdentifierPart(Current))
            {
                _position++;
            }

            return _expression[start.._position];
        }

        private static double? ApplyBinary(double? left, double? right, char operation)
        {
            if (left is null || right is null)
            {
                return null;
            }

            return operation switch
            {
                '+' => left.Value + right.Value,
                '-' => left.Value - right.Value,
                '*' => left.Value * right.Value,
                '/' when Math.Abs(right.Value) < double.Epsilon
                    => throw new FormulaParseException("Деление на ноль."),
                '/' => left.Value / right.Value,
                _ => throw new FormulaParseException($"Операция \"{operation}\" не поддерживается.")
            };
        }

        private bool TryReadOperator(char operation)
        {
            if (Current != operation)
            {
                return false;
            }

            _position++;
            return true;
        }

        private void SkipWhitespace()
        {
            while (!IsEnd && char.IsWhiteSpace(Current))
            {
                _position++;
            }
        }

        private bool IsEnd => _position >= _expression.Length;

        private char Current => IsEnd ? '\0' : _expression[_position];

        private static bool IsIdentifierStart(char value)
        {
            return char.IsLetter(value) || value == '_';
        }

        private static bool IsIdentifierPart(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }

        private static FormulaEvaluationResult Error(string error)
        {
            return FormulaEvaluationResult.FromError(error);
        }
    }

    private sealed class FormulaParseException : Exception
    {
        public FormulaParseException(string message)
            : base(message)
        {
        }
    }
}
