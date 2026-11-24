extern alias JifDetails;
using System.Collections.Immutable;

using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.SiteswapLab;
using CausalStartPropsCalculator = JifDetails::Siteswap.Details.CausalDiagram.CausalStartPropsCalculator;
using PassingDiagramBuilder = JifDetails::Siteswap.Details.CausalDiagram.PassingDiagramBuilder;
using Siteswap = JifDetails::Siteswap.Details.Siteswap;
using SiteswapDiagramBuilder = JifDetails::Siteswap.Details.CausalDiagram.SiteswapDiagramBuilder;
using StartProps = JifDetails::Siteswap.Details.CausalDiagram.StartProps;

namespace Siteswaps.Generator.Test.Feeding;

[TestFixture]
public class CausalStartPropsTests
{
    [Test]
    public void Causal_roots_differ_from_existing_inventory_for_three_person_screenshot_pattern()
    {
        var timeZones = new[] { 0, 1, 1 };
        IReadOnlyList<int>[] heights =
        [
            [9, 6, 6, 6, 6],
            [6, 3, 6, 6, 6],
            [6, 6, 6, 6, 6],
        ];
        IReadOnlyList<int>[] targets =
        [
            [1, 0, 0, 0, 0],
            [1, 0, 1, 1, 1],
            [2, 2, 2, 2, 2],
        ];

        var graphCounts = GraphCounts(timeZones, heights, targets);
        var inventory = InventoryFor(timeZones, heights, targets);
        graphCounts["A"].Should().Be(new StartProps(Left: 0, Right: 1));
        graphCounts["B"].Should().Be(new StartProps(Left: 0, Right: 1));
        graphCounts["C"].Should().Be(new StartProps(Left: 0, Right: 1));
        inventory.Values.Sum(count => count.Left + count.Right).Should().Be(9);
        graphCounts.Values.Sum(count => count.Left + count.Right).Should().Be(3);
    }

    [Test]
    public void Causal_roots_differ_from_existing_inventory_for_two_person_feeding()
    {
        var timeZones = new[] { 0, 1 };
        IReadOnlyList<int>[] heights =
        [
            [5, 6],
            [9, 4],
        ];
        IReadOnlyList<int>[] targets =
        [
            [1, 0],
            [0, 1],
        ];

        var graphCounts = GraphCounts(timeZones, heights, targets);
        var inventory = InventoryFor(timeZones, heights, targets);
        graphCounts["A"].Should().Be(new StartProps(Left: 1, Right: 1));
        graphCounts["B"].Should().Be(new StartProps(Left: 0, Right: 0));
        inventory["A"].Should().Be(new ClubHands(Left: 1, Right: 2));
        inventory["B"].Should().Be(new ClubHands(Left: 2, Right: 1));
        graphCounts.Values.Sum(count => count.Left + count.Right).Should().Be(2);
        inventory.Values.Sum(count => count.Left + count.Right).Should().Be(6);
    }

    [TestCase("3", 1)]
    [TestCase("531", 1)]
    [TestCase("97522", 3)]
    [TestCase("441", 1)]
    public void Causal_root_count_is_repeatable_for_valid_siteswaps(
        string notation,
        int expectedRoots
    )
    {
        Siteswap.TryCreate(notation, out var siteswap).Should().BeTrue();
        var diagram = SiteswapDiagramBuilder.Build(siteswap!, numberOfJugglers: 1);
        var graphCounts = CausalStartPropsCalculator.Calculate(
            diagram.Throws,
            diagram.CausalArrows,
            diagram.Jugglers
        );

        graphCounts.Values.Sum(count => count.Left + count.Right).Should().Be(expectedRoots);
    }

    [Test]
    public void No_incoming_arrow_heuristic_misses_clubs_in_closed_causal_cycles()
    {
        Siteswap.TryCreate("2", out var siteswap).Should().BeTrue();
        var diagram = SiteswapDiagramBuilder.Build(siteswap!, numberOfJugglers: 1);
        var inferred = CausalStartPropsCalculator.Calculate(
            diagram.Throws,
            diagram.CausalArrows,
            diagram.Jugglers
        );

        inferred.Values.Sum(count => count.Left + count.Right).Should().Be(0);
        diagram.StartProps["A"].Should().Be(new StartProps(Left: 1, Right: 1));
    }

    [Test]
    public void Natural_remainder_fills_closed_cycle_for_siteswap_two()
    {
        Siteswap.TryCreate("2", out var siteswap).Should().BeTrue();
        var diagram = SiteswapDiagramBuilder.Build(siteswap!, numberOfJugglers: 1);
        var inferred = CausalStartPropsCalculator.CalculateWithNaturalRemainder(
            diagram.Throws,
            diagram.CausalArrows,
            diagram.Jugglers,
            (int)siteswap!.NumberOfObjects()
        );

        inferred.Should().BeEquivalentTo(diagram.StartProps);
    }

    private static ImmutableDictionary<string, StartProps> GraphCounts(
        int[] timeZones,
        IReadOnlyList<int>[] heights,
        IReadOnlyList<int>[] targets
    )
    {
        var diagram = PassingDiagramBuilder.Build(timeZones, heights, targets);
        return CausalStartPropsCalculator.Calculate(
            diagram.Throws,
            diagram.CausalArrows,
            diagram.Jugglers
        );
    }

    private static Dictionary<string, ClubHands> InventoryFor(
        int[] timeZones,
        IReadOnlyList<int>[] heights,
        IReadOnlyList<int>[] targets
    )
    {
        var period = heights[0].Count;
        var state = new PassingEditorState(string.Concat(Enumerable.Repeat('3', period)));
        state.SetPersonCount(timeZones.Length);
        for (var person = 0; person < timeZones.Length; person++)
        {
            while (state.People[person].TimeZone != timeZones[person])
                state.CycleTimeZone(person);
        }

        for (var person = 0; person < timeZones.Length; person++)
        {
            for (var beat = 0; beat < period; beat++)
                state.SetHeight(person, beat, heights[person][beat]);
        }

        for (var person = 0; person < timeZones.Length; person++)
        {
            for (var beat = 0; beat < period; beat++)
                state.SetTarget(person, beat, targets[person][beat]).Should().BeTrue();
        }

        return state
            .People.Select((person, index) => (person.Name, Clubs: state.StartingClubsFor(index)))
            .ToDictionary(entry => entry.Name, entry => entry.Clubs);
    }
}
