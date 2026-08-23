using BenchmarkDotNet.Attributes;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Benchmarks;

public enum GenerationScenario
{
    LargeNoFilter,
    PatternFilter,
    NumberFilter,
    StateDontCareFilter,
    StateSelectiveFilter,
    NumberAtMostFilter,
    ExactStateFilter,
    NumberOfPassesFilter,
    DefaultBallCountFilter,
    PersonalizedNumberFilter,
    RotationAwarePatternFilter,
    LocallyValidFilter,
    OrFilter,
    NotFilter,
    HighDimensionalFilteredStress,
    HighDimensionalNoFilterStress,
    NestedAndNumberPattern,
    NestedOrNotState,
    NestedStateAndPattern,
    NestedDeepMixed,
    NestedNumberPassesPersonalized,
}

[MemoryDiagnoser]
[ShortRunJob]
public class SiteswapGeneratorBenchmarks
{
    private Func<SiteswapGenerator> createGenerator = null!;

    [ParamsSource(nameof(Scenarios))]
    public GenerationScenario Scenario { get; set; }

    public static IEnumerable<GenerationScenario> Scenarios =>
        GenerationScenarioFactory.AllScenarios.Concat(NestedGenerationScenarioFactory.AllScenarios);

    [GlobalSetup]
    public void Setup()
    {
        createGenerator = () =>
            NestedGenerationScenarioFactory.AllScenarios.Contains(Scenario)
                ? NestedGenerationScenarioFactory.Create(Scenario)
                : GenerationScenarioFactory.Create(Scenario);
    }

    [Benchmark]
    public int GenerateSiteswaps()
    {
        var resultCount = createGenerator().Generate().Count();
        if (NestedGenerationScenarioFactory.AllScenarios.Contains(Scenario))
            NestedGenerationScenarioFactory.ValidateResultCount(Scenario, resultCount);
        else
            GenerationScenarioFactory.ValidateResultCount(Scenario, resultCount);
        return resultCount;
    }
}
