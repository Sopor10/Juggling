using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

public static class SiteswapMapper
{
    public static string ToDisplayFormat(string siteswap)
    {
        return siteswap;
    }

    public static string ToCoreFormat(string siteswap)
    {
        return siteswap.Replace(",", string.Empty).Replace(" ", string.Empty);
    }

    public static string ToDisplayFormat(SiteswapDetails siteswap)
    {
        return siteswap.ToString();
    }

    public static string LocalToDisplayFormat(string localSiteswapLocalNotation)
    {
        return localSiteswapLocalNotation;
    }
}
