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

`--quick` reports five process snapshots (wall time, process CPU time, managed allocations, and peak working set). The BenchmarkDotNet run uses `ShortRun` (three warmups and three measured iterations) with `MemoryDiagnoser`.

Environment used for the measurements:

- Ubuntu 24.04 container
- .NET SDK 10.0.400 / runtime 10.0.11
- BenchmarkDotNet 0.15.8
- 16 logical CPUs

The benchmark creates a fresh, stateful `SiteswapGenerator` for every operation. This is important: reusing one generator makes subsequent operations measure an already-consumed generator rather than generation.

## Workloads

- `LargeNoFilter`: period 7, 8 objects, heights 2–13, up to 100,000 results
- `PatternFilter`: period 10, 6 objects, heights 2–10, pattern filter, up to 1,000 results
- `NumberFilter`: period 10, 6 objects, heights 2–10, exactly two throws of height 5
- `StateDontCareFilter`: period 10, 6 objects, an entirely unconstrained state pattern
- `StateSelectiveFilter`: period 10, 6 objects, a selective occupied/free state pattern; this intentionally exercises rejected candidates and produces no results within the limit

## BenchmarkDotNet comparison

Values are the mean per generator operation. The baseline is `origin/main` with the benchmark harness corrected to create a fresh generator per operation. The final run contains both state-filter optimizations.

| Scenario | Baseline time | Final time | Time change | Baseline allocated | Final allocated | Allocation change |
|---|---:|---:|---:|---:|---:|---:|
| LargeNoFilter | 4.947 ms | 5.201 ms | +5.1% | 1,865.1 KiB | 1,865.1 KiB | 0.0% |
| PatternFilter | 34.512 ms | 34.960 ms | +1.3% | 222.59 KiB | 222.59 KiB | 0.0% |
| NumberFilter | 2.616 ms | 2.447 ms | -6.5% | 213.29 KiB | 213.29 KiB | 0.0% |
| StateDontCareFilter | 695.1 µs | 389.8 µs | -43.9% | 1,236.65 KiB | 213.22 KiB | -82.8% |
| StateSelectiveFilter | 191.025 ms | 72.649 ms | -62.0% | 660,006.46 KiB | 15,116.34 KiB | -97.7% |

The selective state workload is the important result: the generator now spends about 62% less time and allocates about 645 MiB less managed memory per operation. The no-filter and pattern workloads remain within normal short-run noise; no broad generator rewrite was made because the snapshots did not identify a benefit there.

The process snapshot runner also samples CPU time and peak working set. Process CPU time is intentionally treated as diagnostic rather than the go/no-go metric because concurrent GC can finish after the measured generation interval. BenchmarkDotNet's isolated process timings and allocation counters are used for the comparison above.

## Changes justified by the data

1. `State.CalculateState` now updates the state bitmask directly while reading the current `PartialSiteswap` rotation. This removes the per-check integer-array copy, LINQ aggregation, and temporary `State` records.
2. An all-`DontCare` state pattern is recognized once when the filter is created. It skips state calculation and does not advertise rotation awareness, avoiding a redundant rotation loop.
3. Tests cover stable-state calculation, rotated siteswaps, state-pattern matching, and the unconstrained-pattern rotation contract.
