extern alias JifDetails;
using FluentAssertions;

using FsCheck;
using Microsoft.FSharp.Core;
using Siteswaps.Generator.Components;
using Siteswaps.Generator.Components.SiteswapLab;
using Siteswaps.Generator.Core.Generator;
using CausalStartPropsCalculator = JifDetails::Siteswap.Details.CausalDiagram.CausalStartPropsCalculator;
using DetailSiteswap = JifDetails::Siteswap.Details.Siteswap;
using PassingDiagramBuilder = JifDetails::Siteswap.Details.CausalDiagram.PassingDiagramBuilder;
using SiteswapDiagramBuilder = JifDetails::Siteswap.Details.CausalDiagram.SiteswapDiagramBuilder;
using StartProps = JifDetails::Siteswap.Details.CausalDiagram.StartProps;

namespace Siteswaps.Generator.Test.Feeding;

[TestFixture]
public sealed class CausalStartPropsPropertyTests
{
    private const int FeedingSampleCount = 2_000;
    private static int CappedSiteswapInputCount;
    private static readonly string[] GeneratedSiteswaps = GenerateSiteswaps();

    [Test]
    public void Siteswap_generator_supplies_valid_siteswap_inputs()
    {
        GeneratedSiteswaps.Should().NotBeEmpty();
        foreach (var notation in GeneratedSiteswaps)
            DetailSiteswap.TryCreate(notation, out _).Should().BeTrue(notation);
        TestContext.Progress.WriteLine(
            $"Generated {GeneratedSiteswaps.Length} unique siteswaps; {CappedSiteswapInputCount} generator inputs reached the 2000-pattern cap."
        );
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
            var diagramPatternPeriod =
                pattern.Period % 2 == 0 ? pattern.Period : pattern.Period * 2;
            var activeSlots = pattern.State.People.Sum(person =>
                person.Cells.Count(cell => cell.Height > 0)
            );
            pattern
                .Diagram.Throws.Should()
                .HaveCount(activeSlots * (diagramPatternPeriod / pattern.Period));
            validCount++;
        }

        validCount.Should().Be(100);
    }

    [Test]
    public void Passing_diagram_maps_every_landing_to_the_requested_person_and_local_hand()
    {
        for (var seed = 0; seed < FeedingSampleCount; seed++)
        {
            var pattern = FeedingPattern.Create(seed);
            var zones = pattern
                .State.People.Select(person => person.TimeZone)
                .Distinct()
                .Order()
                .ToArray();
            var timeStretch = zones.Length;
            var diagramPeriod = pattern.Period % 2 == 0 ? pattern.Period : pattern.Period * 2;

            for (var person = 0; person < pattern.PersonCount; person++)
            {
                for (var beat = 0; beat < diagramPeriod; beat++)
                {
                    if (pattern.State.People[person].Cells[beat % pattern.Period].Height == 0)
                        continue;

                    var zone = Array.IndexOf(zones, pattern.State.People[person].TimeZone);
                    var time = beat * timeStretch + zone;
                    var node = pattern.Diagram.Throws.Single(item =>
                        item.Hand.Person.Name == ((char)('A' + person)).ToString()
                        && item.Time == time
                    );
                    var cell = pattern.State.People[person].Cells[beat % pattern.Period];
                    var targetPerson = cell.TargetPerson
                        ?? throw new InvalidOperationException("Generated feed target is missing.");
                    var targetZone = Array.IndexOf(
                        zones,
                        pattern.State.People[targetPerson].TimeZone
                    );
                    var targetLocalBeat =
                        beat
                        + (zone + (pattern.Diagram.HeightOf(node) ?? 0) - targetZone) / timeStretch;
                    var expectedHand = targetLocalBeat % 2 == 0 ? "R" : "L";
                    var actualTargetHand = pattern.Diagram.Hands[node.ToLimb!.Value];

                    actualTargetHand
                        .Person.Name.Should()
                        .Be(
                            ((char)('A' + targetPerson)).ToString(),
                            $"seed {seed}, {person}/{beat} should land on its selected person"
                        );
                    actualTargetHand
                        .Name.Should()
                        .Be(
                            expectedHand,
                            $"seed {seed}, {person}/{beat} should land on the hand active at target local beat {targetLocalBeat}"
                        );
                }
            }
        }
    }

    [Test]
    public void Siteswap_distribution_algorithms_are_equal_for_generator_outputs()
    {
        CheckProperty(SiteSwapCase.Arbitrary, testCase =>
        {
            var notation = testCase.Notation;
            DetailSiteswap.TryCreate(notation, out var siteswap);
            var diagram = SiteswapDiagramBuilder.Build(siteswap!, numberOfJugglers: 1);
            var causal = CausalStartPropsCalculator.CalculateFromPeriodicCausalDiagram(diagram);
            return SameDistribution(diagram.StartProps, causal);
        });
    }

    [
        Test,
        Explicit(
            "Exploratory property: known counterexamples are expected while the algorithms differ."
        )
    ]
    public void Feeding_distribution_algorithms_are_equal_for_random_valid_patterns()
    {
        CheckProperty(FeedingPattern.Arbitrary, pattern =>
        {
            var causal = CausalStartPropsCalculator.CalculateFromPeriodicCausalDiagram(
                pattern.Diagram
            );
            var inventory = pattern
                .State.People.Select(
                    (person, index) => (person.Name, Props: pattern.State.StartingClubsFor(index))
                )
                .ToDictionary(
                    item => item.Name,
                    item => new StartProps(item.Props.Left, item.Props.Right),
                    StringComparer.Ordinal
                );
            return SameDistribution(inventory, causal);
        });
    }

    [Test]
    public void FsCheck_feeding_generator_produces_valid_patterns_with_feasible_causal_starts()
    {
        CheckProperty(FeedingPattern.Arbitrary, pattern =>
        {
            if (!pattern.State.IsValid)
                return false;

            var analysis = CausalStartPropsCalculator.AnalyzePeriodicCausalDiagram(pattern.Diagram);
            return CanRun(
                pattern.Diagram,
                analysis.StartProps.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.Ordinal
                ),
                analysis.FullPeriodMultiplier
            );
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
        var allFeedingDifferences = Enumerable
            .Range(0, FeedingSampleCount)
            .Select(seed => (Seed: seed, Pattern: FeedingPattern.Create(seed)))
            .Select(item => (item.Seed, item.Pattern, Difference: CompareFeeding(item.Pattern)))
            .Where(item => item.Difference is not null)
            .ToArray();

        TestContext.Progress.WriteLine(
            $"Siteswap mismatches: {allSiteswapDifferences.Length}/{GeneratedSiteswaps.Length}; first 20:"
        );
        TestContext.Progress.WriteLine(
            $"Siteswap matches: {matchingSiteswaps.Length}/{GeneratedSiteswaps.Length}"
        );
        foreach (var item in allSiteswapDifferences.Take(20))
            TestContext.Progress.WriteLine($"  {item.Notation}: {item.Difference}");

        TestContext.Progress.WriteLine(
            $"Feeding mismatches: {allFeedingDifferences.Length}/{FeedingSampleCount} valid generated patterns; first 20:"
        );
        foreach (var item in allFeedingDifferences.Take(20))
            TestContext.Progress.WriteLine(
                $"  seed {item.Seed}: {item.Pattern}; {item.Difference}"
            );

        allSiteswapDifferences.Should().BeEmpty();
        allFeedingDifferences.Should().NotBeEmpty();
    }

    [Test]
    public void Causal_distribution_is_feasible_when_simulated_from_all_props_in_hands()
    {
        for (var seed = 0; seed < FeedingSampleCount; seed++)
            AssertFeedingSimulation(seed);
    }

    [Test]
    public void Causal_distribution_repairs_invalid_legacy_feeding_starts()
    {
        var failures = new List<(int Seed, string Pattern, string Legacy, string Causal)>();
        var physicallyValidDifferences =
            new List<(int Seed, string Pattern, string Legacy, string Causal)>();
        var naturalDistributionFailures =
            new List<(int Seed, string Pattern, string Natural, string Causal)>();
        var naturalDistributionDifferences =
            new List<(int Seed, string Pattern, string Natural, string Causal)>();
        var legacyCountErrors = 0;
        var legacySimulationFailures = 0;
        var legacyRuns = 0;
        var naturalRuns = 0;
        for (var seed = 0; seed < FeedingSampleCount; seed++)
        {
            var pattern = FeedingPattern.Create(seed);
            var analysis = CausalStartPropsCalculator.AnalyzePeriodicCausalDiagram(pattern.Diagram);
            var legacy = pattern
                .State.People.Select(
                    (person, index) => (person.Name, Props: pattern.State.StartingClubsFor(index))
                )
                .ToDictionary(
                    item => item.Name,
                    item => new StartProps(item.Props.Left, item.Props.Right),
                    StringComparer.Ordinal
                );
            var expectedProps =
                pattern.Diagram.Throws.Sum(node => pattern.Diagram.HeightOf(node) ?? 0)
                / pattern.Diagram.StepCount;
            var natural = CausalStartPropsCalculator.CalculateWithNaturalRemainder(
                pattern.Diagram.Throws,
                pattern.Diagram.CausalArrows,
                pattern.Diagram.Jugglers,
                expectedProps
            );
            if (
                !CanRun(
                    pattern.Diagram,
                    natural.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.Ordinal
                    ),
                    analysis.FullPeriodMultiplier
                )
            )
                naturalDistributionFailures.Add(
                    (seed, pattern.ToString(), Format(natural), Format(analysis.StartProps))
                );
            else
            {
                naturalRuns++;
                if (!SameDistribution(natural, analysis.StartProps))
                    naturalDistributionDifferences.Add(
                        (seed, pattern.ToString(), Format(natural), Format(analysis.StartProps))
                    );
            }

            var actualProps = legacy.Values.Sum(count => count.Left + count.Right);
            var hasCorrectPropCount = actualProps == expectedProps;
            var canRun =
                hasCorrectPropCount
                && CanRun(pattern.Diagram, legacy, analysis.FullPeriodMultiplier);
            if (!hasCorrectPropCount)
                legacyCountErrors++;
            else if (!canRun)
                legacySimulationFailures++;
            else
            {
                legacyRuns++;
                if (!SameDistribution(legacy, analysis.StartProps))
                    physicallyValidDifferences.Add(
                        (seed, pattern.ToString(), Format(legacy), Format(analysis.StartProps))
                    );
            }

            if (!canRun)
                failures.Add(
                    (seed, pattern.ToString(), Format(legacy), Format(analysis.StartProps))
                );
        }

        TestContext.Progress.WriteLine(
            $"Legacy starts: {legacyCountErrors} wrong prop counts; {legacySimulationFailures} fail the physical simulation; {legacyRuns} pass (total {FeedingSampleCount} valid feeds)."
        );
        foreach (var failure in failures.Take(20))
            TestContext.Progress.WriteLine(
                $"  seed {failure.Seed}: {failure.Pattern}; legacy={failure.Legacy}; causal={failure.Causal}"
            );
        TestContext.Progress.WriteLine(
            $"Legacy distributions that differ but pass prop-count, no-shortage, and periodic-state checks: {physicallyValidDifferences.Count}"
        );
        foreach (var caseData in physicallyValidDifferences.Take(20))
            TestContext.Progress.WriteLine(
                $"  seed {caseData.Seed}: {caseData.Pattern}; legacy={caseData.Legacy}; causal={caseData.Causal}"
            );
        TestContext.Progress.WriteLine(
            $"Natural alphabetical distributions: {naturalRuns}/{FeedingSampleCount} pass physical simulation; {naturalDistributionFailures.Count} fail; {naturalDistributionDifferences.Count} valid cases differ from causal path distribution."
        );
        foreach (var caseData in naturalDistributionFailures.Take(20))
            TestContext.Progress.WriteLine(
                $"  seed {caseData.Seed}: {caseData.Pattern}; natural={caseData.Natural}; causal={caseData.Causal}"
            );
        foreach (var caseData in naturalDistributionDifferences.Take(20))
            TestContext.Progress.WriteLine(
                $"  valid seed {caseData.Seed}: {caseData.Pattern}; natural={caseData.Natural}; causal={caseData.Causal}"
            );
        failures.Should().NotBeEmpty();
    }

    [Test]
    public void Causal_path_phase_is_needed_when_a_feed_has_a_closed_prop_cycle()
    {
        /// Seed 1 is a 3-person, period-2 valid feed whose positive throws form one
        /// closed causal prop path: A/R -> B/R -> C/L -> A/R.
        var pattern = FeedingPattern.Create(1);
        pattern.State.IsValid.Should().BeTrue();
        var analysis = CausalStartPropsCalculator.AnalyzePeriodicCausalDiagram(pattern.Diagram);
        analysis
            .StartProps.Should()
            .BeEquivalentTo(
                new Dictionary<string, StartProps>
                {
                    ["A"] = new(Left: 0, Right: 1),
                    ["B"] = new(Left: 0, Right: 0),
                    ["C"] = new(Left: 0, Right: 0),
                }
            );
        pattern.Diagram.StartProps.Should().BeEquivalentTo(analysis.StartProps);
        SiteswapLabJifMapper
            .CalculateCausalStartProps(
                pattern.State.People.Select(person => person.TimeZone).ToArray(),
                pattern
                    .State.People.Select(person =>
                        (IReadOnlyList<int>)person.Cells.Select(cell => cell.Height).ToArray()
                    )
                    .ToArray(),
                pattern
                    .State.People.Select(
                        (person, index) =>
                            (IReadOnlyList<int>)
                                person.Cells.Select(cell => cell.TargetPerson ?? index).ToArray()
                    )
                    .ToArray()
            )
            .Should()
            .BeEquivalentTo(analysis.StartProps);

        var totalProps =
            pattern.Diagram.Throws.Sum(node => pattern.Diagram.HeightOf(node) ?? 0)
            / pattern.Diagram.StepCount;
        var alphabeticalRemainder = CausalStartPropsCalculator.CalculateWithNaturalRemainder(
            pattern.Diagram.Throws,
            pattern.Diagram.CausalArrows,
            pattern.Diagram.Jugglers,
            totalProps
        );
        alphabeticalRemainder
            .Should()
            .BeEquivalentTo(
                new Dictionary<string, StartProps>
                {
                    ["A"] = new(Left: 0, Right: 0),
                    ["B"] = new(Left: 0, Right: 1),
                    ["C"] = new(Left: 1, Right: 0),
                }
            );
        CanRun(
                pattern.Diagram,
                alphabeticalRemainder.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.Ordinal
                ),
                analysis.FullPeriodMultiplier
            )
            .Should()
            .BeFalse("the first scheduled throw is A/R at time zero");
        CanRun(
                pattern.Diagram,
                analysis.StartProps.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.Ordinal
                ),
                analysis.FullPeriodMultiplier
            )
            .Should()
            .BeTrue("the causal cycle's phase determines which hand holds the club at start");
    }

    [Test]
    public void Siteswap_lab_start_props_are_unavailable_for_incompatible_landing_phases()
    {
        var timeZones = new[] { 0, 1 };
        IReadOnlyList<int>[] heights = [[3], [3]];
        IReadOnlyList<int>[] targets = [[0], [1]];

        SiteswapLabJifMapper
            .CalculateCausalStartProps(timeZones, heights, targets)
            .Should()
            .BeEmpty();
    }

    private static void AssertFeedingSimulation(int seed)
    {
        var pattern = FeedingPattern.Create(seed);
        var analysis = CausalStartPropsCalculator.AnalyzePeriodicCausalDiagram(pattern.Diagram);
        var distribution = analysis.StartProps;
        var heightSum = pattern.Diagram.Throws.Sum(node => pattern.Diagram.HeightOf(node) ?? 0);
        (heightSum % pattern.Diagram.StepCount)
            .Should()
            .Be(0, $"seed {seed} should have an integral prop count");
        var expectedProps = heightSum / pattern.Diagram.StepCount;
        distribution.Values.Sum(count => count.Left + count.Right).Should().Be(expectedProps);

        var inventory = new Dictionary<(string Juggler, string Hand), int>();
        foreach (var hand in pattern.Diagram.Hands)
        {
            var props = distribution[hand.Person.Name];
            inventory[(hand.Person.Name, hand.Name)] = hand.Name == "L" ? props.Left : props.Right;
        }
        var throwsByTime = pattern
            .Diagram.Throws.Where(node => pattern.Diagram.HeightOf(node) is > 0)
            .GroupBy(node => node.Time)
            .ToDictionary(
                group => group.Key,
                group =>
                    group.OrderBy(node => node.Hand.Person.Name, StringComparer.Ordinal).ToArray()
            );
        var arrivals = new Dictionary<(int Time, string Juggler, string Hand), int>();
        var period = pattern.Diagram.StepCount;
        var maxHeight = pattern.Diagram.Throws.Max(node => pattern.Diagram.HeightOf(node) ?? 0);
        var fullPeriod = period * analysis.FullPeriodMultiplier;
        var seedTime = decimal.ToInt32(analysis.SeedingCompletionTime);
        var horizon = seedTime + fullPeriod + maxHeight;
        SimulationSnapshot? stateAfterSeeding = null;
        for (var time = 0; time < horizon; time++)
        {
            foreach (var arrival in arrivals.Where(pair => pair.Key.Time == time).ToArray())
            {
                var key = (arrival.Key.Juggler, arrival.Key.Hand);
                inventory[key] = inventory.GetValueOrDefault(key) + arrival.Value;
                arrivals.Remove(arrival.Key);
            }

            var phase = (decimal)(time % period);
            if (throwsByTime.TryGetValue(phase, out var events))
            {
                foreach (var throwNode in events)
                {
                    var source = (throwNode.Hand.Person.Name, throwNode.Hand.Name);
                    inventory
                        .GetValueOrDefault(source)
                        .Should()
                        .BeGreaterThan(
                            0,
                            $"seed {seed}, at time {time}, before throw {throwNode.Hand.Person.Name}/{throwNode.Hand.Name}"
                        );
                    inventory[source]--;

                    var targetHand = pattern.Diagram.Hands[throwNode.ToLimb!.Value];
                    var landing = (
                        time + (pattern.Diagram.HeightOf(throwNode) ?? 0),
                        targetHand.Person.Name,
                        targetHand.Name
                    );
                    arrivals[landing] = arrivals.GetValueOrDefault(landing) + 1;
                }
            }

            if (time == seedTime)
                stateAfterSeeding = Snapshot(time, inventory, arrivals);
            if (time == seedTime + fullPeriod)
                Snapshot(time, inventory, arrivals)
                    .Should()
                    .BeEquivalentTo(
                        stateAfterSeeding,
                        $"seed {seed}, {pattern}, start={Format(distribution)}, multiplier={analysis.FullPeriodMultiplier}, state should repeat after every prop orbit has been seeded"
                    );
        }
    }

    private static bool CanRun(
        JifDetails::Siteswap.Details.CausalDiagram.DiagramSet diagram,
        Dictionary<string, StartProps> startProps,
        int periodMultiplier
    )
    {
        var totalHeight = diagram.Throws.Sum(node => diagram.HeightOf(node) ?? 0);
        if (
            totalHeight % diagram.StepCount != 0
            || startProps.Values.Sum(count => count.Left + count.Right)
                != totalHeight / diagram.StepCount
        )
            return false;

        var inventory = new Dictionary<(string Juggler, string Hand), int>();
        foreach (var hand in diagram.Hands)
        {
            var props = startProps[hand.Person.Name];
            inventory[(hand.Person.Name, hand.Name)] = hand.Name == "L" ? props.Left : props.Right;
        }

        var throwsByTime = diagram
            .Throws.Where(node => diagram.HeightOf(node) is > 0)
            .GroupBy(node => node.Time)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var arrivals = new Dictionary<(int Time, string Juggler, string Hand), int>();
        var period = diagram.StepCount;
        var horizon =
            period * (periodMultiplier + 2)
            + diagram.Throws.Max(node => diagram.HeightOf(node) ?? 0);
        var warmup = period + diagram.Throws.Max(node => diagram.HeightOf(node) ?? 0);
        var fullPeriod = period * periodMultiplier;
        SimulationSnapshot? firstPeriodicState = null;
        SimulationSnapshot? repeatedPeriodicState = null;
        for (var time = 0; time < horizon; time++)
        {
            foreach (var arrival in arrivals.Where(pair => pair.Key.Time == time).ToArray())
            {
                var hand = (arrival.Key.Juggler, arrival.Key.Hand);
                inventory[hand] = inventory.GetValueOrDefault(hand) + arrival.Value;
                arrivals.Remove(arrival.Key);
            }

            if (throwsByTime.TryGetValue(time % period, out var events))
            {
                foreach (var throwNode in events)
                {
                    var source = (throwNode.Hand.Person.Name, throwNode.Hand.Name);
                    if (inventory.GetValueOrDefault(source) <= 0)
                        return false;
                    inventory[source]--;

                    var targetHand = diagram.Hands[throwNode.ToLimb!.Value];
                    var arrival = (
                        time + (diagram.HeightOf(throwNode) ?? 0),
                        targetHand.Person.Name,
                        targetHand.Name
                    );
                    arrivals[arrival] = arrivals.GetValueOrDefault(arrival) + 1;
                }
            }

            if (time == warmup)
                firstPeriodicState = Snapshot(time, inventory, arrivals);
            if (time == warmup + fullPeriod)
                repeatedPeriodicState = Snapshot(time, inventory, arrivals);
        }

        return firstPeriodicState is not null
            && repeatedPeriodicState is not null
            && SameEntries(firstPeriodicState.Inventory, repeatedPeriodicState.Inventory)
            && SameEntries(firstPeriodicState.InFlight, repeatedPeriodicState.InFlight);
    }

    private static bool SameEntries<TKey>(Dictionary<TKey, int> left, Dictionary<TKey, int> right)
        where TKey : notnull =>
        left.Count == right.Count
        && left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);

    private static string[] GenerateSiteswaps()
    {
        var siteswaps = new HashSet<string>(StringComparer.Ordinal);
        for (var period = 1; period <= 6; period++)
        {
            for (var objects = 0; objects <= Math.Min(8, period * 10); objects++)
            {
                var input = new SiteswapGeneratorInput(period, objects, 0, 10)
                {
                    StopCriteria = new StopCriteria(TimeSpan.FromSeconds(1), 2000),
                };
                var generatedCount = 0;
                foreach (var siteswap in new SiteswapGenerator(input).Generate())
                {
                    siteswaps.Add(siteswap.ToString());
                    generatedCount++;
                }
                if (generatedCount == 2000)
                    CappedSiteswapInputCount++;
            }
        }

        return siteswaps.Order(StringComparer.Ordinal).ToArray();
    }

    private static bool SameDistribution(
        IReadOnlyDictionary<string, StartProps> expected,
        System.Collections.Immutable.ImmutableDictionary<string, StartProps> actual
    ) =>
        expected.Count == actual.Count
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
        var causal = CausalStartPropsCalculator.CalculateFromPeriodicCausalDiagram(diagram);
        return SameDistribution(diagram.StartProps, causal)
            ? null
            : $"legacy={Format(diagram.StartProps)}, causal={Format(causal)}";
    }

    private static string? CompareFeeding(FeedingPattern pattern)
    {
        var causal = CausalStartPropsCalculator.CalculateFromPeriodicCausalDiagram(pattern.Diagram);
        var inventory = pattern
            .State.People.Select(
                (person, index) => (person.Name, Props: pattern.State.StartingClubsFor(index))
            )
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
        string.Join(
            ", ",
            distribution
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}:{pair.Value.Left}/{pair.Value.Right}")
        );

    private static SimulationSnapshot Snapshot(
        int time,
        IReadOnlyDictionary<(string Juggler, string Hand), int> inventory,
        IReadOnlyDictionary<(int Time, string Juggler, string Hand), int> arrivals
    ) =>
        new(
            inventory.ToDictionary(pair => pair.Key, pair => pair.Value),
            arrivals
                .Where(pair => pair.Key.Time > time)
                .ToDictionary(
                    pair => (pair.Key.Time - time, pair.Key.Juggler, pair.Key.Hand),
                    pair => pair.Value
                )
        );

    private sealed record SimulationSnapshot(
        Dictionary<(string Juggler, string Hand), int> Inventory,
        Dictionary<(int TimeUntilLanding, string Juggler, string Hand), int> InFlight
    );

    private static int PositiveModulo(int value, int modulo) => (value % modulo + modulo) % modulo;

    private sealed record SiteSwapCase(string Notation)
    {
        public override string ToString() => Notation;

        public static Arbitrary<SiteSwapCase> Arbitrary { get; } =
            new DelegateArbitrary<SiteSwapCase>(
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
        public static Arbitrary<FeedingPattern> Arbitrary { get; } =
            new DelegateArbitrary<FeedingPattern>(
                FsCheck.Fluent.Gen.Select(FsCheck.Fluent.Gen.Choose(0, int.MaxValue), seed =>
                    Create(seed)
                )
            );

        public override string ToString() =>
            $"people={PersonCount}, period={Period}, zones=[{string.Join(',', State.People.Select(person => person.TimeZone))}], "
            + $"heights=[{string.Join(';', State.People.Select(person => string.Join(',', person.Cells.Select(cell => cell.Height))))}], "
            + $"targets=[{string.Join(';', State.People.Select(person => string.Join(',', person.Cells.Select(cell => cell.TargetPerson))))}]";

        public static FeedingPattern Create(int seed)
        {
            var random = new Random(seed);
            var personCount = random.Next(3, 6);
            var period = random.Next(2, 6);
            var zoneCount = random.Next(2, Math.Min(4, personCount) + 1);

            var zones = Enumerable
                .Range(0, personCount)
                .Select(person => person % zoneCount)
                .ToArray();
            Shuffle(zones, random);

            var targets = Enumerable.Range(0, personCount).Select(_ => new int[period]).ToArray();
            var heights = Enumerable.Range(0, personCount).Select(_ => new int[period]).ToArray();
            var allSlots = Enumerable.Range(0, personCount * period).ToArray();
            var activeSlots = Enumerable
                .Range(0, personCount)
                .Select(person => person * period + random.Next(period))
                .ToHashSet();
            var remainingSlots = allSlots.Where(slot => !activeSlots.Contains(slot)).ToArray();
            Shuffle(remainingSlots, random);
            var extraActiveCount = random.Next(remainingSlots.Length + 1);
            foreach (var slot in remainingSlots.Take(extraActiveCount))
                activeSlots.Add(slot);

            var sources = activeSlots.Order().ToArray();
            var destinations = sources.ToArray();
            Shuffle(destinations, random);

            for (var index = 0; index < sources.Length; index++)
            {
                var source = sources[index];
                var sourcePerson = source / period;
                var sourceBeat = source % period;
                var targetPerson = destinations[index] / period;
                var targetBeat = destinations[index] % period;
                targets[sourcePerson][sourceBeat] = targetPerson;

                // Choose the smallest non-negative height that lands on the permuted slot.
                var beatDelta = PositiveModulo(targetBeat - sourceBeat, period);
                var height = beatDelta * zoneCount + zones[targetPerson] - zones[sourcePerson];
                while (height < 1)
                    height += zoneCount * period;
                heights[sourcePerson][sourceBeat] = height;
            }

            var state = new PassingEditorState(
                new string('3', period),
                maxThrowHeight: SettingsDto.MaxMaxHeight
            );
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
                    if (heights[person][beat] == 0)
                    {
                        state
                            .SetTarget(person, beat, person)
                            .Should()
                            .BeTrue($"empty slot {person}/{beat} should target itself");
                        continue;
                    }
                    state
                        .SetTarget(person, beat, targets[person][beat])
                        .Should()
                        .BeTrue($"generated landing for {person}/{beat} must be zone-compatible");
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
