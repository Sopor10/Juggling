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

Values are the mean per generator operation. The baseline is `origin/main` with the same shared scenario harness and a fresh generator per operation. The current ShortRun run used .NET 10.0.11 in the Devcontainer.

| Scenario | Baseline time | Current time | Speedup | Baseline allocated | Current allocated |
|---|---:|---:|---:|---:|---:|
| LargeNoFilter | 5.065 ms | 2.144 ms | **57.66%** | 1,865.1 KiB | 955.97 KiB |
| PatternFilter | 33.071 ms | 13.006 ms | **60.67%** | 222.59 KiB | 112.8 KiB |
| NumberFilter | 2.273 ms | 670.7 µs | **70.50%** | 213.29 KiB | 103.38 KiB |
| StateDontCareFilter | 634.5 µs | 135.8 µs | **78.59%** | 1,236.65 KiB | 103.3 KiB |
| StateSelectiveFilter | 175.830 ms | 37.910 ms | **78.44%** | 660,006.46 KiB | 15,115.79 KiB |

The sum of the five means fell from 216.873 ms to 53.867 ms (**75.16% faster**). Every individual benchmark is at least 50% faster; the slowest relative improvement is PatternFilter at 60.67%.

## Changes justified by data

1. `CyclicArray` uses an in-range branch before modulo and correctly normalizes negative rotations.
2. `PartialSiteswap` tracks landing occupancy in a `ulong` bitset for periods up to 64. Collision checks and bound searches avoid repeated cyclic-array reads; larger periods retain the safe fallback.
3. `FillCurrentPosition` rejects landing collisions before mutating the partial state. Setter updates reuse the normalized landing index.
4. `SiteswapGenerator` skips filter dispatch for `NoFilter`, performs the ball-count check only at completed leaves, and uses bitset-backed bound searches.
5. `NumberFilter` is rotation-invariant and specializes the common single-number case to direct integer comparison. A single filter is no longer wrapped in an unnecessary `AndFilter`.
6. Generated Siteswaps use a dedicated span-based construction path. The previous path created an array for `AsSpan().ToArray()` and then copied it again through `CreateFromCorrect(params ...)`; the new path performs one defensive copy.
7. `State.CalculateState` updates the state bitmask directly while reading the current `PartialSiteswap` rotation. This removes the per-check integer-array copy, LINQ aggregation, and temporary `State` records.
8. An all-`DontCare` state pattern is recognized once when the filter is created. It skips state calculation and does not advertise rotation awareness, avoiding a redundant rotation loop.
9. Scenario construction is centralized, QuickBench emits JSON, `--scenario` isolates workloads for profiling, and result-count validation is performed in both QuickBench and BenchmarkDotNet.

## Optimization process

Each candidate follows the same loop:

1. Identify a hot path or allocation from source inspection and a measured scenario.
2. Add one focused behavior/regression test and run it red when the behavior is new.
3. Implement one minimal change.
4. Run the focused test, the relevant generator tests, QuickBench, and a BenchmarkDotNet comparison.
5. Keep the change only when the intended workload improves without unacceptable regressions; otherwise revert it.

A lazy recursive generator was tested as a main-flow improvement. It was rejected because iterator state-machine allocations caused a large regression in `PatternFilter`, despite helping one state scenario. Reordering the object-count filter was also rejected after an identical-condition BDN comparison increased the selective workload from 77.61 ms to 81.51 ms. These rejected experiments are intentionally not part of the final code.
