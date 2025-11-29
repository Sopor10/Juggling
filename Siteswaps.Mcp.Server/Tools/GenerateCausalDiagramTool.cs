using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class GenerateCausalDiagramTool
{
    [McpServerTool]
    [Description("Generates a causal diagram representation of a siteswap showing ball movements between hands. Returns nodes (throws) and transitions (ball paths).")]
    public CausalDiagramInfo GenerateCausalDiagram(
        [Description("Siteswap string (e.g., '531', '441', 'a7242')")] string siteswap,
        [Description("Number of hands (default: 2)")] int numberOfHands = 2)
    {
        if (string.IsNullOrWhiteSpace(siteswap))
        {
            throw new ArgumentException("Siteswap string cannot be null or empty.", nameof(siteswap));
        }

        if (numberOfHands < 1)
        {
            throw new ArgumentException("Number of hands must be at least 1.", nameof(numberOfHands));
        }

        if (!SiteswapDetails.TryCreate(siteswap, out var siteswapObj))
        {
            throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
        }

        // Create hands for a single juggler
        var hands = new List<Hand>();
        var person = new Person("A");
        var handNames = new[] { "R", "L" };
        for (var i = 0; i < numberOfHands; i++)
        {
            var handName = i < handNames.Length ? handNames[i] : $"H{i + 1}";
            hands.Add(new Hand(handName, person));
        }

        var generator = new CausalDiagramGenerator();
        var diagram = generator.Generate(siteswapObj, hands.ToCyclicArray());

        return new CausalDiagramInfo
        {
            Siteswap = siteswapObj.ToString(),
            NumberOfHands = numberOfHands,
            Throws = diagram.Throws.Select(t => new CausalDiagramThrow
            {
                Hand = t.Hand.Name,
                Person = t.Hand.Person.Name,
                Height = t.Height,
                Time = t.Time
            }).ToList(),
            Transitions = diagram.Transitions.Select(tr => new CausalDiagramTransition
            {
                FromHand = tr.Start.Hand.Name,
                FromPerson = tr.Start.Hand.Person.Name,
                FromHeight = tr.Start.Height,
                FromTime = tr.Start.Time,
                ToHand = tr.End.Hand.Name,
                ToPerson = tr.End.Hand.Person.Name,
                ToHeight = tr.End.Height,
                ToTime = tr.End.Time
            }).ToList(),
            MaxTime = diagram.MaxTime
        };
    }
}

public class CausalDiagramInfo
{
    public string Siteswap { get; init; } = string.Empty;
    public int NumberOfHands { get; init; }
    public List<CausalDiagramThrow> Throws { get; init; } = new();
    public List<CausalDiagramTransition> Transitions { get; init; } = new();
    public decimal MaxTime { get; init; }
}

public class CausalDiagramThrow
{
    public string Hand { get; init; } = string.Empty;
    public string Person { get; init; } = string.Empty;
    public int Height { get; init; }
    public decimal Time { get; init; }
}

public class CausalDiagramTransition
{
    public string FromHand { get; init; } = string.Empty;
    public string FromPerson { get; init; } = string.Empty;
    public int FromHeight { get; init; }
    public decimal FromTime { get; init; }
    public string ToHand { get; init; } = string.Empty;
    public string ToPerson { get; init; } = string.Empty;
    public int ToHeight { get; init; }
    public decimal ToTime { get; init; }
}

