using System.Collections.Immutable;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Benchmarks;

internal static class GenerationScenarioFactory
{
    public static IReadOnlyList<GenerationScenario> AllScenarios { get; } =
    [
        GenerationScenario.LargeNoFilter,
        GenerationScenario.PatternFilter,
        GenerationScenario.NumberFilter,
        GenerationScenario.StateDontCareFilter,
        GenerationScenario.StateSelectiveFilter,
        GenerationScenario.NumberAtMostFilter,
        GenerationScenario.ExactStateFilter,
        GenerationScenario.NumberOfPassesFilter,
        GenerationScenario.DefaultBallCountFilter,
        GenerationScenario.PersonalizedNumberFilter,
        GenerationScenario.RotationAwarePatternFilter,
        GenerationScenario.LocallyValidFilter,
        GenerationScenario.OrFilter,
        GenerationScenario.NotFilter,
        GenerationScenario.HighDimensionalFilteredStress,
        GenerationScenario.HighDimensionalNoFilterStress,
    ];

    public static SiteswapGenerator Create(GenerationScenario scenario)
    {
        var input = CreateInput(scenario);
        var filter = scenario switch
        {
            GenerationScenario.LargeNoFilter => new NoFilter(),
            GenerationScenario.PatternFilter => new FilterBuilder(input)
                .Pattern([2, -1, 6, -1, 5, -1, -1, -1, -1, -1], 2)
                .Build(),
            GenerationScenario.NumberFilter => new FilterBuilder(input)
                .ExactOccurence([5], 2)
                .Build(),
            GenerationScenario.StateDontCareFilter => new FilterBuilder(input)
                .WithState(
                    new StatePattern([
                        StateValue.DontCare,
                        StateValue.DontCare,
                        StateValue.DontCare,
                        StateValue.DontCare,
                        StateValue.DontCare,
                        StateValue.DontCare,
                        StateValue.DontCare,
                        StateValue.DontCare,
                        StateValue.DontCare,
                        StateValue.DontCare,
                    ])
                )
                .Build(),
            GenerationScenario.StateSelectiveFilter => new FilterBuilder(input)
                .WithState(
                    new StatePattern([
                        StateValue.Occupied,
                        StateValue.Free,
                        StateValue.DontCare,
                        StateValue.Occupied,
                        StateValue.Free,
                        StateValue.DontCare,
                        StateValue.Occupied,
                        StateValue.Free,
                        StateValue.DontCare,
                        StateValue.DontCare,
                    ])
                )
                .Build(),
            GenerationScenario.NumberAtMostFilter => new FilterBuilder(input)
                .MaximumOccurence([5], 2)
                .Build(),
            GenerationScenario.ExactStateFilter => new FilterBuilder(input)
                .WithState(State.GroundState(6))
                .Build(),
            GenerationScenario.NumberOfPassesFilter => new FilterBuilder(input)
                .ExactNumberOfPasses(0, 2)
                .Build(),
            GenerationScenario.DefaultBallCountFilter => new FilterBuilder(input)
                .WithDefault()
                .Build(),
            GenerationScenario.PersonalizedNumberFilter => new PersonalizedNumberFilter(
                2,
                input.MinHeight,
                input.MaxHeight,
                [6],
                1,
                PersonalizedNumberFilter.Type.AtLeast,
                0
            ),
            GenerationScenario.RotationAwarePatternFilter => new RotationAwareFlexiblePatternFilter(
                Enumerable.Repeat(new List<int> { -1 }, 5).ToList(),
                2,
                input,
                0
            ),
            GenerationScenario.LocallyValidFilter => new LocallyValidFilter(2, 0),
            GenerationScenario.OrFilter => new FilterBuilder(input)
                .Or([
                    new FilterBuilder(input).ExactOccurence([6], 6).Build(),
                    new FilterBuilder(input).MaximumOccurence([5], 2).Build(),
                ])
                .Build(),
            GenerationScenario.NotFilter => new FilterBuilder(input)
                .Not(new FilterBuilder(input).MaximumOccurence([5], 0).Build())
                .Build(),
            GenerationScenario.HighDimensionalFilteredStress => CreateHighDimensionalStress(input),
            GenerationScenario.HighDimensionalNoFilterStress => new NoFilter(),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null),
        };

        return new SiteswapGenerator(filter, input);
    }

    private static ISiteswapFilter CreateHighDimensionalStress(SiteswapGeneratorInput input)
    {
        var allowedThrows = Enumerable
            .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(value => value % 2 == 0)
            .ToList();
        var passThrows = new List<int> { -1 };
        var positionalPattern = Enumerable.Repeat(passThrows, input.Period).ToList();
        positionalPattern[0] = allowedThrows;

        var statePattern = new StatePattern(
            Enumerable.Repeat(StateValue.DontCare, input.Period).ToImmutableArray()
        );
        var rotationPattern = Enumerable
            .Repeat(new List<int> { -2, -3 }, input.Period / 2)
            .ToList();
        var stressFilters = new List<ISiteswapFilter>
        {
            new PersonalizedNumberFilter(
                2,
                input.MinHeight,
                input.MaxHeight,
                [input.MaxHeight - 2, input.MaxHeight],
                1,
                PersonalizedNumberFilter.Type.AtLeast,
                0
            ),
            new NotFilter(new FilterBuilder(input).MaximumOccurence([0], 0).Build()),
        };
        stressFilters.AddRange(
            Enumerable
                .Range(0, 2_000)
                .Select(_ =>
                    (ISiteswapFilter)
                        new RotationAwareFlexiblePatternFilter(rotationPattern, 2, input, 0)
                )
        );

        var broadFilters = new FilterBuilder(input)
            .FlexiblePattern(positionalPattern, 2, true)
            .MinimumOccurence([3, 5, 7], 1)
            .MaximumOccurence(
                Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1),
                input.Period
            )
            .ExactOccurence([input.MaxHeight], 1)
            .WithDefault()
            .WithState(statePattern)
            .And(stressFilters)
            .Build();

        return broadFilters;
    }

    public static bool IsTimeBound(GenerationScenario scenario) =>
        scenario
            is GenerationScenario.HighDimensionalFilteredStress
                or GenerationScenario.HighDimensionalNoFilterStress;

    public static int MinimumResultCount(GenerationScenario scenario) =>
        scenario switch
        {
            GenerationScenario.HighDimensionalFilteredStress => 300,
            GenerationScenario.HighDimensionalNoFilterStress => 300_000,
            _ => 1,
        };

    public static void ValidateResultCount(GenerationScenario scenario, int resultCount)
    {
        if (scenario is GenerationScenario.StateSelectiveFilter)
            return;

        var minimumResultCount = MinimumResultCount(scenario);
        if (resultCount < minimumResultCount)
        {
            throw new InvalidOperationException(
                $"Benchmark scenario {scenario} must generate at least {minimumResultCount} Siteswaps; got {resultCount}."
            );
        }

        if (resultCount > 0 || scenario is GenerationScenario.StateSelectiveFilter)
            return;

        throw new InvalidOperationException(
            $"Benchmark scenario {scenario} must generate at least one Siteswap."
        );
    }

    public static void ValidateResultCounts(IReadOnlyDictionary<GenerationScenario, int> counts)
    {
        var missing = AllScenarios.Where(scenario => !counts.ContainsKey(scenario)).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"Missing benchmark results for: {string.Join(", ", missing)}."
            );
        }

        var empty = AllScenarios.Where(scenario => counts[scenario] == 0).ToArray();
        if (empty.Length > 1)
        {
            throw new InvalidOperationException(
                $"At most one benchmark may be empty; empty scenarios: {string.Join(", ", empty)}."
            );
        }

        foreach (var scenario in AllScenarios)
        {
            ValidateResultCount(scenario, counts[scenario]);
        }
    }

    private static SiteswapGeneratorInput CreateInput(GenerationScenario scenario)
    {
        return scenario is GenerationScenario.LargeNoFilter
                ? new SiteswapGeneratorInput(7, 8, 2, 13)
                {
                    StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 100_000),
                }
            : scenario is GenerationScenario.LocallyValidFilter
                ? new SiteswapGeneratorInput(6, 6, 0, 10)
                {
                    StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1_000),
                }
            : scenario
                is GenerationScenario.HighDimensionalFilteredStress
                    or GenerationScenario.HighDimensionalNoFilterStress
                ? new SiteswapGeneratorInput(30, 30, 0, 40)
                {
                    StopCriteria = new StopCriteria(TimeSpan.FromSeconds(6), 14_000_000),
                }
            : new SiteswapGeneratorInput(10, 6, 2, 10)
            {
                StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1_000),
            };
    }
}
