using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class CombineLocalSiteswapsTool
{
    [McpServerTool]
    [Description(
        "Combines multiple local siteswaps into a single global siteswap. All local siteswaps must have the same length and compatible interfaces."
    )]
    public ToolResult<CombineLocalSiteswapsResult> CombineLocalSiteswaps(
        [Description("List of local siteswap strings (e.g., ['978', '732'])")]
            IEnumerable<string> localSiteswaps
    )
    {
        var result = LocalSiteswap.FromLocals(
            localSiteswaps
                .Select(IList<int> (x) => x.Select(SiteswapDetails.ToInt).ToList())
                .ToList()
        );
        return result switch
        {
            Result<SiteswapDetails>.Success success => ToolResult<CombineLocalSiteswapsResult>.Ok(
                new CombineLocalSiteswapsResult { GlobalSiteswap = success.ToString() }
            ),
            Result<SiteswapDetails>.Failure error => ToolResult<CombineLocalSiteswapsResult>.Fail(
                error.Error
            ),
        };
    }
}

public class CombineLocalSiteswapsResult
{
    public string GlobalSiteswap { get; init; } = string.Empty;
}
