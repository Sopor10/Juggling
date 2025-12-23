using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class AnalyzeSiteswapTool
{
    [McpServerTool]
    [Description(
        "Analyzes a siteswap and returns detailed information including orbits, states, period, number of objects, and other properties."
    )]
    public ToolResult<SiteswapAnalysis> AnalyzeSiteswap(
        [Description("The siteswap string to analyze (e.g., '531', '441', 'a7242')")]
            string siteswap
    )
    {
        return ToolResult.From(() =>
        {
            if (string.IsNullOrWhiteSpace(siteswap))
            {
                throw new ArgumentException(
                    "Siteswap string cannot be null or empty.",
                    nameof(siteswap)
                );
            }

            if (
                !Siteswap.Details.Siteswap.TryCreate(siteswap, out var siteswapObj)
                || siteswapObj == null
            )
            {
                throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
            }

            var orbits = siteswapObj.GetOrbits();
            var state = siteswapObj.State;
            var allStates = siteswapObj.AllStates();

            return new SiteswapAnalysis
            {
                Siteswap = siteswapObj.ToString(),
                Period = siteswapObj.Period.Value,
                NumberOfObjects = siteswapObj.NumberOfObjects(),
                MaxHeight = siteswapObj.Max(),
                Length = siteswapObj.Length,
                IsExcitedState = siteswapObj.IsExcitedState(),
                CurrentState = state.ToString(),
                Interface = siteswapObj.Interface.ToString(),
                Orbits = orbits
                    .Select(o => new OrbitInfo { DisplayValue = o.DisplayValue, Items = o.Items })
                    .ToList(),
                AllStates = allStates
                    .Select(kvp => new StateInfo
                    {
                        State = kvp.Key.ToString(),
                        Siteswaps = kvp.Value.Select(s => s.ToString()).ToList(),
                    })
                    .ToList(),
            };
        });
    }
}

public class SiteswapAnalysis
{
    public string Siteswap { get; set; } = string.Empty;
    public int Period { get; set; }
    public decimal NumberOfObjects { get; set; }
    public int MaxHeight { get; set; }
    public int Length { get; set; }
    public bool IsExcitedState { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public List<OrbitInfo> Orbits { get; set; } = new();
    public List<StateInfo> AllStates { get; set; } = new();
    public required string Interface { get; set; }
}

public class OrbitInfo
{
    public string DisplayValue { get; set; } = string.Empty;
    public List<int> Items { get; set; } = new();
}

public class StateInfo
{
    public string State { get; set; } = string.Empty;
    public List<string> Siteswaps { get; set; } = new();
}
