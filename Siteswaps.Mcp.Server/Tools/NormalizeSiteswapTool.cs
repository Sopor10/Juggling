using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class NormalizeSiteswapTool
{
    [McpServerTool]
    [Description(
        "Normalizes a siteswap to its unique representation (canonical form). This ensures that different rotations of the same siteswap are represented identically."
    )]
    public ToolResult<string> NormalizeSiteswap(
        [Description("Siteswap string (e.g., '5,3,1', '4,4,1', 'a,7,2,4,2')")] string siteswap
    )
    {
        return ToolResult.From(() =>
        {
            var coreSiteswap = SiteswapMapper.ToCoreFormat(siteswap);
            if (string.IsNullOrWhiteSpace(coreSiteswap))
            {
                throw new ArgumentException(
                    "Siteswap string cannot be null or empty.",
                    nameof(siteswap)
                );
            }

            if (!SiteswapDetails.TryCreate(coreSiteswap, out var siteswapObj))
            {
                throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
            }

            var normalized = SiteswapDetails.ToUniqueRepresentation(
                siteswapObj.Items.EnumerateValues(1).ToArray()
            );
            var normalizedSiteswap = new SiteswapDetails(normalized);
            return SiteswapMapper.ToDisplayFormat(normalizedSiteswap);
        });
    }
}
