using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.Core
{
    internal static class Utils
    {
        public static T FirstOrDefault<T>(IEnumerable<T> values, Func<T, bool> predicate)
        {
            foreach(var x in values)
            {
                if (predicate(x))
                    return x;
            }
            return default;
        }

        public static T First<T>(IEnumerable<T> values, Func<T, bool> predicate)
        {
            var val = FirstOrDefault(values, predicate);
            return val.Equals(default(T)) ? throw new KeyNotFoundException() : val; 
        }

    }
}
