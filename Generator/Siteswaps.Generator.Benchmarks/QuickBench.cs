using System.Diagnostics;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Benchmarks;

public static class QuickBench
{
    public static Task Run()
    {
        Console.WriteLine("=== Quick Benchmark (CPU-/Memory-Snapshot) ===");
        Console.WriteLine(
            $"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}"
        );
        Console.WriteLine();

        RunBench(
            "Large / NoFilter",
            () => CreateGenerator(GenerationScenario.LargeNoFilter)
        );
        RunBench(
            "Medium / PatternFilter",
            () => CreateGenerator(GenerationScenario.PatternFilter)
        );
        RunBench(
            "Medium / NumberFilter",
            () => CreateGenerator(GenerationScenario.NumberFilter)
        );
        RunBench(
            "Medium / StateDontCare",
            () => CreateGenerator(GenerationScenario.StateDontCareFilter)
        );
        RunBench(
            "Medium / StateSelective",
            () => CreateGenerator(GenerationScenario.StateSelectiveFilter)
        );

        return Task.CompletedTask;
    }

    private static void RunBench(string name, Func<SiteswapGenerator> createGenerator)
    {
        for (var warmup = 0; warmup < 2; warmup++)
        {
            foreach (var _ in createGenerator().Generate()) { }
        }

        var samples = new List<Sample>();
        var resultCount = 0;
        for (var run = 0; run < 5; run++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var startCpu = process.TotalProcessorTime;
            var startAllocated = GC.GetTotalAllocatedBytes(true);
            var peakWorkingSet = process.WorkingSet64;
            var stopwatch = Stopwatch.StartNew();

            var count = 0;
            foreach (var _ in createGenerator().Generate())
            {
                count++;
                if ((count & 255) == 0)
                {
                    process.Refresh();
                    peakWorkingSet = Math.Max(peakWorkingSet, process.WorkingSet64);
                }
            }

            process.Refresh();
            stopwatch.Stop();
            peakWorkingSet = Math.Max(peakWorkingSet, process.WorkingSet64);
            resultCount = count;
            samples.Add(
                new Sample(
                    stopwatch.Elapsed.TotalMilliseconds,
                    (process.TotalProcessorTime - startCpu).TotalMilliseconds,
                    GC.GetTotalAllocatedBytes(true) - startAllocated,
                    peakWorkingSet
                )
            );
        }

        samples.Sort((left, right) => left.WallMilliseconds.CompareTo(right.WallMilliseconds));
        var median = samples[samples.Count / 2];
        Console.WriteLine(
            $"  {name}: wall={median.WallMilliseconds:F1}ms, cpu={median.CpuMilliseconds:F1}ms, "
                + $"allocated={median.AllocatedBytes / 1024d / 1024d:F1}MiB, "
                + $"peak-working-set={median.PeakWorkingSetBytes / 1024d / 1024d:F1}MiB "
                + $"({resultCount} results)"
        );
    }

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

    private sealed record Sample(
        double WallMilliseconds,
        double CpuMilliseconds,
        long AllocatedBytes,
        long PeakWorkingSetBytes
    );
}
