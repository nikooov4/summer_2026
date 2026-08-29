using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mephi.Cybernetics.Race.MappingDescriptionLanguage
{
    public class LanguageUtils
    {
        public static IList CreateList(Type myType)
        {
            Type genericListType = CreateListType(myType);
            return (IList)Activator.CreateInstance(genericListType);
        }

        public static Type CreateListType(Type myType)
        {
            return typeof(List<>).MakeGenericType(myType);
        }

        public static List<List<T>> Transpose<T>(List<List<T>> lists)
        {
            Debug.Assert(lists.Any());
            Debug.Assert(lists.All(c => c.Count == lists.First().Count));

            var longest = lists.Any() ? lists.Max(l => l.Count) : 0;
            List<List<T>> outer = new List<List<T>>(longest);
            for (int i = 0; i < longest; i++)
                outer.Add(new List<T>(lists.Count));
            for (int j = 0; j < lists.Count; j++)
                for (int i = 0; i < longest; i++)
                    outer[i].Add(lists[j].Count > i ? lists[j][i] : default(T));
            return outer;
        }
    }
}
