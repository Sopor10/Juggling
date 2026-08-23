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
}

[MemoryDiagnoser]
[ShortRunJob]
public class SiteswapGeneratorBenchmarks
{
    private Func<SiteswapGenerator> createGenerator = null!;

    [ParamsSource(nameof(Scenarios))]
    public GenerationScenario Scenario { get; set; }

    public static IEnumerable<GenerationScenario> Scenarios =>
        GenerationScenarioFactory.AllScenarios;

    [GlobalSetup]
    public void Setup()
    {
        createGenerator = () => GenerationScenarioFactory.Create(Scenario);
    }

    [Benchmark]
    public int GenerateSiteswaps()
    {
        var resultCount = createGenerator().Generate().Count();
        GenerationScenarioFactory.ValidateResultCount(Scenario, resultCount);
        return resultCount;
    }
}
