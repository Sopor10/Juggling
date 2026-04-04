using BenchmarkDotNet.Attributes;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class SiteswapGeneratorBenchmarks
{
    [Benchmark]
    public async Task<int> Small_Period3_Balls5()
    {
        var input = new SiteswapGeneratorInput(3, 5, 0, 10)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 100000)
        };
        var generator = new SiteswapGenerator(new NoFilter(), input);
        var count = 0;
        await foreach (var _ in generator.GenerateAsync(CancellationToken.None))
            count++;
        return count;
    }

    [Benchmark]
    public async Task<int> Medium_Period5_Balls7()
    {
        var input = new SiteswapGeneratorInput(5, 7, 2, 10)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 100000)
        };
        var generator = new SiteswapGenerator(new NoFilter(), input);
        var count = 0;
        await foreach (var _ in generator.GenerateAsync(CancellationToken.None))
            count++;
        return count;
    }

    [Benchmark]
    public async Task<int> Large_Period7_Balls8()
    {
        var input = new SiteswapGeneratorInput(7, 8, 2, 13)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 100000)
        };
        var generator = new SiteswapGenerator(new NoFilter(), input);
        var count = 0;
        await foreach (var _ in generator.GenerateAsync(CancellationToken.None))
            count++;
        return count;
    }

    [Benchmark]
    public async Task<int> WithPatternFilter()
    {
        var input = new SiteswapGeneratorInput(10, 6, 2, 10)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1000)
        };
        var filter = new FilterBuilder(input)
            .Pattern([2, -1, 6, -1, 5, -1, -1, -1, -1, -1], 2)
            .Build();
        var generator = new SiteswapGenerator(filter, input);
        var count = 0;
        await foreach (var _ in generator.GenerateAsync(CancellationToken.None))
            count++;
        return count;
    }
}
