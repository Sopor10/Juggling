namespace Siteswaps.Generator.Core.Generator;

public static class EnumerableExtension
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
        return source.OfType<T>();
    }
}
