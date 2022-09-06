namespace Siteswaps.Generator.Generator;

public static class EnumerableExtension
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
        return source
            .Where(x => x is not null)
            .Select(x => x!);

    }
}