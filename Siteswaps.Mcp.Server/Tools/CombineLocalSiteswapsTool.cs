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
        [Description(
            "List of local siteswap string separated with | or if no commas are used with , (e.g., '5,3,1|5,3,1' or '531,531')"
        )]
            string localSiteswaps
    )
    {
        var separators = new[] { '|' };
        if (!localSiteswaps.Contains('|'))
        {
            separators = new[] { ',' };
        }

        var result = LocalSiteswap.FromLocals(
            localSiteswaps
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => SiteswapMapper.ToCoreFormat(x.Trim()))
                .Select(IList<int> (x) => [.. x.Select(SiteswapDetails.ToInt)])
                .ToList()
        );
        return result switch
        {
            Result<SiteswapDetails>.Success success => ToolResult<CombineLocalSiteswapsResult>.Ok(
                new CombineLocalSiteswapsResult
                {
                    GlobalSiteswap = SiteswapMapper.ToDisplayFormat(success.Value),
                }
            ),
            Result<SiteswapDetails>.Failure error => ToolResult<CombineLocalSiteswapsResult>.Fail(
                error.Error
            ),
            _ => throw new InvalidOperationException("Unknown result type"),
        };
    }
}

public class CombineLocalSiteswapsResult
{
    public string GlobalSiteswap { get; init; } = string.Empty;
}
