using FluentAssertions;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public sealed class CausalDiagramEditorSequenceTests
{
    [Test]
    public void RunawayCatalog_PreservesOrderedPhasesAndFiniteEvents()
    {
        var sequence = CausalDiagramEditorSequences.All.Should().ContainSingle().Subject;
        sequence.Id.Should().Be("highgate-thirteenclubrunaway");
        sequence
            .Phases.Select(phase => phase.StateId)
            .Should()
            .Equal(
                "highgate-thirteenclubrunaway-initial-urban-terror",
                "highgate-thirteenclubrunaway-after-daniel-walk",
                "highgate-thirteenclubrunaway-before-anne-walk",
                "highgate-thirteenclubrunaway-after-anne-walk"
            );
        sequence.Transitions.Select(transition => transition.AfterPhaseIndex).Should().Equal(0, 2);
        sequence
            .Transitions.Select(transition => transition.Actor)
            .Should()
            .Equal("Daniel", "Anne");
        sequence
            .Transitions.Should()
            .OnlyContain(transition =>
                transition.DurationBeats == 2.5m && transition.SelfThrows == 3
            );
    }

    [Test]
    public void SequenceSelection_RoundTripsWithoutChangingPeriodicStateGraph()
    {
        var state = CausalDiagramEditorSequences.CreateRunawayPhase(
            "highgate-thirteenclubrunaway-before-anne-walk"
        );
        var originalGraph = state
            .Throws.Select(item =>
                (item.Id, item.JugglerId, item.Hand, item.StartTime, item.Height)
            )
            .ToArray();
        state.Version = 5;
        state.SequenceId = "highgate-thirteenclubrunaway";
        state.SequencePhaseIndex = 2;

        var token = state.Encode();
        token.Should().StartWith("j5.");
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeTrue();
        decoded!.SequenceId.Should().Be(state.SequenceId);
        decoded.SequencePhaseIndex.Should().Be(2);
        decoded.PresetId.Should().Be("highgate-thirteenclubrunaway-before-anne-walk");
        decoded
            .Throws.Select(item =>
                (item.Id, item.JugglerId, item.Hand, item.StartTime, item.Height)
            )
            .Should()
            .Equal(originalGraph);
        decoded.CausalEdges.Should().BeEquivalentTo(state.CausalEdges);
    }

    [Test]
    public void RunawayPhases_AreSelectablePeriodicPresetsAndValidate()
    {
        foreach (var phase in CausalDiagramEditorSequences.All.Single().Phases)
        {
            CausalDiagramEditorPresets.All.Should().Contain(preset => preset.Id == phase.StateId);
            var graph = CausalDiagramEditorSequences.CreateRunawayPhase(phase.StateId);
            graph.IsValidForEditor().Should().BeTrue(phase.StateId);
            graph.Throws.Should().HaveCount(24);
            graph.CausalEdges.Should().HaveCount(24);
        }
    }

    [Test]
    public void ExistingPeriodicLink_RemainsJ4AndDecodesWithoutSequenceMetadata()
    {
        var state = CausalDiagramEditorPresets
            .All.First(preset => preset.Id == "siteswap-531")
            .Create();
        var token = state.Encode();
        token.Should().StartWith("j4.");
        CausalDiagramEditorState.TryDecode(token, out var decoded).Should().BeTrue();
        decoded!.Version.Should().Be(4);
        decoded.SequenceId.Should().BeNull();
        decoded.SequencePhaseIndex.Should().BeNull();
    }

    [Test]
    public void SequenceSelection_RejectsUnknownSequenceAndOutOfRangePhase()
    {
        CausalDiagramEditorSequences.IsValidSelection("missing", 0).Should().BeFalse();
        CausalDiagramEditorSequences
            .IsValidSelection("highgate-thirteenclubrunaway", 4)
            .Should()
            .BeFalse();
    }
}
