using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Benchmarks;

public static class QuickBench
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static Task<int> Run(string[] args)
    {
        Console.WriteLine("=== Quick Benchmark (CPU-/Memory-Snapshot) ===");
        Console.WriteLine(
            $"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}"
        );
        Console.WriteLine();

        var reports = new List<ScenarioReport>();
        foreach (var scenario in GetScenarios(args))
        {
            reports.Add(RunBench(scenario));
        }

        if (reports.Count == GenerationScenarioFactory.AllScenarios.Count)
        {
            GenerationScenarioFactory.ValidateResultCounts(
                reports.ToDictionary(report => report.Scenario, report => report.ResultCount)
            );
        }
        else
        {
            foreach (var report in reports)
            {
                GenerationScenarioFactory.ValidateResultCount(report.Scenario, report.ResultCount);
            }
        }

        var jsonPath = GetJsonPath(args);
        if (jsonPath is not null)
        {
            var report = new QuickBenchReport(
                Environment.GetEnvironmentVariable("BENCHMARK_COMMIT"),
                System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                reports
            );
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(report, JsonOptions));
            Console.WriteLine($"JSON report: {jsonPath}");
        }

        return Task.FromResult(0);
    }

    private static ScenarioReport RunBench(GenerationScenario scenario)
    {
        for (var warmup = 0; warmup < 2; warmup++)
        {
            _ = GenerationScenarioFactory.Create(scenario).Generate().Count();
        }

        var samples = new List<Sample>();
        var sampleResultCounts = new List<int>();
        var resultCount = -1;
        for (var run = 0; run < 5; run++)
        {
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var startCpu = process.TotalProcessorTime;
            var startAllocated = GC.GetTotalAllocatedBytes(true);
            var peakWorkingSet = process.WorkingSet64;
            var stopwatch = Stopwatch.StartNew();

            var count = 0;
            foreach (var _ in GenerationScenarioFactory.Create(scenario).Generate())
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
            if (
                !GenerationScenarioFactory.IsTimeBound(scenario)
                && resultCount >= 0
                && resultCount != count
            )
            {
                throw new InvalidOperationException(
                    $"Benchmark scenario {scenario} is not deterministic: "
                        + $"sample counts were {resultCount} and {count}."
                );
            }

            resultCount = count;
            sampleResultCounts.Add(count);
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
        if (GenerationScenarioFactory.IsTimeBound(scenario))
            resultCount = sampleResultCounts.Min();
        var report = new ScenarioReport(
            scenario,
            median.WallMilliseconds,
            median.CpuMilliseconds,
            median.AllocatedBytes,
            median.PeakWorkingSetBytes,
            resultCount
        );
        Console.WriteLine(
            $"  {scenario}: wall={report.WallMilliseconds:F1}ms, "
                + $"cpu={report.CpuMilliseconds:F1}ms, "
                + $"allocated={report.AllocatedBytes / 1024d / 1024d:F1}MiB, "
                + $"peak-working-set={report.PeakWorkingSetBytes / 1024d / 1024d:F1}MiB "
                + $"({report.ResultCount} results)"
        );
        return report;
    }

    private static IEnumerable<GenerationScenario> GetScenarios(string[] args)
    {
        var index = Array.FindIndex(
            args,
            arg => arg.Equals("--scenario", StringComparison.OrdinalIgnoreCase)
        );
        if (index < 0)
            return GenerationScenarioFactory.AllScenarios;
        if (
            index + 1 >= args.Length
            || !Enum.TryParse(args[index + 1], true, out GenerationScenario scenario)
        )
            throw new ArgumentException("--scenario requires a valid GenerationScenario name.");
        return [scenario];
    }

    private static string? GetJsonPath(string[] args)
    {
        var index = Array.FindIndex(
            args,
            arg => arg.Equals("--json", StringComparison.OrdinalIgnoreCase)
        );
        if (index < 0)
            return null;
        if (index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
            throw new ArgumentException("--json requires a file path.");
        return args[index + 1];
    }

    private sealed record Sample(
        double WallMilliseconds,
        double CpuMilliseconds,
        long AllocatedBytes,
        long PeakWorkingSetBytes
    );
}

public sealed record QuickBenchReport(
    string? Commit,
    string Runtime,
    IReadOnlyList<ScenarioReport> Scenarios
);

public sealed record ScenarioReport(
    GenerationScenario Scenario,
    double WallMilliseconds,
    double CpuMilliseconds,
    long AllocatedBytes,
    long PeakWorkingSetBytes,
    int ResultCount
);
