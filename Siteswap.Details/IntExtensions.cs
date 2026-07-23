namespace Siteswap.Details;

public static class IntExtensions
{
    public static string ToSiteswapString(this int i) =>
        i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString(),
        };

    public static string ToSiteswapString(this IEnumerable<int> enumerable) =>
        string.Join("", enumerable.Select(x => x.ToSiteswapString()));
}
