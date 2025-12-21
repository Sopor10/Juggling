using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class GenerateTransitionGraphTool
{
    [McpServerTool]
    [Description(
        "Generates a transition graph for a list of siteswaps showing all possible transitions between them. Returns nodes (siteswaps) and edges (transitions)."
    )]
    public ToolResult<TransitionGraphInfo> GenerateTransitionGraph(
        [Description("Comma-separated list of siteswaps (e.g., '531,441,423')")] string siteswaps,
        [Description("Maximum transition length (number of throws in transition paths)")]
            int maxLength
    )
    {
        return ToolResult.From(() =>
        {
            if (string.IsNullOrWhiteSpace(siteswaps))
            {
                throw new ArgumentException(
                    "Siteswaps string cannot be null or empty.",
                    nameof(siteswaps)
                );
            }

            if (maxLength < 1)
            {
                throw new ArgumentException(
                    "Maximum transition length must be at least 1.",
                    nameof(maxLength)
                );
            }

            var siteswapStrings = siteswaps.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            if (siteswapStrings.Length == 0)
            {
                throw new ArgumentException(
                    "At least one siteswap must be provided.",
                    nameof(siteswaps)
                );
            }

            var siteswapList = new List<SiteswapDetails>();
            foreach (var siteswapStr in siteswapStrings)
            {
                if (!SiteswapDetails.TryCreate(siteswapStr, out var siteswapObj))
                {
                    throw new ArgumentException($"Invalid siteswap: {siteswapStr}", nameof(siteswaps));
                }

                siteswapList.Add(siteswapObj);
            }

            var siteswapListObj = new SiteswapList(siteswapList.ToArray());
            var graph = siteswapListObj.TransitionGraph(maxLength);

            return new TransitionGraphInfo
            {
                Siteswaps = siteswaps,
                MaxLength = maxLength,
                Nodes = graph.Nodes.Select(n => n.ToString()).ToList(),
                Edges = graph
                    .Edges.Select(e => new TransitionGraphEdge
                    {
                        FromSiteswap = e.N1.ToString(),
                        ToSiteswap = e.N2.ToString(),
                        Transition = e.Data.PrettyPrint(),
                    })
                    .ToList(),
            };
        });
    }
}

public class TransitionGraphInfo
{
    public string Siteswaps { get; init; } = string.Empty;
    public int MaxLength { get; init; }
    public List<string> Nodes { get; init; } = new();
    public List<TransitionGraphEdge> Edges { get; init; } = new();
}

public class TransitionGraphEdge
{
    public string FromSiteswap { get; init; } = string.Empty;
    public string ToSiteswap { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
}
