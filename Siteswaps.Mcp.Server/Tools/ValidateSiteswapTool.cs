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
        [Description("The siteswap string to validate (e.g., '531', '441', 'a7242')")]
            string siteswap
    )
    {
        return ToolResult.From(() =>
        {
            if (string.IsNullOrWhiteSpace(siteswap))
            {
                return false;
            }

            return Siteswap.Details.Siteswap.TryCreate(siteswap, out _);
        });
    }
}
