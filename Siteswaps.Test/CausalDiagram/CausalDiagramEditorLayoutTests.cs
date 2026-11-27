using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Components.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public sealed class CausalDiagramEditorLayoutTests
{
    [Test]
    public void SimultaneousHandsUseDistinctTracksAndTouchTargets()
    {
        var state = new CausalDiagramEditorState
        {
            LaneSpacing = 80m,
            Jugglers = [new EditorJuggler("A", "Anne"), new EditorJuggler("B", "Ben")],
            Throws =
            [
                new EditorThrow("A-R", "A", "R", 0m),
                new EditorThrow("A-L", "A", "L", 0m),
                new EditorThrow("B-R", "B", "R", 0m),
                new EditorThrow("B-L", "B", "L", 0m),
            ],
        };

        var left = CausalDiagramEditorLayout.ThrowY(state, state.Throws[1]);
        var right = CausalDiagramEditorLayout.ThrowY(state, state.Throws[0]);
        var nextJugglerLeft = CausalDiagramEditorLayout.ThrowY(state, state.Throws[3]);

        CausalDiagramEditorLayout.HasHandTracks(state, "A").Should().BeTrue();
        CausalDiagramEditorLayout.EffectiveLaneSpacing(state).Should().Be(120m);
        Math.Abs(right - left)
            .Should()
            .BeGreaterThanOrEqualTo(2 * CausalDiagramEditorLayout.ThrowHitTargetRadius);
        Math.Abs(nextJugglerLeft - right)
            .Should()
            .BeGreaterThanOrEqualTo(2 * CausalDiagramEditorLayout.ThrowHitTargetRadius);
    }

    [Test]
    public void AsynchronousThrowsKeepTheOriginalJugglerLane()
    {
        var state = new CausalDiagramEditorState
        {
            LaneSpacing = 80m,
            Jugglers = [new EditorJuggler("A", "Anne")],
            Throws = [new EditorThrow("A-R", "A", "R", 0m), new EditorThrow("A-L", "A", "L", 1m)],
        };

        CausalDiagramEditorLayout.HasHandTracks(state, "A").Should().BeFalse();
        CausalDiagramEditorLayout
            .ThrowY(state, state.Throws[0])
            .Should()
            .Be(CausalDiagramEditorLayout.ThrowY(state, state.Throws[1]));
        CausalDiagramEditorLayout.EffectiveLaneSpacing(state).Should().Be(80m);
    }
}
