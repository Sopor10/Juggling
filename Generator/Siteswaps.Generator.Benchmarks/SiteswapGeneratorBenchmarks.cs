using BenchmarkDotNet.Attributes;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Benchmarks;

public enum GenerationScenario
{
    LargeNoFilter,
    PatternFilter,
    NumberFilter,
    StateDontCareFilter,
    StateSelectiveFilter,
}

[MemoryDiagnoser]
[ShortRunJob]
public class SiteswapGeneratorBenchmarks
{
    private Func<SiteswapGenerator> createGenerator = null!;

    [Params(
        GenerationScenario.LargeNoFilter,
        GenerationScenario.PatternFilter,
        GenerationScenario.NumberFilter,
        GenerationScenario.StateDontCareFilter,
        GenerationScenario.StateSelectiveFilter
    )]
    public GenerationScenario Scenario { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        createGenerator = () => CreateGenerator(Scenario);
    }

    [Benchmark]
    public int GenerateSiteswaps() => createGenerator().Generate().Count();

    private static SiteswapGenerator CreateGenerator(GenerationScenario scenario)
    {
        var input = scenario is GenerationScenario.LargeNoFilter
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
                    new StatePattern(
                        [
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
                        ]
                    )
                )
                .Build(),
            GenerationScenario.StateSelectiveFilter => new FilterBuilder(input)
                .WithState(
                    new StatePattern(
                        [
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
                        ]
                    )
                )
                .Build(),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null),
        };

        return new SiteswapGenerator(filter, input);
    }
}
