extern alias JifDetails;

using FsCheck;
using FluentAssertions;
using Microsoft.FSharp.Core;
using Siteswaps.Generator.Components;
using Siteswaps.Generator.Components.SiteswapLab;
using Siteswaps.Generator.Core.Generator;
using CausalStartPropsCalculator = JifDetails::Siteswap.Details.CausalDiagram.CausalStartPropsCalculator;
using PassingDiagramBuilder = JifDetails::Siteswap.Details.CausalDiagram.PassingDiagramBuilder;
using SiteswapDiagramBuilder = JifDetails::Siteswap.Details.CausalDiagram.SiteswapDiagramBuilder;
using StartProps = JifDetails::Siteswap.Details.CausalDiagram.StartProps;
using DetailSiteswap = JifDetails::Siteswap.Details.Siteswap;

namespace Siteswaps.Generator.Test.Feeding;

[TestFixture]
public sealed class CausalStartPropsPropertyTests
{
    private static readonly string[] GeneratedSiteswaps = GenerateSiteswaps();

    [Test]
    public void Siteswap_generator_supplies_valid_siteswap_inputs()
    {
        GeneratedSiteswaps.Should().NotBeEmpty();
        foreach (var notation in GeneratedSiteswaps)
            DetailSiteswap.TryCreate(notation, out _).Should().BeTrue(notation);
    }

    [Test]
    public void Random_feeding_generator_produces_valid_patterns_with_more_than_two_people()
    {
        var validCount = 0;
        for (var seed = 0; seed < 100; seed++)
        {
            var pattern = FeedingPattern.Create(seed);
            pattern.PersonCount.Should().BeInRange(3, 5);
            pattern.State.IsValid.Should().BeTrue(pattern.ToString());
            pattern.Diagram.Throws.Should().HaveCount(pattern.PersonCount * pattern.Period);
            validCount++;
        }

        validCount.Should().Be(100);
    }

    [Test, Explicit("Exploratory property: known counterexamples are expected while the algorithms differ.")]
    public void Siteswap_distribution_algorithms_are_equal_for_generator_outputs()
    {
        CheckProperty(SiteSwapCase.Arbitrary, testCase =>
        {
            var notation = testCase.Notation;
            DetailSiteswap.TryCreate(notation, out var siteswap);
            var diagram = SiteswapDiagramBuilder.Build(siteswap!, numberOfJugglers: 1);
            var causal = CausalStartPropsCalculator.CalculateWithNaturalRemainder(
                diagram.Throws,
                diagram.CausalArrows,
                diagram.Jugglers,
                (int)siteswap!.NumberOfObjects()
            );
            return SameDistribution(diagram.StartProps, causal);
        });
    }

    [Test, Explicit("Exploratory property: known counterexamples are expected while the algorithms differ.")]
    public void Feeding_distribution_algorithms_are_equal_for_random_valid_patterns()
    {
        CheckProperty(FeedingPattern.Arbitrary, pattern =>
        {
            var causal = CausalStartPropsCalculator.CalculateWithNaturalRemainder(
                pattern.Diagram.Throws,
                pattern.Diagram.CausalArrows,
                pattern.Diagram.Jugglers,
                TotalProps(pattern.Diagram)
            );
            var inventory = pattern.State.People.Select((person, index) =>
                    (person.Name, Props: pattern.State.StartingClubsFor(index)))
                .ToDictionary(
                    item => item.Name,
                    item => new StartProps(item.Props.Left, item.Props.Right),
                    StringComparer.Ordinal
                );
            return SameDistribution(inventory, causal);
        });
    }

    [Test]
    public void Report_differences_found_in_representative_siteswaps_and_feeding_patterns()
    {
        var allSiteswapDifferences = GeneratedSiteswaps
            .Select(notation => (Notation: notation, Difference: CompareSiteswap(notation)))
            .Where(item => item.Difference is not null)
            .ToArray();
        var matchingSiteswaps = GeneratedSiteswaps
            .Where(notation => CompareSiteswap(notation) is null)
            .ToArray();
        var allFeedingDifferences = Enumerable.Range(0, 500)
            .Select(seed => (Seed: seed, Pattern: FeedingPattern.Create(seed)))
            .Select(item => (item.Seed, item.Pattern, Difference: CompareFeeding(item.Pattern)))
            .Where(item => item.Difference is not null)
            .ToArray();

        TestContext.Progress.WriteLine(
            $"Siteswap mismatches: {allSiteswapDifferences.Length}/{GeneratedSiteswaps.Length}; first 20:"
        );
        TestContext.Progress.WriteLine($"Matching siteswaps: {string.Join(", ", matchingSiteswaps)}");
        foreach (var item in allSiteswapDifferences.Take(20))
            TestContext.Progress.WriteLine($"  {item.Notation}: {item.Difference}");

        TestContext.Progress.WriteLine(
            $"Feeding mismatches: {allFeedingDifferences.Length}/500 valid generated patterns; first 20:"
        );
        foreach (var item in allFeedingDifferences.Take(20))
            TestContext.Progress.WriteLine($"  seed {item.Seed}: {item.Pattern}; {item.Difference}");

        allSiteswapDifferences.Should().NotBeEmpty();
        allFeedingDifferences.Should().NotBeEmpty();
    }

    private static string[] GenerateSiteswaps()
    {
        var siteswaps = new HashSet<string>(StringComparer.Ordinal);
        for (var period = 1; period <= 5; period++)
        {
            for (var objects = 0; objects <= Math.Min(7, period * 8); objects++)
            {
                var input = new SiteswapGeneratorInput(period, objects, 0, 8)
                {
                    StopCriteria = new StopCriteria(TimeSpan.FromSeconds(2), 120),
                };
                foreach (var siteswap in new SiteswapGenerator(input).Generate())
                    siteswaps.Add(siteswap.ToString());
            }
        }

        return siteswaps.Order(StringComparer.Ordinal).ToArray();
    }

    private static bool SameDistribution(
        IReadOnlyDictionary<string, StartProps> expected,
        System.Collections.Immutable.ImmutableDictionary<string, StartProps> actual
    ) => expected.Count == actual.Count
        && expected.All(pair => actual.TryGetValue(pair.Key, out var value) && pair.Value == value);

    private static void CheckProperty<T>(Arbitrary<T> arbitrary, Func<T, bool> property)
    {
        var fsCheckProperty = FsCheck.FSharp.Prop.ForAll(
            arbitrary,
            FuncConvert.ToFSharpFunc(new Converter<T, bool>(property))
        );
        Check.One(Config.QuickThrowOnFailure, fsCheckProperty);
    }

    private static string? CompareSiteswap(string notation)
    {
        DetailSiteswap.TryCreate(notation, out var siteswap);
        var diagram = SiteswapDiagramBuilder.Build(siteswap!, numberOfJugglers: 1);
        var causal = CausalStartPropsCalculator.CalculateWithNaturalRemainder(
            diagram.Throws,
            diagram.CausalArrows,
            diagram.Jugglers,
            (int)siteswap!.NumberOfObjects()
        );
        return SameDistribution(diagram.StartProps, causal)
            ? null
            : $"legacy={Format(diagram.StartProps)}, causal={Format(causal)}";
    }

    private static string? CompareFeeding(FeedingPattern pattern)
    {
        var causal = CausalStartPropsCalculator.CalculateWithNaturalRemainder(
            pattern.Diagram.Throws,
            pattern.Diagram.CausalArrows,
            pattern.Diagram.Jugglers,
            TotalProps(pattern.Diagram)
        );
        var inventory = pattern.State.People.Select((person, index) =>
                (person.Name, Props: pattern.State.StartingClubsFor(index)))
            .ToDictionary(
                item => item.Name,
                item => new StartProps(item.Props.Left, item.Props.Right),
                StringComparer.Ordinal
            );
        return SameDistribution(inventory, causal)
            ? null
            : $"legacy={Format(inventory)}, causal={Format(causal)}";
    }

    private static string Format(IReadOnlyDictionary<string, StartProps> distribution) =>
        string.Join(", ", distribution.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value.Left}/{pair.Value.Right}"));

    private static int TotalProps(JifDetails::Siteswap.Details.CausalDiagram.DiagramSet diagram) =>
        diagram.Throws.Sum(throwNode => throwNode.Height) / diagram.StepCount;

    private static int PositiveModulo(int value, int modulo) => (value % modulo + modulo) % modulo;

    private sealed record SiteSwapCase(string Notation)
    {
        public override string ToString() => Notation;

        public static Arbitrary<SiteSwapCase> Arbitrary { get; } = new DelegateArbitrary<SiteSwapCase>(
            FsCheck.Fluent.Gen.Select(
                FsCheck.Fluent.Gen.Elements(GeneratedSiteswaps),
                notation => new SiteSwapCase(notation)
            )
        );
    }

    private sealed record FeedingPattern(
        int Seed,
        int PersonCount,
        int Period,
        PassingEditorState State,
        JifDetails::Siteswap.Details.CausalDiagram.DiagramSet Diagram
    )
    {
        public static Arbitrary<FeedingPattern> Arbitrary { get; } = new DelegateArbitrary<FeedingPattern>(
            FsCheck.Fluent.Gen.Select(FsCheck.Fluent.Gen.Choose(0, int.MaxValue), seed => Create(seed))
        );

        public override string ToString() =>
            $"people={PersonCount}, period={Period}, zones=[{string.Join(',', State.People.Select(person => person.TimeZone))}], " +
            $"heights=[{string.Join(';', State.People.Select(person => string.Join(',', person.Cells.Select(cell => cell.Height))))}], " +
            $"targets=[{string.Join(';', State.People.Select(person => string.Join(',', person.Cells.Select(cell => cell.TargetPerson))))}]";

        public static FeedingPattern Create(int seed)
        {
            var random = new Random(seed);
            var personCount = random.Next(3, 6);
            var period = random.Next(2, 6);
            var zoneCount = random.Next(2, Math.Min(4, personCount) + 1);

            var zones = Enumerable.Range(0, personCount)
                .Select(person => person % zoneCount)
                .ToArray();
            Shuffle(zones, random);

            var targets = Enumerable.Range(0, personCount)
                .Select(_ => new int[period])
                .ToArray();
            var heights = Enumerable.Range(0, personCount)
                .Select(_ => new int[period])
                .ToArray();
            var slots = Enumerable.Range(0, personCount * period).ToArray();
            Shuffle(slots, random);

            for (var source = 0; source < personCount * period; source++)
            {
                var sourcePerson = source / period;
                var sourceBeat = source % period;
                var targetPerson = slots[source] / period;
                var targetBeat = slots[source] % period;
                targets[sourcePerson][sourceBeat] = targetPerson;

                // Choose the smallest non-negative height that lands on the permuted slot.
                var beatDelta = PositiveModulo(targetBeat - sourceBeat, period);
                var height = beatDelta * zoneCount + zones[targetPerson] - zones[sourcePerson];
                while (height < 1)
                    height += zoneCount * period;
                heights[sourcePerson][sourceBeat] = height;
            }

            var state = new PassingEditorState(new string('3', period), maxThrowHeight: SettingsDto.MaxMaxHeight);
            state.SetPersonCount(personCount);
            for (var person = 0; person < personCount; person++)
            {
                while (state.People[person].TimeZone != zones[person])
                    state.CycleTimeZone(person);
            }
            for (var person = 0; person < personCount; person++)
            {
                for (var beat = 0; beat < period; beat++)
                {
                    state.SetHeight(person, beat, heights[person][beat]);
                    state.SetTarget(person, beat, targets[person][beat])
                        .Should().BeTrue($"generated landing for {person}/{beat} must be zone-compatible");
                }
            }

            var diagram = PassingDiagramBuilder.Build(
                zones,
                heights.Select(row => (IReadOnlyList<int>)row).ToArray(),
                targets.Select(row => (IReadOnlyList<int>)row).ToArray()
            );
            return new FeedingPattern(seed, personCount, period, state, diagram);
        }

        private static void Shuffle<T>(T[] values, Random random)
        {
            for (var i = values.Length - 1; i > 0; i--)
            {
                var other = random.Next(i + 1);
                (values[i], values[other]) = (values[other], values[i]);
            }
        }
    }

    private sealed class DelegateArbitrary<T>(Gen<T> generator) : Arbitrary<T>
    {
        public override Gen<T> Generator => generator;

        public override IEnumerable<T> Shrinker(T value) => [];
    }
}
