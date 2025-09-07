using MoreLinq.Extensions;

namespace Siteswap.Details;

public static class EnumerableIntExtension
{
    public static int CompareSequences(this IEnumerable<int> arr1, IEnumerable<int> arr2)
    {
        if (!arr1.Any() && !arr2.Any())
        {
            throw new InvalidOperationException("At least oe sequence must be non empty");
        }
        foreach (
            var (first, second) in arr1.Select(x => (int?)x)
                .ZipLongest(arr2.Select(x => (int?)x), (i, j) => (i, j))
        )
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
}









