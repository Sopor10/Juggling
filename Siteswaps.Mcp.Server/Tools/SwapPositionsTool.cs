using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class SwapPositionsTool
{
    [McpServerTool]
    [Description(
        "Swaps two positions in a siteswap and adjusts values accordingly. Returns the modified siteswap."
    )]
    public string SwapPositions(
        [Description("Siteswap string (e.g., '531', '441', 'a7242')")] string siteswap,
        [Description("First position index (0-based)")] int position1,
        [Description("Second position index (0-based)")] int position2
    )
    {
        if (string.IsNullOrWhiteSpace(siteswap))
        {
            throw new ArgumentException(
                "Siteswap string cannot be null or empty.",
                nameof(siteswap)
            );
        }

        if (!SiteswapDetails.TryCreate(siteswap, out var siteswapObj))
        {
            throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
        }

        if (position1 < 0)
        {
            throw new ArgumentException(
                "Position indices must be non-negative.",
                nameof(position1)
            );
        }

        if (position2 < 0)
        {
            throw new ArgumentException(
                "Position indices must be non-negative.",
                nameof(position2)
            );
        }

        var swapped = siteswapObj.Swap(position1, position2);
        return swapped.ToString();
    }
}
