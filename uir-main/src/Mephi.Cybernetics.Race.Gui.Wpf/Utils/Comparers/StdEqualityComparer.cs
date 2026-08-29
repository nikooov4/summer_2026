using System;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Gui.Wpf.Utils.Comparers
{
    public class StdEqualityComparer<T>:IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparer;

        public StdEqualityComparer(Predicate<Tuple<T,T>> comparer)
        {
            _comparer = (x,y) => comparer(new Tuple<T, T>(x,y));
        }

        public StdEqualityComparer(Predicate<(T,T)> comparer)
        {
            _comparer = (x,y) => comparer((x,y));
        }

        public StdEqualityComparer(Func<(T,T),bool> comparer)
        {
            _comparer = (x, y) => comparer((x, y));
        }

        public StdEqualityComparer(Func<T, T, bool> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return _comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        public static implicit operator StdEqualityComparer<T>(Func<T, T, bool> cmp)
        {
            return new StdEqualityComparer<T>(cmp);
        }
    }
}