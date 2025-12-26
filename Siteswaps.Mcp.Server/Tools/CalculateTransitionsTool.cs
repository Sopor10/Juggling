using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class CalculateTransitionsTool
{
    [McpServerTool]
    [Description(
        "Calculates all possible transitions between two siteswaps. Returns a list of transition paths showing how to move from the source siteswap to the target siteswap."
    )]
    public ToolResult<List<TransitionInfo>> CalculateTransitions(
        [Description("Source siteswap string (e.g., '531', '441')")] string fromSiteswap,
        [Description("Target siteswap string (e.g., '531', '441')")] string toSiteswap,
        [Description("Maximum transition length (number of throws in the transition path)")]
            int maxLength,
        [Description("Maximum throw height (optional, defaults to max of both siteswaps)")]
            int? maxHeight = null
    )
    {
        return ToolResult.From(() =>
        {
            if (string.IsNullOrWhiteSpace(fromSiteswap))
            {
                throw new ArgumentException(
                    "Source siteswap cannot be null or empty.",
                    nameof(fromSiteswap)
                );
            }

            if (string.IsNullOrWhiteSpace(toSiteswap))
            {
                throw new ArgumentException(
                    "Target siteswap cannot be null or empty.",
                    nameof(toSiteswap)
                );
            }

            if (maxLength < 0)
            {
                throw new ArgumentException(
                    "Maximum transition length must be non-negative.",
                    nameof(maxLength)
                );
            }

            if (!SiteswapDetails.TryCreate(fromSiteswap, out var from))
            {
                throw new ArgumentException(
                    $"Invalid source siteswap: {fromSiteswap}",
                    nameof(fromSiteswap)
                );
            }

            if (!SiteswapDetails.TryCreate(toSiteswap, out var to))
            {
                throw new ArgumentException(
                    $"Invalid target siteswap: {toSiteswap}",
                    nameof(toSiteswap)
                );
            }

            if (from.NumberOfObjects != to.NumberOfObjects)
            {
                throw new ArgumentException(
                    $"Source and target must use the same number of objects (from: {from.NumberOfObjects}, to: {to.NumberOfObjects}).",
                    nameof(toSiteswap)
                );
            }

            var transitions = TransitionCalculator.CreateTransitions(
                from,
                to,
                maxLength,
                maxHeight
            );

            return transitions
                .Select(t => new TransitionInfo
                {
                    FromSiteswap = t.From.ToString(),
                    ToSiteswap = t.To.ToString(),
                    Throws = t
                        .Throws.Select(th => new ThrowInfo
                        {
                            Value = th.Value,
                            StartingState = th.StartingState.ToString(),
                            EndingState = th.EndingState.ToString(),
                        })
                        .ToList(),
                    Length = t.Throws.Length,
                    PrettyPrint = t.PrettyPrint(),
                    IsMinimal = t.IsMinimal,
                })
                .ToList();
        });
    }
}

public class TransitionInfo
{
    public string FromSiteswap { get; init; } = string.Empty;
    public string ToSiteswap { get; init; } = string.Empty;
    public List<ThrowInfo> Throws { get; init; } = new();
    public int Length { get; init; }
    public string PrettyPrint { get; init; } = string.Empty;
    public bool IsMinimal { get; init; }
}

public class ThrowInfo
{
    public int Value { get; init; }
    public string StartingState { get; init; } = string.Empty;
    public string EndingState { get; init; } = string.Empty;
}
