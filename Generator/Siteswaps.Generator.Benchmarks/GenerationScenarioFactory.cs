using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

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
    ];

    public static SiteswapGenerator Create(GenerationScenario scenario)
    {
        var input =
            scenario is GenerationScenario.LargeNoFilter
                ? new SiteswapGeneratorInput(7, 8, 2, 13)
                {
                    StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 100_000),
                }
                : new SiteswapGeneratorInput(10, 6, 2, 10)
                {
                    StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1_000),
                };

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
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null),
        };

        return new SiteswapGenerator(filter, input);
    }

    public static void ValidateResultCount(GenerationScenario scenario, int resultCount)
    {
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
}
