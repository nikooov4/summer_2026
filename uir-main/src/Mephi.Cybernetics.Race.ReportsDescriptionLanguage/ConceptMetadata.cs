using System.Collections;
using System.Collections.Immutable;

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{
    public class AttributeDefenitionMetadata
    {
        public string? Description {get; set;}
        public uint OrderIndex {get;}
        public string? Comment {get; set;}
    }

    public interface IConstraint
    {
        Type CSType {get;}
        bool IsValueValid(object? value);
        //Тут ещё должен быть вариант IsValueValid, который возвращает причину, почему значение не валидное
        bool IsNullable {get;}
    }

    public class TypeConstraint<T> : IConstraint
    {
        public Type CSType => typeof(T);
        public bool IsNullable {get;}
        public TypeConstraint(bool isNullable)
        {
            IsNullable = isNullable;
        }
        public bool IsValueValid(object? value)
        {
            if (value == null)
            {
                return IsNullable;
            }
            return CSType.IsAssignableFrom(value.GetType());
        }
    }

    public class RangeConstraint<T> : IConstraint where T : IComparable<T> 
    {
        public Type CSType => typeof(T);
        public T MinValue {get;}
        public bool IsIncludingMin {get;}
        public T MaxValue {get;}
        public bool IsIncludingMax {get;}

        public bool IsNullable {get;}

        public RangeConstraint(T min, T max, bool isNullable, bool isIncludingMin = true, bool isIncludingMax = true)
        {
            MinValue = min;
            MaxValue = max;
            IsIncludingMin = isIncludingMin;
            IsIncludingMax = isIncludingMax;
            IsNullable = isNullable;
        }

        public bool IsValueValid(object? value)
        {
            if (value == null)
            {
                return IsNullable;
            }
            if (value is IComparable<T> compValue)
            {
                int minComp = compValue.CompareTo(MinValue); 
                int maxComp = compValue.CompareTo(MaxValue);
                bool minCheck = IsIncludingMin ? minComp >= 0 : minComp > 0;
                bool maxCheck = IsIncludingMax ? maxComp <= 0 : maxComp < 0;
                return minCheck && maxCheck;
            }
            return false;
        }
    }

    public class OneOfConstraint<T> : IConstraint where T : IEqualityComparer<T>
    {
        public Type CSType => typeof(T);
        public bool IsNullable {get;}
        public IImmutableSet<T> AllowedValues {get;}

        public OneOfConstraint(IEnumerable<T>? allowedValues, bool isNullable)
        {

            IsNullable = isNullable;
            AllowedValues = (IImmutableSet<T>?)allowedValues?.ToHashSet() ?? throw new ArgumentNullException(nameof(allowedValues));
        }

        public bool IsValueValid(object? value)
        {
            if (value == null)
            {
                return IsNullable;
            }
            if (value is T equatableValue)
            {
                return AllowedValues.Contains(equatableValue);
            }
            return false;
        }
    }

}