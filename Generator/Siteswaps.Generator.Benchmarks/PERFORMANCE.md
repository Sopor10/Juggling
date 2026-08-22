# Siteswap generation performance

## Reproduction

Run inside the repository Devcontainer with .NET 10:

```bash
dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj \
  -- --quick

dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj \
  -- --filter "*GenerateSiteswaps*"
```

`--quick` reports process snapshots (wall time, process CPU time, managed allocations, peak working set, and generated result count). The BenchmarkDotNet run uses `ShortRun` with `MemoryDiagnoser`.

Environment used for the measurements:

- Ubuntu 24.04 container
- .NET SDK 10.0.400 / runtime 10.0.11
- BenchmarkDotNet 0.15.8
- 16 logical CPUs

The benchmark creates a fresh, stateful `SiteswapGenerator` for every operation. The scenario definitions are shared by QuickBench and BenchmarkDotNet through `GenerationScenarioFactory`, so the two measurement paths cannot silently drift apart.

## Workloads and result-count rule

- `LargeNoFilter`: period 7, 8 objects, heights 2–13, up to 100,000 results
- `PatternFilter`: period 10, 6 objects, heights 2–10, pattern filter, up to 1,000 results
- `NumberFilter`: period 10, 6 objects, exactly two throws of height 5, up to 1,000 results
- `StateDontCareFilter`: period 10, 6 objects, entirely unconstrained state pattern
- `StateSelectiveFilter`: period 10, 6 objects, selective occupied/free state pattern

Every scenario consumes `Generate()` and reports its result count. The factory rejects a zero-result scenario unless it is the explicitly documented `StateSelectiveFilter`; therefore at most one benchmark may generate zero Siteswaps. The current quick run produced:

```text
Large / NoFilter:       8946 results
PatternFilter:          1000 results
NumberFilter:           1000 results
StateDontCare:          1000 results
StateSelective:         0 results
```

## BenchmarkDotNet comparison

Values are the mean per generator operation. The baseline is `origin/main` with the benchmark harness corrected to create a fresh generator per operation. The current run includes the state-filter optimizations and the generated-result copy optimization.

| Scenario | Baseline time | Current time | Baseline allocated | Current allocated |
|---|---:|---:|---:|---:|
| LargeNoFilter | 4.947 ms | 4.613 ms | 1,865.1 KiB | 956.55 KiB |
| PatternFilter | 34.512 ms | 36.394 ms | 222.59 KiB | 113.21 KiB |
| NumberFilter | 2.616 ms | 2.246 ms | 213.29 KiB | 103.91 KiB |
| StateDontCareFilter | 695.1 µs | 338.2 µs | 1,236.65 KiB | 103.84 KiB |
| StateSelectiveFilter | 191.025 ms | 73.608 ms | 660,006.46 KiB | 15,116.34 KiB |

The allocation reduction from the generated-result copy is approximately 49–51% for scenarios that produce Siteswaps. The selective state workload retains the earlier state optimizations and reduces time by about 61.5% and managed allocations by about 97.7% relative to the original baseline. Pattern-filter time remains a known mixed result; no pattern-specific optimization is claimed from this run.

## Changes justified by data

1. `State.CalculateState` updates the state bitmask directly while reading the current `PartialSiteswap` rotation. This removes the per-check integer-array copy, LINQ aggregation, and temporary `State` records.
2. An all-`DontCare` state pattern is recognized once when the filter is created. It skips state calculation and does not advertise rotation awareness, avoiding a redundant rotation loop.
3. Generated Siteswaps use a dedicated span-based construction path. The previous path created an array for `AsSpan().ToArray()` and then copied it again through `CreateFromCorrect(params ...)`; the new path performs one defensive copy.
4. Scenario construction is centralized and result-count validation is performed in both QuickBench and BenchmarkDotNet.

## Optimization process

Each candidate follows the same loop:

1. Identify a hot path or allocation from source inspection and a measured scenario.
2. Add one focused behavior/regression test and run it red when the behavior is new.
3. Implement one minimal change.
4. Run the focused test, the relevant generator tests, QuickBench, and a BenchmarkDotNet comparison.
5. Keep the change only when the intended workload improves without unacceptable regressions; otherwise revert it.

A lazy recursive generator was tested as a main-flow improvement. It was rejected because iterator state-machine allocations caused a large regression in `PatternFilter`, despite helping one state scenario. Reordering the object-count filter was also rejected after an identical-condition BDN comparison increased the selective workload from 77.61 ms to 81.51 ms. These rejected experiments are intentionally not part of the final code.
