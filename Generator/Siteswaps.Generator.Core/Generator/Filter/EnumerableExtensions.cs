namespace Siteswaps.Generator.Core.Generator.Filter;

public static class EnumerableExtensions
{
    public static List<T> Rotate<T>(this IEnumerable<T> source, int number)
    {
        return source.ToCyclicArray().Rotate(number).EnumerateValues(1).ToList();
    }
}
