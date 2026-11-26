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
    public void UserProvidedIrregularAsynchronousDiagram_MapsToPassist()
    {
        var state = CreateUserProvidedIrregularPattern();
        CausalDiagramEditorState.TryDecode(state.Encode(), out var decoded).Should().BeTrue();

        CausalDiagramEditorJifAdapter
            .TryMap(decoded!, out var mapped, out var error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
        mapped!.Jugglers.Should().HaveCount(2);
        mapped.Throws.Should().NotBeEmpty();
        mapped.Repetition.Should().NotBeNull();
    }

    [Test]
    public void ValidFactoryGeneratedCausalPatterns_AlwaysMapToPeriodicJif()
    {
        CheckProperty(CausalPatternSeed.Arbitrary, sample =>
        {
            var state = CreateFactoryGeneratedPattern(sample.Value);
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
                || table.Rows.Sum(row => row.Cells.Count(chip => chip is not null))
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
            .Count(cell => cell is not null)
            .Should()
            .Be(state.Throws.Count * state.Repetitions);
        table.Rows[0].Cells.Any(cell => cell is null).Should().BeTrue();
        table.Rows[1].Cells.Any(cell => cell is null).Should().BeTrue();
    }

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
    public void TemporaryMultipleIncomingArrowsKeepTheDiagramEditableButPauseAnimation()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();
        var edgeIndex = state.CausalEdges.FindIndex(edge => edge.FromThrowId == "P1-2");
        var duplicate = state.CausalEdges.Single(edge => edge.FromThrowId == "P1-1");
        state.CausalEdges[edgeIndex] = state.CausalEdges[edgeIndex] with
        {
            ToThrowId = duplicate.ToThrowId,
            CycleOffset = duplicate.CycleOffset,
        };
        state.SynchronizeHeights();

        CausalDiagramEditorState.TryDecode(state.Encode(), out _).Should().BeTrue();
        CausalDiagramEditorJifAdapter.TryMap(state, out _, out var error).Should().BeFalse();
        error.Should().Contain("mehrere eingehende Pfeile");
    }

    [Test]
    public void JugglerWithoutThrowsPausesAnimationWithoutThrowing()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();
        state.Jugglers.Add(new EditorJuggler("J3", "Jongleur 3"));

        CausalDiagramEditorJifAdapter.TryMap(state, out _, out var error).Should().BeFalse();
        error.Should().Contain("mindestens einen Wurf");
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
        diagram
            .Throws.Where(item => item.Hand.Person.Name == "A")
            .Select(item => item.Height)
            .Should()
            .OnlyContain(height => height == 5);
        diagram
            .Throws.Where(item => item.Hand.Person.Name == "B")
            .Select(item => item.Height)
            .Should()
            .OnlyContain(height => height == 7);
        diagram.Throws.Should().OnlyContain(item => item.ToLimb.HasValue);
        CausalDiagramEditorJifAdapter
            .TryMap(state, out var mapped, out error)
            .Should()
            .BeTrue(error);
        mapped.Should().NotBeNull();
        mapped!.Jugglers.Should().HaveCount(2);
        mapped.Jugglers!.Select(juggler => juggler.Position![0]).Should().Equal(1.6m, -1.6m);
        mapped.Jugglers.Select(juggler => juggler.Position![2]).Should().OnlyContain(z => z == 0m);
        mapped.Props.Should().HaveCount(2);
        mapped.Props!.Should().OnlyContain(prop => prop.Type == "club");
        mapped.Limbs.Should().HaveCount(4);
        mapped.Repetition.Should().NotBeNull();
        mapped.Repetition!.Period.Should().Be(14);
        mapped.TimeStretchFactor.Should().Be(1m);
        mapped
            .Throws!.Select(item => item.Duration)
            .Should()
            .OnlyContain(duration => duration == 1m || duration == 1.4m);
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

    private static CausalDiagramEditorState CreateUserProvidedIrregularPattern()
    {
        var state = CausalDiagramEditorState.CreateFeeding75();
        state.PresetId = "custom";
        state.CycleLength = 35m;
        state.Throws =
        [
            new("P1-1", "P1", "R", 0m, 0m),
            new("P1-2", "P1", "L", 6m, 0m),
            new("P1-3", "P1", "R", 10m, 0m),
            new("P1-4", "P1", "L", 15m, 0m),
            new("P1-5", "P1", "R", 20m, 0m),
            new("P1-6", "P1", "L", 25m, 0m),
            new("P1-7", "P1", "R", 30m, 0m),
            new("P2-1", "P2", "R", 0m, 0m),
            new("P2-2", "P2", "L", 7m, 0m),
            new("P2-3", "P2", "R", 14m, 0m),
            new("P2-4", "P2", "L", 21m, 0m),
            new("P2-5", "P2", "R", 28m, 0m),
        ];
        state.CausalEdges =
        [
            new("feed-P1-1", "P1-1", "P1-2", 0),
            new("feed-P1-2", "P1-2", "P1-3", 0),
            new("feed-P1-3", "P1-3", "P2-1", 0),
            new("feed-P1-4", "P1-4", "P1-5", 0),
            new("feed-P1-5", "P1-5", "P1-4", 0),
            new("feed-P1-6", "P1-6", "P1-7", 0),
            new("feed-P1-7", "P1-7", "P1-6", 0),
            new("feed-P2-1", "P2-1", "P1-1", 0),
            new("feed-P2-2", "P2-2", "P2-3", 0),
            new("feed-P2-3", "P2-3", "P2-2", 0),
            new("feed-P2-4", "P2-4", "P2-5", 0),
            new("feed-P2-5", "P2-5", "P2-4", 0),
        ];
        state.SynchronizeHeights();
        return state;
    }

    private static CausalDiagramEditorState CreateFactoryGeneratedPattern(int seed)
    {
        var random = new Random(seed);
        CausalDiagramEditorState state;
        if (random.Next(3) == 0)
        {
            var validSiteswaps = new[] { "3", "4", "5", "531", "441", "423", "97531", "756" };
            state = CausalDiagramEditorState.CreateSiteswapPattern(
                validSiteswaps[random.Next(validSiteswaps.Length)]
            );
        }
        else if (random.Next(2) == 0)
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
            {
                for (var beat = 0; beat < beatsPerJuggler; beat++)
                {
                    var id = $"{juggler.Id}-{beat + 1}";
                    state.Throws.Add(
                        new EditorThrow(
                            id,
                            juggler.Id,
                            beat % 2 == 0 ? "R" : "L",
                            beat,
                            beatsPerJuggler
                        )
                    );
                    state.CausalEdges.Add(new EditorCausalEdge($"self-{id}", id, id, 1));
                }
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
