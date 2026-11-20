using FluentAssertions;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;
using Siteswap.Details.Jif;
using Siteswaps.Components.CausalDiagram;
using SiteswapPattern = global::Siteswap.Details.Siteswap;

namespace Siteswaps.Test.CausalDiagram;

public sealed class CausalDiagramEditorJifAdapterTests
{
    [Test]
    public void PolyrhythmPresets_AllProducePassistAnimations()
    {
        var presets = new[]
        {
            CausalDiagramEditorState.CreateFeedingPattern(3, 3, "feeding-3-3"),
            CausalDiagramEditorState.CreateFeedingPattern(5, 3, "feeding-5-3"),
            CausalDiagramEditorState.CreateFeeding75(),
        };

        foreach (var state in presets)
            CausalDiagramEditorJifAdapter
                .TryMap(state, out _, out var error)
                .Should()
                .BeTrue(error);
    }

    [Test]
    public void Feeding75_ExpandsIndependentTrackPeriodsToBalancedCommonPeriod()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();

        CausalDiagramEditorJifAdapter
            .TryCreateDiagramSet(state, out var diagram, out var error)
            .Should()
            .BeTrue(error);
        diagram.Should().NotBeNull();
        diagram!.StepCount.Should().Be(35);
        diagram.TimeStretchFactor.Should().Be(1m);
        diagram.CausalArrows.Should().HaveCount(12);
        diagram.Throws.Should().HaveCount(12);
        diagram.Throws.Count(item => item.Hand.Person.Name == "A").Should().Be(7);
        diagram.Throws.Count(item => item.Hand.Person.Name == "B").Should().Be(5);
        diagram.Throws.Should().OnlyContain(item => item.ToLimb.HasValue);
        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
        mapped!.Jugglers.Should().HaveCount(2);
        mapped.Repetition.Should().NotBeNull();
        mapped.Repetition!.Period.Should().BeGreaterThanOrEqualTo(35);
        (mapped.Repetition.Period % 35).Should().Be(0);
        mapped.Throws.Should().HaveCount(12 * mapped.Repetition.Period!.Value / 35);
        mapped.Throws.Should().HaveCount(24);
    }

    [Test]
    public void Siteswap3_ManualHandAssignmentIsAuthoritativeInGenericMapping()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("3");
        state.Throws[0] = state.Throws[0] with { Hand = "L" };

        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out var error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
        mapped!.Throws!.OrderBy(item => item.Time).First().From.Should().Be(1);
    }

    [Test]
    public void Siteswap531_MapsToJifUsingTheSameDiagramSetPath()
    {
        var state = CausalDiagramEditorState.CreateSiteswap531();

        CausalDiagramEditorJifAdapter
            .TryCreateDiagramSet(state, out var diagram, out var error)
            .Should()
            .BeTrue(error);
        diagram.Should().NotBeNull();
        diagram!.CausalArrows.Should().HaveCount(5);
        diagram
            .StepCount.Should()
            .Be(SiteswapDiagramBuilder.ComputeSteps(new SiteswapPattern(5, 3, 1), 2));

        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
        var expectedDiagram = SiteswapDiagramBuilder.Build(new SiteswapPattern(5, 3, 1), 1);
        var expected = JifMapper.Map(
            expectedDiagram,
            propType: "club",
            patternType: "passingSiteswap"
        );
        mapped!.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Siteswap3_UsesAlternatingLandingLimbsAndThreeClubOrbit()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("3");
        CausalDiagramEditorJifAdapter
            .TryCreateDiagramSet(state, out var diagram, out var error)
            .Should()
            .BeTrue(error);
        diagram.Should().NotBeNull();
        diagram!.StepCount.Should().Be(6);
        diagram.Throws.Should().HaveCount(6);
        diagram.Throws.Select(item => item.ToLimb).Should().ContainInOrder(1, 0, 1, 0, 1, 0);
        diagram
            .Throws.Select(item => item.Hand.Name)
            .Should()
            .ContainInOrder("R", "L", "R", "L", "R", "L");

        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
        var expectedDiagram = SiteswapDiagramBuilder.Build(new SiteswapPattern(3), 1);
        var expected = JifMapper.Map(
            expectedDiagram,
            propType: "club",
            patternType: "passingSiteswap"
        );
        mapped!.Props.Should().HaveCount(3);
        mapped.Repetition!.Period.Should().Be(expected.Repetition!.Period);
        mapped.Repetition.PropPermutation.Should().Equal(expected.Repetition!.PropPermutation!);
        mapped.Repetition.LimbPermutation.Should().Equal(expected.Repetition.LimbPermutation!);
        mapped.Throws.Should().BeEquivalentTo(expected.Throws);
        mapped.Throws.Select(item => item.From).Should().Contain(0).And.Contain(1);
        mapped.Throws.Select(item => item.To).Should().Contain(0).And.Contain(1);
    }

    [Test]
    public void Siteswap756_UsesHandExactPeriodAndClosesCombinedLimbAndPropOrbit()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("756");
        var siteswap = new SiteswapPattern(7, 5, 6);
        var diagram = SiteswapDiagramBuilder.Build(siteswap, numberOfJugglers: 1);
        diagram.StepCount.Should().Be(12);
        diagram.Throws.Should().HaveCount(12);
        diagram
            .Throws.Take(6)
            .Select(item => item.Hand.Name)
            .Should()
            .Equal("R", "L", "R", "L", "R", "L");
        diagram
            .Throws.Skip(6)
            .Select(item => item.Hand.Name)
            .Should()
            .Equal("R", "L", "R", "L", "R", "L");
        diagram
            .Throws.Take(6)
            .Select(item => item.Height)
            .Should()
            .Equal(diagram.Throws.Skip(6).Select(item => item.Height));
        diagram
            .Throws.Take(6)
            .Select(item => item.ToLimb)
            .Should()
            .Equal(diagram.Throws.Skip(6).Select(item => item.ToLimb));

        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out var error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
        mapped!.Repetition.Should().NotBeNull();
        var expected = JifMapper.Map(diagram, propType: "club", patternType: "passingSiteswap");
        mapped.Repetition!.Period.Should().Be(expected.Repetition!.Period);
        mapped.Repetition.Period.Should().BeGreaterThanOrEqualTo(diagram.StepCount);
        (mapped.Repetition.Period % 6).Should().Be(0);
        mapped.Repetition.LimbPermutation.Should().Equal(expected.Repetition.LimbPermutation!);
        mapped.Repetition.PropPermutation.Should().Equal(expected.Repetition.PropPermutation!);
        mapped.Repetition.LimbPermutation.Should().HaveCount(2);
        mapped.Repetition.PropPermutation.Should().HaveCount(mapped.Props!.Count);
        mapped.Throws.Should().NotBeEmpty();
    }

    [Test]
    public void IncompleteGraph_ReturnsUnavailableReasonWithoutThrowing()
    {
        var state = CausalDiagramEditorState.CreateEmpty();
        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out var error)
            .Should()
            .BeFalse();
        mapped.Should().BeNull();
        error.Should().NotBeNullOrWhiteSpace();
    }
}
