using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class GenerateStateGraphTool
{
    [McpServerTool]
    [Description(
        "Generates a state graph for a siteswap showing all state transitions. Returns nodes (states) and edges (transitions with throw values) that represent the complete state diagram."
    )]
    public ToolResult<StateGraphInfo> GenerateStateGraph(
        [Description("Siteswap string (e.g., '531', '441', 'a7242')")] string siteswap
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

            if (!SiteswapDetails.TryCreate(siteswap, out var siteswapObj))
            {
                throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
            }

            var stateGraph = StateGraphFromSiteswapGenerator.CalculateGraph(siteswapObj);
            var graph = stateGraph.Graph;

            return new StateGraphInfo
            {
                Siteswap = siteswapObj.ToString(),
                Nodes = graph.Nodes.Select(n => n.ToString()).ToList(),
                Edges = graph
                    .Edges.Select(e => new StateGraphEdge
                    {
                        FromState = e.N1.ToString(),
                        ToState = e.N2.ToString(),
                        ThrowValue = e.Data,
                    })
                    .ToList(),
            };
        });
    }
}

public class StateGraphInfo
{
    public string Siteswap { get; init; } = string.Empty;
    public List<string> Nodes { get; init; } = new();
    public List<StateGraphEdge> Edges { get; init; } = new();
}

public class StateGraphEdge
{
    public string FromState { get; init; } = string.Empty;
    public string ToState { get; init; } = string.Empty;
    public int ThrowValue { get; init; }
}
