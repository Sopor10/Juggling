using FluentAssertions;
using FsCheck;
using Microsoft.FSharp.Core;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;
using Siteswap.Details.Jif;
using Siteswaps.Components.CausalDiagram;
using SiteswapPattern = global::Siteswap.Details.Siteswap;

namespace Siteswaps.Test.CausalDiagram;

public sealed class CausalDiagramEditorJifAdapterTests
{
    [Test]
    public void ValidFactoryGeneratedPatternsIncludingThirdPhases_MapToPeriodicJif()
    {
        var testedThirdPhase = false;
        CheckProperty(CausalPatternSeed.Arbitrary, sample =>
        {
            var state = CreateFactoryGeneratedPattern(sample.Value);
            testedThirdPhase |= state.PresetId == "jugglers-3";
            if (
                !CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded)
                || decoded is null
            )
                throw new InvalidOperationException($"Generated state is invalid: {sample}.");

            if (
                !CausalDiagramEditorJifAdapter.TryMap(decoded, out var mapped, out var error)
                || mapped is not { Valid: true, Throws.Count: > 0, Repetition.Period: > 0 }
            )
                throw new InvalidOperationException($"{sample}: {error}");
            var table = CausalDiagramEditorTableAdapter.Map(decoded);
            if (
                table.Rows.Count != decoded.Jugglers.Count
                || table.Rows.Sum(row => row.Cells.Sum(chips => chips?.Count ?? 0))
                    != decoded.Throws.Count * decoded.Repetitions
            )
                throw new InvalidOperationException(
                    $"{sample}: the editor table lost juggler rows or repeated throws."
                );
            for (var jugglerIndex = 0; jugglerIndex < decoded.Jugglers.Count; jugglerIndex++)
            {
                var juggler = decoded.Jugglers[jugglerIndex];
                var actualTimes = table
                    .Times.Where(
                        (_, timeIndex) => table.Rows[jugglerIndex].Cells[timeIndex] is not null
                    )
                    .ToList();
                var expectedTimes = Enumerable
                    .Range(0, decoded.Repetitions)
                    .SelectMany(cycle =>
                        decoded
                            .Throws.Where(item => item.JugglerId == juggler.Id)
                            .Select(item => item.StartTime + cycle * decoded.CycleLength)
                    )
                    .Order()
                    .ToList();
                if (!actualTimes.SequenceEqual(expectedTimes))
                    throw new InvalidOperationException(
                        $"{sample}: the table changed the timing for {juggler.Name}."
                    );
            }
            return true;
        });
        testedThirdPhase.Should().BeTrue();
    }

    [Test]
    public void EditorTable_PreservesAsynchronousOffsetsAndAllVisibleRepetitions()
    {
        var state = CausalDiagramEditorPresets
            .All.Single(preset => preset.Id == "jugglers-2")
            .Create();
        var table = CausalDiagramEditorTableAdapter.Map(state);

        table.Times.Should().Contain(0.5m);
        table.Times.Should().Contain(state.CycleLength);
        table.Rows.Should().HaveCount(2);
        table
            .Rows.SelectMany(row => row.Cells)
            .Sum(cell => cell?.Count ?? 0)
            .Should()
            .Be(state.Throws.Count * state.Repetitions);
        table.Rows[0].Cells.Any(cell => cell is null).Should().BeTrue();
        table.Rows[1].Cells.Any(cell => cell is null).Should().BeTrue();
    }

    [Test]
    public void MultiplexBeat_PreservesAllThrowsInTableAndPassistAnimation()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();
        CausalDiagramEditorJifAdapter
            .TryMap(state, out var baseline, out var baselineError)
            .Should()
            .BeTrue(baselineError);
        var source = state.Throws.Single(node => node.Id == "P1-1");
        var edge = state.CausalEdges.Single(candidate => candidate.FromThrowId == source.Id);
        const string duplicateId = "P1-multiplex";
        state.Throws.Add(source with { Id = duplicateId });
        state.CausalEdges.Add(edge with { Id = "e-P1-multiplex", FromThrowId = duplicateId });

        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();
        var multiplex = decoded!
            .Throws.Where(node =>
                node.JugglerId == source.JugglerId && node.StartTime == source.StartTime
            )
            .ToArray();
        multiplex.Should().HaveCount(2);
        var table = CausalDiagramEditorTableAdapter.Map(decoded);
        var beatColumn = table.Times.ToList().IndexOf(source.StartTime);
        table.Rows[0].Cells[beatColumn].Should().HaveCount(2);
        table.Rows[0].Cells[beatColumn]!.Should().OnlyContain(chip => chip.Hand != null);

        CausalDiagramEditorJifAdapter
            .TryMap(decoded, out var mapped, out var error)
            .Should()
            .BeTrue(error);
        mapped!.Throws.Should().NotBeEmpty();
        mapped
            .Throws!.Count(throwEvent => throwEvent.Time == source.StartTime)
            .Should()
            .Be(baseline!.Throws!.Count(throwEvent => throwEvent.Time == source.StartTime) + 1);
    }

    [Test]
    public void Polyrhythm23_MapsToPassistWithHandAssignmentsFromLandingTimes()
    {
        var state = CausalDiagramEditorState.CreateFeedingPattern(2, 3, "custom");

        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out var error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
        mapped!.Props.Should().HaveCount(2);
        mapped
            .Throws!.OrderBy(item => item.Time)
            .Take(5)
            .Select(item => item.From)
            .Should()
            .Equal(0, 1, 2, 3, 1);
        mapped
            .Throws!.OrderBy(item => item.Time)
            .Take(5)
            .Select(item => item.To)
            .Should()
            .Equal(3, 2, 1, 0, 2);
    }

    [Test]
    public void OtherCompletePolyrhythmFactoryPatterns_MapToPassist()
    {
        foreach (
            var state in new[]
            {
                CausalDiagramEditorState.CreateFeedingPattern(3, 3, "feeding-3-3"),
                CausalDiagramEditorState.CreateFeedingPattern(5, 3, "feeding-5-3"),
            }
        )
            CausalDiagramEditorJifAdapter
                .TryMap(state, out _, out var error)
                .Should()
                .BeTrue(error);
    }

    [Test]
    public void MultipleIncomingArrows_MapToPassist()
    {
        var state = CausalDiagramEditorState.CreateSiteswapPattern("531");
        var edgeIndex = state.CausalEdges.FindIndex(edge => edge.FromThrowId == "A4");
        state.CausalEdges[edgeIndex] = state.CausalEdges[edgeIndex] with
        {
            ToThrowId = "A3",
            CycleOffset = 0,
        };

        CausalDiagramEditorState.TryDecode(state.Encode(), out _).Should().BeTrue();
        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out var error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
    }

    [Test]
    public void JugglerWithoutThrowsPausesAnimationWithoutThrowing()
    {
        var state = CausalDiagramEditorState.CreateFeedingPattern(2, 3, "custom");
        state.Jugglers.Add(new EditorJuggler("J3", "Jongleur 3"));

        CausalDiagramEditorJifAdapter.TryMap(state, out _, out var error).Should().BeFalse();
        error.Should().Contain("mindestens einen Wurf");
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
        mapped!.Throws!.OrderBy(item => item.Time).First().From.Should().Be(0);
    }

    [Test]
    public void DiagramHandSequenceDrivesPassistLimbAssignments()
    {
        var state = new CausalDiagramEditorState
        {
            PresetId = "custom",
            ActiveJugglerId = "P1",
            CycleLength = 4m,
            Jugglers = [new("P1", "A"), new("P2", "B")],
            Throws =
            [
                new("P1-1", "P1", "R", 0m),
                new("P1-2", "P1", "L", 1m),
                new("P1-3", "P1", "R", 2m),
                new("P1-4", "P1", "L", 3m),
                new("P2-1", "P2", "R", 0.5m),
                new("P2-2", "P2", "R", 1.5m),
                new("P2-3", "P2", "L", 2.5m),
                new("P2-4", "P2", "L", 3.5m),
            ],
            CausalEdges =
            [
                new("e1", "P1-1", "P2-1"),
                new("e2", "P1-2", "P2-2"),
                new("e3", "P1-3", "P2-3"),
                new("e4", "P1-4", "P2-4"),
                new("e5", "P2-1", "P1-2"),
                new("e6", "P2-2", "P1-3"),
                new("e7", "P2-3", "P1-4"),
                new("e8", "P2-4", "P1-1", 1),
            ],
        };

        var chronological = state.Throws.OrderBy(node => node.StartTime).ToArray();
        chronological
            .Select(node => state.HeightOf(node.Id))
            .Should()
            .OnlyContain(height => height == 5m);
        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out var error)
            .Should()
            .BeTrue(error);
        mapped!
            .Throws!.OrderBy(item => item.Time)
            .Take(8)
            .Select(item => item.From)
            .Should()
            .Equal(0, 1, 2, 1, 0, 3, 2, 3);
        mapped
            .Throws!.OrderBy(item => item.Time)
            .Take(8)
            .Select(item => item.To)
            .Should()
            .Equal(3, 2, 3, 0, 1, 2, 1, 0);
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
        diagram!.CausalArrows.Should().HaveCount(6);
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
            .Select(item => diagram.HeightOf(item))
            .Should()
            .Equal(diagram.Throws.Skip(6).Select(item => diagram.HeightOf(item)));
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

    private static CausalDiagramEditorState CreateFactoryGeneratedPattern(int seed)
    {
        var random = new Random(seed);
        CausalDiagramEditorState state;
        var variant = random.Next(4);
        if (variant == 0)
            return CausalDiagramEditorPresets
                .All.Single(preset => preset.Id == "jugglers-3")
                .Create();
        if (variant == 1)
        {
            var validSiteswaps = new[] { "3", "4", "5", "531", "441", "423", "97531", "756" };
            state = CausalDiagramEditorState.CreateSiteswapPattern(
                validSiteswaps[random.Next(validSiteswaps.Length)]
            );
        }
        else if (variant == 2)
        {
            state = CausalDiagramEditorState.CreateFeedingPattern(
                random.Next(2, 9),
                random.Next(2, 9),
                "custom"
            );
        }
        else
        {
            var jugglerCount = random.Next(2, 6);
            var beatsPerJuggler = random.Next(2, 9);
            state = new CausalDiagramEditorState
            {
                PresetId = "custom",
                ActiveJugglerId = "P1",
                CycleLength = beatsPerJuggler,
                Repetitions = 2,
                Jugglers = Enumerable
                    .Range(0, jugglerCount)
                    .Select(index => new EditorJuggler(
                        $"P{index + 1}",
                        ((char)('A' + index)).ToString()
                    ))
                    .ToList(),
            };
            foreach (var juggler in state.Jugglers)
                for (var beat = 0; beat < beatsPerJuggler; beat++)
                {
                    var id = $"{juggler.Id}-{beat + 1}";
                    state.Throws.Add(
                        new EditorThrow(id, juggler.Id, beat % 2 == 0 ? "R" : "L", beat)
                    );
                    state.CausalEdges.Add(new EditorCausalEdge($"self-{id}", id, id, 1));
                }
        }
        return state;
    }

    private static void CheckProperty<T>(Arbitrary<T> arbitrary, Func<T, bool> property)
    {
        var fsCheckProperty = FsCheck.FSharp.Prop.ForAll(
            arbitrary,
            FuncConvert.ToFSharpFunc(new Converter<T, bool>(property))
        );
        Check.One(Config.QuickThrowOnFailure, fsCheckProperty);
    }

    private sealed record CausalPatternSeed(int Value)
    {
        public override string ToString() => $"causal pattern seed {Value}";

        public static Arbitrary<CausalPatternSeed> Arbitrary { get; } =
            new DelegateArbitrary<CausalPatternSeed>(
                FsCheck.Fluent.Gen.Select(
                    FsCheck.Fluent.Gen.Choose(0, int.MaxValue),
                    value => new CausalPatternSeed(value)
                )
            );
    }

    private sealed class DelegateArbitrary<T>(Gen<T> generator) : Arbitrary<T>
    {
        public override Gen<T> Generator => generator;

        public override IEnumerable<T> Shrinker(T value) => [];
    }
}
