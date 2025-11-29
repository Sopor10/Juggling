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
    [Description("Simulates a single throw in a siteswap and returns the resulting state and siteswap. This shows what happens when one throw is executed.")]
    public ThrowSimulationResult SimulateThrow(
        [Description("Siteswap string (e.g., '531', '441', 'a7242')")] string siteswap)
    {
        if (string.IsNullOrWhiteSpace(siteswap))
        {
            throw new ArgumentException("Siteswap string cannot be null or empty.", nameof(siteswap));
        }

        if (!SiteswapDetails.TryCreate(siteswap, out var siteswapObj))
        {
            throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
        }

        var (newSiteswap, throwInfo) = siteswapObj.Throw();

        return new ThrowSimulationResult
        {
            OriginalSiteswap = siteswapObj.ToString(),
            NewSiteswap = newSiteswap.ToString(),
            ThrowValue = throwInfo.Value,
            StartingState = throwInfo.StartingState.ToString(),
            EndingState = throwInfo.EndingState.ToString(),
            PrettyPrint = throwInfo.PrettyPrint()
        };
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

