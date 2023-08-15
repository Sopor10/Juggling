namespace Siteswaps.Generator.Generator;

public static class IntExtensions
{
    public static string Transform(this int i)
    {
        return i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString()
        };
    }

    public static string ToSiteswapString(this IEnumerable<int> items)
    {
        return string.Join("", items.Select(Transform));
    }
}