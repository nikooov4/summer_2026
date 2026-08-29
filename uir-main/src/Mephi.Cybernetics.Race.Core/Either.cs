using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Mephi.Cybernetics.Race.Core
{
    public readonly struct Either<T1,T2> : IEquatable<Either<T1,T2>>
    {
        [Pure]
        public static implicit operator Either<T1, T2>(T1 value)
        {
            return new Either<T1, T2>(value);
        }
        [Pure]
        public static implicit operator Either<T1, T2>(T2 value)
        {
            return new Either<T1, T2>(value);
        }

        [Pure]
        public static explicit operator T1(Either<T1, T2> either)
        {
            return either.First;
        }
        [Pure]
        public static explicit operator T2(Either<T1, T2> either)
        {
            return either.Second;
        }

        private readonly T1 _first;
        private readonly bool _isFirst;
        private readonly T2 _second;
        private readonly bool _isSecond;

        public Either(T1 first)
        {
            _first = first;
            _isFirst = true;
            _second = default(T2);
            _isSecond = false;
        }

        public Either(T2 second)
        {
            _first = default(T1);
            _isFirst = false;
            _second = second;
            _isSecond = true;
        }

        [Pure]
        public T1 First => _isFirst ? _first : throw new InvalidOperationException("Either is not First");

        [Pure]
        public Optional<T1> FirstOptional => _isFirst ? new Optional<T1>(_first) : Optional<T1>.None;

        [Pure]
        public bool IsFirst => _isFirst;

        [Pure]
        public T2 Second => _isSecond ? _second : throw new InvalidOperationException("Either is not Second");
        
        [Pure]
        public Optional<T2> SecondOptional => _isSecond ? new Optional<T2>(_second) : Optional<T2>.None;

        [Pure]
        public bool IsSecond => _isSecond;

        #region Equality members

        public bool Equals(Either<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(_first, other._first) && _isFirst == other._isFirst &&
                   EqualityComparer<T2>.Default.Equals(_second, other._second) && _isSecond == other._isSecond;
        }

        public override bool Equals(object obj)
        {
            return obj is Either<T1, T2> other && Equals(other);
        }

        public override int GetHashCode()
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            int hash = 17;
            int num2 = 31;
            hash = hash * num2 + _first.GetHashCode();
            hash = hash * num2 + _isFirst.GetHashCode();
            hash = hash * num2 + _second.GetHashCode();
            hash = hash * num2 + _isSecond.GetHashCode();
            return hash;
#else
            return HashCode.Combine(_first, _isFirst, _second, _isSecond);
#endif
        }

        #endregion
        public static bool operator ==(Either<T1, T2> one, Either<T1, T2> two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(Either<T1, T2> one, Either<T1, T2> two)
        {
            return !one.Equals(two);
        }

    }

    public readonly struct Either<T1, T2, T3> : IEquatable<Either<T1, T2, T3>>
    {
        [Pure]
        public static implicit operator Either<T1, T2, T3>(T1 value)
        {
            return new Either<T1, T2, T3>(value);
        }
        [Pure]
        public static implicit operator Either<T1, T2, T3>(T2 value)
        {
            return new Either<T1, T2, T3>(value);
        }
        [Pure]
        public static implicit operator Either<T1, T2, T3>(T3 value)
        {
            return new Either<T1, T2, T3>(value);
        }

        [Pure]
        public static explicit operator T1(Either<T1, T2, T3> either)
        {
            return either.First;
        }
        [Pure]
        public static explicit operator T2(Either<T1, T2, T3> either)
        {
            return either.Second;
        }
        [Pure]
        public static explicit operator T3(Either<T1, T2, T3> either)
        {
            return either.Third;
        }

        private readonly T1 _first;
        private readonly bool _isFirst;
        private readonly T2 _second;
        private readonly bool _isSecond;
        private readonly T3 _third;
        private readonly bool _isThird;

        public Either(T1 first)
        {
            _first = first;
            _isFirst = true;
            _second = default(T2);
            _isSecond = false;
            _third = default(T3);
            _isThird = false;
        }

        public Either(T2 second)
        {
            _first = default(T1);
            _isFirst = false;
            _second = second;
            _isSecond = true;
            _third = default(T3);
            _isThird = false;
        }

        public Either(T3 third)
        {
            _first = default(T1);
            _isFirst = false;
            _second = default(T2);
            _isSecond = false;
            _third = third;
            _isThird = true;
        }

        [Pure]
        public T1 First => _isFirst ? _first : throw new InvalidOperationException("Either is not First");

        [Pure]
        public Optional<T1> FirstOptional => _isFirst ? new Optional<T1>(_first) : Optional<T1>.None;

        [Pure]
        public bool IsFirst => _isFirst;

        [Pure]
        public T2 Second => _isSecond ? _second : throw new InvalidOperationException("Either is not Second");

        [Pure]
        public Optional<T2> SecondOptional => _isSecond ? new Optional<T2>(_second) : Optional<T2>.None;

        [Pure]
        public bool IsSecond => _isSecond;

        [Pure]
        public T3 Third => _isThird ? _third : throw new InvalidOperationException("Either is not Second");

        [Pure]
        public Optional<T3> ThirdOptional => _isThird ? new Optional<T3>(_third) : Optional<T3>.None;

        [Pure]
        public bool IsThird => _isThird;

        #region Equality members

        public bool Equals(Either<T1, T2, T3> other)
        {
            return EqualityComparer<T1>.Default.Equals(_first, other._first) && _isFirst == other._isFirst &&
                   EqualityComparer<T2>.Default.Equals(_second, other._second) && _isSecond == other._isSecond &&
                   EqualityComparer<T3>.Default.Equals(_third, other._third) && _isThird == other._isThird;
        }

        public override bool Equals(object obj)
        {
            return obj is Either<T1, T2, T3> other && Equals(other);
        }

        public override int GetHashCode()
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            int hash = 17;
            int num2 = 31;
            hash = hash * num2 + _first.GetHashCode();
            hash = hash * num2 + _isFirst.GetHashCode();
            hash = hash * num2 + _second.GetHashCode();
            hash = hash * num2 + _isSecond.GetHashCode();
            hash = hash * num2 + _third.GetHashCode();
            hash = hash * num2 + _isThird.GetHashCode();
            return hash;
#else
            return HashCode.Combine(_first, _isFirst, _second, _isSecond, _third, _isThird);
#endif
        }

        #endregion

        public static bool operator ==(Either<T1, T2, T3> one, Either<T1, T2, T3> two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(Either<T1, T2, T3> one, Either<T1, T2, T3> two)
        {
            return !one.Equals(two);
        }
    }
}