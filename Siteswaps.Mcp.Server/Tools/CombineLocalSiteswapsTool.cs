using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class CombineLocalSiteswapsTool
{
    [McpServerTool]
    [Description("Combines multiple local siteswaps into a single global siteswap.")]
    public ToolResult<CombineLocalSiteswapsResult> CombineLocalSiteswaps(
        [Description("List of local siteswap string seperated with , (e.g., 978,732)")]
            string localSiteswaps
    )
    {
        var result = LocalSiteswap.FromLocals(
            localSiteswaps
                .Split(',')
                .Select(IList<int> (x) => [.. x.Select(SiteswapDetails.ToInt)])
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
