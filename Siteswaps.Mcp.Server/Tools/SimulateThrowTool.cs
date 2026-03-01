using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using Siteswap.Details.StateDiagram;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class SimulateThrowTool
{
    [McpServerTool]
    [Description(
        "Simulates a single throw in a siteswap and returns the resulting state and siteswap. Is also commonly referred to as rotating."
    )]
    public ToolResult<ThrowSimulationResult> SimulateThrow(
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

            var (newSiteswap, throwInfo) = siteswapObj.Throw();

            return new ThrowSimulationResult
            {
                OriginalSiteswap = SiteswapMapper.ToDisplayFormat(siteswapObj),
                NewSiteswap = SiteswapMapper.ToDisplayFormat(newSiteswap),
                ThrowValue = throwInfo.Value,
                StartingState = throwInfo.StartingState.ToString(),
                EndingState = throwInfo.EndingState.ToString(),
                PrettyPrint = throwInfo.PrettyPrint(),
            };
        });
    }
}

public class ThrowSimulationResult
{
    public string OriginalSiteswap { get; init; } = string.Empty;
    public string NewSiteswap { get; init; } = string.Empty;
    public int ThrowValue { get; init; }
    public string StartingState { get; init; } = string.Empty;
    public string EndingState { get; init; } = string.Empty;
    public string PrettyPrint { get; init; } = string.Empty;
}
