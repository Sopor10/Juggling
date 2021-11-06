using System.Collections.Generic;
using System.Linq;
using Linq.Extras;
using MoreLinq.Extensions;

namespace Siteswaps.Generator
{
    public static class EnumerableIntExtension
    {
        
        public static int CompareSequences(this IEnumerable<int> arr1, IEnumerable<int> arr2)
        {
            foreach (var (first, second) in arr1.Select(x => (int?) x).ZipLongest(arr2.Select(x => (int?)x), (i,j)=> (i,j)))
            {
                if (first == second)
                {
                    continue;
                }

                if (first is null)
                {
                    return second < arr1.Max() ? 1 : -1;
                }

                if (second is null)
                {
                    return first < arr2.Max() ? -1 : 1;
                }

                if (first < second)
                {
                    return -1;
                }

                return 1;
            }

            return arr1.Count() < arr2.Count() ? -1 : 1;
        }
        
        public static IEnumerable<IEnumerable<int>> AbsteigendeSeq(this IEnumerable<int> input)
        {
            using var enumerator = input.GetEnumerator();
            var list = new List<int>();
            int? last = null;
            while (enumerator.MoveNext())
            {
                if (last is null)
                {
                    last = enumerator.Current;
                    list.Add(enumerator.Current);
                    continue;
                }

                if (enumerator.Current > last)
                {
                    yield return list;
                    list = new List<int>();
                }

                last = enumerator.Current;
                list.Add(enumerator.Current);
                
            }

            if (!list.IsNullOrEmpty())
            {
                yield return list;
            }
        }   
    }
}