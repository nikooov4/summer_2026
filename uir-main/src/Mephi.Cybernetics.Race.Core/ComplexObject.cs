using Mephi.Cybernetics.Race.Core.Attributes.ComplexObjects;

namespace Mephi.Cybernetics.Race.Core
{
    public interface IObjectConstructor
    {
        object[] SubObjects { get; }
        int Cardinality { get; }
    }

    [ObjectConstructor]
    public class TupleConstructor
    {
        protected readonly object[] _values;

        public TupleConstructor(params object[] values)
        {
            _values = new object[values.Length];
            values.CopyTo(_values, 0);
        }

        [SubObjectCollection]
        public object this[int index]
        {
            get { return _values[index]; }
        }

        public int Cardinality
        {
            get { return _values.Length; }
        }
    }

    public class TupleConstructor<T1,T2> : TupleConstructor
    {
        public TupleConstructor(T1 element1, T2 element2) : base(new object[] {element1, element2})
        {
        }

        [SubObject(0)]
        public T1 Element1
        {
            get { return (T1)_values[0]; }
        }

        [SubObject(1)]
        public T2 Element2
        {
            get { return (T2)_values[1]; }
        }
    }

    public class TupleConstructor<T1, T2, T3> : TupleConstructor
    {
        public TupleConstructor(T1 element1, T2 element2, T3 element3)
            : base(new object[] { element1, element2, element3 })
        {
        }

        [SubObject(0)]
        public T1 Element1
        {
            get { return (T1)_values[0]; }
        }

        [SubObject(1)]
        public T2 Element2
        {
            get { return (T2)_values[1]; }
        }

        [SubObject(2)]
        public T3 Element3
        {
            get { return (T3)_values[2]; }
        }
    }

    public class TupleConstructor<T1, T2, T3, T4> : TupleConstructor
    {
        public TupleConstructor(T1 element1, T2 element2, T3 element3, T4 element4)
            : base(new object[] { element1, element2, element3, element4 })
        {
        }

        [SubObject(0)]
        public T1 Element1
        {
            get { return (T1)_values[0]; }
        }

        [SubObject(1)]
        public T2 Element2
        {
            get { return (T2)_values[1]; }
        }

        [SubObject(2)]
        public T3 Element3
        {
            get { return (T3)_values[2]; }
        }

        [SubObject(3)]
        public T4 Element4
        {
            get { return (T4)_values[3]; }
        }
    }

    public class TupleConstructor<T1, T2, T3, T4, T5> : TupleConstructor
    {
        public TupleConstructor(T1 element1, T2 element2, T3 element3, T4 element4, T5 element5)
            : base(new object[] { element1, element2, element3, element4, element5 })
        {
        }

        [SubObject(0)]
        public T1 Element1
        {
            get { return (T1)_values[0]; }
        }

        [SubObject(1)]
        public T2 Element2
        {
            get { return (T2)_values[1]; }
        }

        [SubObject(2)]
        public T3 Element3
        {
            get { return (T3)_values[2]; }
        }

        [SubObject(3)]
        public T4 Element4
        {
            get { return (T4)_values[3]; }
        }

        [SubObject(4)]
        public T5 Element5
        {
            get { return (T5)_values[4]; }
        }
    }

    [EntityConstructor]
    public class EntityPair<T1,T2>
    {
        [SubEntity]
        public T1 First
        {
            get
            {
                return default(T1);
            }
        }

        [SubEntity]
        public T2 Second
        {
            get
            {
                return default(T2);
            }
        }
    }
}