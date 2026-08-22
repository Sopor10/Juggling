using System.Diagnostics;
using Siteswaps.Generator.Core.Generator;

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

        RunBench("Large / NoFilter", GenerationScenario.LargeNoFilter);
        RunBench("Medium / PatternFilter", GenerationScenario.PatternFilter);
        RunBench("Medium / NumberFilter", GenerationScenario.NumberFilter);
        RunBench("Medium / StateDontCare", GenerationScenario.StateDontCareFilter);
        RunBench("Medium / StateSelective", GenerationScenario.StateSelectiveFilter);

        return Task.CompletedTask;
    }

    private static void RunBench(string name, GenerationScenario scenario)
    {
        Func<SiteswapGenerator> createGenerator = () => GenerationScenarioFactory.Create(scenario);

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

        GenerationScenarioFactory.ValidateResultCount(scenario, resultCount);
        samples.Sort((left, right) => left.WallMilliseconds.CompareTo(right.WallMilliseconds));
        var median = samples[samples.Count / 2];
        Console.WriteLine(
            $"  {name}: wall={median.WallMilliseconds:F1}ms, cpu={median.CpuMilliseconds:F1}ms, "
                + $"allocated={median.AllocatedBytes / 1024d / 1024d:F1}MiB, "
                + $"peak-working-set={median.PeakWorkingSetBytes / 1024d / 1024d:F1}MiB "
                + $"({resultCount} results)"
        );
    }

    private sealed record Sample(
        double WallMilliseconds,
        double CpuMilliseconds,
        long AllocatedBytes,
        long PeakWorkingSetBytes
    );
}
