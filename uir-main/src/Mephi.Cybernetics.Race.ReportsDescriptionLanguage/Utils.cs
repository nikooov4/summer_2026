using System.Collections.Immutable;

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{
    public static class Utils
    {
        public static bool CompareDictKeys<TKey, TValue1, TValue2>
        (
            IReadOnlyDictionary<TKey, TValue1> dict1,
            IReadOnlyDictionary<TKey, TValue2> dict2
        )
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (var key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key))
                {
                    return false;
                }
            }

            foreach (var key in dict2.Keys)
            {
                if (!dict1.ContainsKey(key))
                {
                    return false;
                }
            }
            return true;
        }
    }
}