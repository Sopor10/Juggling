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
