using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Mephi.Cybernetics.Race.Core
{
    public readonly struct Optional<T> : IEquatable<Optional<T>>
    {
        private readonly T _value;

        [Pure]
        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        public static explicit operator T(Optional<T> optional)
        {
            if (optional.HasValue)
                return optional.Value;
            else
                throw new ArgumentException("Value is None");
        }

        [Pure]
        public static Optional<T> None { get; private set; }

        static Optional()
        {
            Optional<T>.None = new Optional<T>();
        }

//        public Optional()
//        {
//            HasValue = false;
//            Value = default(T);
//        }

        public Optional(T value)
        {
            _value = value;
            HasValue = true;
        }

        [Pure]
        public T Value => HasValue ? _value : throw new InvalidOperationException("Optional is None");

        [Pure]
        public bool HasValue { get; }

        #region Equality members

        public bool Equals(Optional<T> other)
        {
            return EqualityComparer<T>.Default.Equals(_value, other._value) && HasValue == other.HasValue;
        }

        public override bool Equals(object obj)
        {
            return obj is Optional<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
#if NETFRAMEWORK
            int num1 = 17;
            int num2 = 31;
            return (num1 * num2 * _value.GetHashCode()) * num2 + HasValue.GetHashCode();
#else
            return HashCode.Combine(_value, HasValue);
#endif
        }

#endregion

        public override string ToString()
        {
            return HasValue ? $"Optional({Value})" : "Optional.None";
        }

        public static bool operator ==(Optional<T> value1, Optional<T> value2)
        {
            return value1.Equals(value2);
        }
        public static bool operator !=(Optional<T> value1, Optional<T> value2)
        {
            return !value1.Equals(value2);
        }
    }
}