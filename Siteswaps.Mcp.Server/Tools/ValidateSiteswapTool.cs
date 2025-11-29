using System.ComponentModel;
using ModelContextProtocol.Server;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

public class ValidateSiteswapTool
{
    [McpServerTool]
    [Description("Validates whether a siteswap string is valid. Returns true if the siteswap is valid, false otherwise.")]
    public bool ValidateSiteswap(
        [Description("The siteswap string to validate (e.g., '531', '441', 'a7242')")] string siteswap)
    {
        if (string.IsNullOrWhiteSpace(siteswap))
        {
            return false;
        }

        return SiteswapDetails.TryCreate(siteswap, out _);
    }
}

