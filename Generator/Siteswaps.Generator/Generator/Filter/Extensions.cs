using Shared;

namespace Siteswaps.Generator.Generator.Filter;

public static class Extensions
{
    public static List<T> Rotate<T>(this IEnumerable<T> source, int number)
    {
        return source.ToCyclicArray().Rotate(number).EnumerateValues(1).ToList();
    }
}
