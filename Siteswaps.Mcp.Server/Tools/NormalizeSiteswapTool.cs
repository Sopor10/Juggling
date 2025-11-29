using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class NormalizeSiteswapTool
{
    [McpServerTool]
    [Description("Normalizes a siteswap to its unique representation (canonical form). This ensures that different rotations of the same siteswap are represented identically.")]
    public string NormalizeSiteswap(
        [Description("Siteswap string (e.g., '531', '441', 'a7242')")] string siteswap)
    {
        if (string.IsNullOrWhiteSpace(siteswap))
        {
            throw new ArgumentException("Siteswap string cannot be null or empty.", nameof(siteswap));
        }

        if (!SiteswapDetails.TryCreate(siteswap, out var siteswapObj))
        {
            throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
        }

        var normalized = SiteswapDetails.ToUniqueRepresentation(siteswapObj.Items.EnumerateValues(1).ToArray());
        var normalizedSiteswap = new SiteswapDetails(normalized);
        return normalizedSiteswap.ToString();
    }
}

