using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class ValidateSiteswapTool
{
    [McpServerTool]
    [Description(
        "Validates whether a siteswap string is valid. Returns true if the siteswap is valid, false otherwise."
    )]
    public ToolResult<bool> ValidateSiteswap(
        [Description("The siteswap string to validate (e.g., '5,3,1', '4,4,1', 'a,7,2,4,2')")]
            string siteswap
    )
    {
        return ToolResult.From(() =>
        {
            var coreSiteswap = SiteswapMapper.ToCoreFormat(siteswap);
            if (string.IsNullOrWhiteSpace(coreSiteswap))
            {
                return false;
            }

            return Siteswap.Details.Siteswap.TryCreate(coreSiteswap, out _);
        });
    }
}
