using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MirrorConfigBL.Extender
{
    public static class ListExtender
    {

        public static bool IsNullOrEmpty(this ICollection List)
        {
            return (List?.Count ?? 0) == 0;
        }

        public static bool IsNotNullOrEmpty(this ICollection List)
        {
            return !List.IsNullOrEmpty();
        }

        public static void AddIfNeeded<T>(this ICollection<T> List, T Item)
        {
            if (List.Contains(Item))
                return;
            List.Add(Item);
        }

        public static void AddRangeIfNeeded<T>(this List<T> List, IEnumerable<T> Range)
        {
            List.AddRange(Range.Except(List));
        }

        public static void AddWithDefaultValue<T,V>(this Dictionary<T,V> Dictionary, IEnumerable<T> Keys)
        {
            foreach (var key in Keys)
            {
                Dictionary[key] = default(V);
            }
        }
    }
}
