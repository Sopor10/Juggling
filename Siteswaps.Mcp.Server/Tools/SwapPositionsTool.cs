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
    public ToolResult<string> SwapPositions(
        [Description("Siteswap string (e.g., '5,3,1', '4,4,1', 'a,7,2,4,2')")] string siteswap,
        [Description("First position index (0-based)")] int position1,
        [Description("Second position index (0-based)")] int position2
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

            var length = siteswapObj.Length;
            if (position1 >= length)
            {
                throw new ArgumentException(
                    $"position1 ({position1}) is out of range for siteswap length {length}.",
                    nameof(position1)
                );
            }

            if (position2 >= length)
            {
                throw new ArgumentException(
                    $"position2 ({position2}) is out of range for siteswap length {length}.",
                    nameof(position2)
                );
            }

            var swapped = siteswapObj.Swap(position1, position2);
            return SiteswapMapper.ToDisplayFormat(swapped);
        });
    }
}
