using System.Diagnostics;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Benchmarks;

public static class QuickBench
{
    public static async Task Run()
    {
        Console.WriteLine("=== Quick Benchmark (MAIN BASELINE) ===");
        Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        Console.WriteLine();

        await RunBench("Small  (Period=3, Balls=5, Height=0-10)", new SiteswapGeneratorInput(3, 5, 0, 10)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 100000)
        });

        await RunBench("Medium (Period=5, Balls=7, Height=2-10)", new SiteswapGeneratorInput(5, 7, 2, 10)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 100000)
        });

        await RunBench("Large  (Period=7, Balls=8, Height=2-13)", new SiteswapGeneratorInput(7, 8, 2, 13)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 100000)
        });

        await RunBench("Filter (Period=10, Pattern)", new SiteswapGeneratorInput(10, 6, 2, 10)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1000)
        }, new FilterBuilder(new SiteswapGeneratorInput(10, 6, 2, 10)).Pattern([2, -1, 6, -1, 5, -1, -1, -1, -1, -1], 2).Build());
    }

    private static async Task RunBench(string name, SiteswapGeneratorInput input, ISiteswapFilter? filter = null)
    {
        // 3 warmup runs
        for (int w = 0; w < 3; w++)
        {
            var warmGen = filter != null ? new SiteswapGenerator(filter, input) : new SiteswapGenerator(input);
            await foreach (var _ in warmGen.GenerateAsync(CancellationToken.None)) { }
        }

        // 5 measured runs
        var times = new List<double>();
        var resultCount = 0;
        for (int run = 0; run < 5; run++)
        {
            var gen = filter != null ? new SiteswapGenerator(filter, input) : new SiteswapGenerator(input);
            var sw = Stopwatch.StartNew();
            var count = 0;
            await foreach (var _ in gen.GenerateAsync(CancellationToken.None))
                count++;
            sw.Stop();
            times.Add(sw.Elapsed.TotalMilliseconds);
            resultCount = count;
        }

        times.Sort();
        var median = times[times.Count / 2];
        var avg = times.Average();
        var min = times.Min();
        Console.WriteLine($"  {name}: median={median:F1}ms, avg={avg:F1}ms, min={min:F1}ms ({resultCount} results) [{string.Join(", ", times.Select(t => $"{t:F1}ms"))}]");
    }
}
