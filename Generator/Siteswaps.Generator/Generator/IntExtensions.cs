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
    public static string Transform(this sbyte i) => Transform((int)i);

    public static string ToSiteswapString(this IEnumerable<sbyte> items)
    {
        return string.Join("", items.Select(Transform));
    }
}
