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

Every scenario consumes real `Generate()` output and reports its result count. The factory rejects a zero-result scenario unless it is the explicitly documented `StateSelectiveFilter`; therefore at most one benchmark may generate zero Siteswaps.

The scenario catalog is shared by QuickBench and BenchmarkDotNet. The latest QuickBench run produced:

| Scenario | Input | Filter coverage | Results |
|---|---|---|---:|
| `LargeNoFilter` | period 7, 8 objects, heights 2–13, max 100,000 | `NoFilter` | 8,946 |
| `PatternFilter` | period 10, 6 objects, heights 2–10, max 1,000 | `AndFilter`, `AtLeastXXXTimesFilter`, `FlexiblePatternFilter` | 1,000 |
| `NumberFilter` | period 10, 6 objects, heights 2–10, exactly two 5s | `ExactlyXXXTimesFilter` | 1,000 |
| `StateDontCareFilter` | period 10, 6 objects, unconstrained state | `StatePatternFilter` | 1,000 |
| `StateSelectiveFilter` | period 10, 6 objects, selective occupied/free state | `StatePatternFilter` | 0 |
| `NumberAtMostFilter` | period 10, 6 objects, at most two 5s | `AtMostXXXTimesFilter` | 1,000 |
| `ExactStateFilter` | period 10, 6 objects, `GroundState(6)` | `StateFilter` | 1,000 |
| `NumberOfPassesFilter` | period 10, 6 objects, exactly zero passes for two jugglers | `NumberOfPassesFilter` | 564 |
| `DefaultBallCountFilter` | period 10, 6 objects, heights 2–10 | `RightAmountOfBallsFilter` | 1,000 |
| `PersonalizedNumberFilter` | period 10, 6 objects, at least one 6 from juggler 0 | `PersonalizedNumberFilter` | 1,000 |
| `RotationAwarePatternFilter` | period 10, 6 objects, unconstrained five-position pattern | `RotationAwareFlexiblePatternFilter` | 1,000 |
| `LocallyValidFilter` | period 6, 6 objects, heights 0–10 | `LocallyValidFilter` | 225 |
| `OrFilter` | period 10, 6 objects, exact-six or at-most-two-fives | `OrFilter` | 1,000 |
| `NotFilter` | period 10, 6 objects, not at-most-zero-fives | `NotFilter` | 1,000 |
| `HighDimensionalFilteredStress` | period 30, 30 objects, heights 0–40, 2 jugglers, 2,000 rotation-aware filters, time-bound 6 s | `FlexiblePatternFilter`, Number filters, `StatePatternFilter`, `PersonalizedNumberFilter`, `NotFilter`, `RotationAwareFlexiblePatternFilter`, `WithDefault()` | minimum 300 |
| `HighDimensionalNoFilterStress` | period 30, 30 objects, heights 0–40, no filters, time-bound 6 s | `NoFilter` | minimum 300,000 |
| `NestedAndNumberPattern` | period 10, 6 objects, five nested leaves | nested `And`, Number, Pattern, Default, Not | 1,000 |
| `NestedOrNotState` | period 10, 6 objects, five nested leaves | nested `Or`, `And`, `Not`, Number, Pattern, Default | 1,000 |
| `NestedStateAndPattern` | period 10, 6 objects, five nested leaves | nested `And`, `Or`, State, Pattern, Number, Default | 1,000 |
| `NestedDeepMixed` | period 10, 6 objects, seven nested leaves | deep `And`, `Or`, `Not`, Number, Passes, Pattern, Default | 1,000 |
| `NestedNumberPassesPersonalized` | period 10, 6 objects, seven nested leaves | nested Number, Passes, Personalized, Pattern, Default, Not | 1,000 |

The five original performance-comparison scenarios remain the primary optimization gate. A complete 14-scenario baseline/current comparison is stored in `PERFORMANCE-baseline.json` and was measured with the same `ShortRun` configuration and the same scenario catalog:

| Scenario | Baseline | Current | Speedup | Results |
|---|---:|---:|---:|---:|
| `LargeNoFilter` | 4.846 ms | 1.861 ms | **61.60%** | 8,946 |
| `PatternFilter` | 33.844 ms | 14.870 ms | **56.06%** | 1,000 |
| `NumberFilter` | 2.219 ms | 642.1 µs | **71.06%** | 1,000 |
| `StateDontCareFilter` | 694.3 µs | 119.3 µs | **82.82%** | 1,000 |
| `StateSelectiveFilter` | 194.960 ms | 38.356 ms | **80.33%** | 0 |
| `NumberAtMostFilter` | 517.6 µs | 170.2 µs | **67.12%** | 1,000 |
| `ExactStateFilter` | 1.452 ms | 297.4 µs | **79.51%** | 1,000 |
| `NumberOfPassesFilter` | 805.8 µs | 536.6 µs | **33.41%** | 564 |
| `DefaultBallCountFilter` | 388.7 µs | 125.6 µs | **67.69%** | 1,000 |
| `PersonalizedNumberFilter` | 1.481 ms | 992.3 µs | **33.01%** | 1,000 |
| `RotationAwarePatternFilter` | 430.7 µs | 167.6 µs | **61.09%** | 1,000 |
| `LocallyValidFilter` | 745.7 µs | 686.7 µs | **7.91%** | 225 |
| `OrFilter` | 777.7 µs | 288.3 µs | **62.93%** | 1,000 |
| `NotFilter` | 427.3 µs | 183.5 µs | **57.06%** | 1,000 |

The original five-scenario aggregate remains **75.16% faster**. The complete filter-coverage catalog is not yet a 50%-faster gate: `NumberOfPassesFilter`, `PersonalizedNumberFilter`, and `LocallyValidFilter` require separate optimization work. The baseline JSON also records error, standard deviation, managed allocations, runtime, runner, commits, and result counts for reproducible future comparisons.

## High-dimensional stress workloads

Both long-running stress scenarios use period 30, 30 objects, heights 0–40, `StopCriteria.TimeOut = 6 s`, and `MaxNumberOfResults = 14,000,000`.

`HighDimensionalFilteredStress` verwendet zusätzlich personenspezifische Filter für 2 Jugglers, erlaubt an Position 0 nur gerade Würfe und wertet 2.000 rotationsbewusste Pattern-Filter sowie Number-, State-, Personalized-, Default- und Not-Filter aus. Es muss mindestens 300 Ergebnisse erzeugen. Der letzte vollständige QuickBench-Katalog erzeugte konservativ mindestens **1.590 Ergebnisse** über fünf Samples in **6,007 s** Wall time und allokierte **2,3 MiB** verwalteten Speicher. Benchmark.NET maß **6,006 s** pro `Generate()` und **2,25 MB** Allokationen.

`HighDimensionalNoFilterStress` verwendet dieselbe hochdimensionale Eingabe ohne Filter. Es muss mindestens 300.000 Ergebnisse erzeugen. Der letzte vollständige QuickBench-Katalog erzeugte konservativ mindestens **12.663.839 Ergebnisse** in **10,554 s** Median-Wall-time, allokierte **3,41 GiB** und erreichte einen Peak-Working-Set von **2,49 GiB**. Benchmark.NET erreichte den Zeitstopp bei **6,000 s**, allokierte **2,23 GB** und erzeugte vor dem Zeitlimit deutlich mehr als die Mindestzahl von 300.000 Ergebnissen.

QuickBench does not require identical result counts for these time-bound scenarios because the exact count depends on scheduling and machine load. It validates the minimum count and reports the minimum count across the five samples. All non-time-bound scenarios retain the exact result-count determinism gate.

Run either workload with:

```bash
dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj \
  -- --quick --scenario HighDimensionalFilteredStress

dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj \
  -- --quick --scenario HighDimensionalNoFilterStress
```

Die frühere `CollectionsMarshal.AsSpan`-Variante bleibt verworfen: der vergleichbare Benchmark.NET-Lauf regressierte auf **7,432 s**. Die `NumberMask`-Optimierung bleibt dagegen übernommen, weil sie im gefilterten Stressfall nachweislich Allokationen und Laufzeit reduziert.

## Follow-up optimization in the separate PR

This follow-up is based on the current PR head but is intentionally kept on a separate branch and pull request. Three measured hot-loop changes were retained:

1. `AndFilter` partitions rotation-invariant and rotation-aware filters once in its constructor. `CanFulfillAnyRotation` no longer checks `IsRotationAware` for every filter on every rotation.
2. `FlexiblePatternFilter` and `RotationAwareFlexiblePatternFilter` skip the common singleton `DontCare` pattern position directly in their existing loop. No additional per-filter position array is allocated.
3. An explicit `CanRejectPartial` capability allows filters that cannot reject a partial prefix to be skipped safely until the Siteswap is filled. Unknown/custom filters retain the conservative default and are still evaluated.

On the identical fixed-count stress profile (period 30, 30 objects, heights 0–40, 2,000 filters, 300 results), the measured means were:

| Variant | Mean | Allocated |
|---|---:|---:|
| Existing PR baseline | 4.756 s | approximately 1.9 MB |
| Filter-list partitioning | 3.604 s | 1.93 MB |
| Plus singleton `DontCare` skip | 3.361 s | 1.93 MB |
| Plus safe partial-prefix capability | **673.1 ms** | **1.92 MB** |

This is a measured **85.85%** improvement over the existing PR baseline. The final time-bound profile remains intact: Benchmark.NET measured **6.005 s** with the six-second stop criterion, and the latest full QuickBench catalog produced **14,225 filtered results** while all existing result counts remained unchanged. Capability-specific tests cover both filters that must still be queried on partial values and filters that explicitly opt into the safe skip. The full test run passed **242 tests** with **3 skips**.

## Nested-filter bottleneck analysis

Five additional scenarios use only 5–7 leaf filters but deliberately nest `And`, `Or`, and `Not` compositions. All produce exactly 1,000 results. Benchmark.NET measurements on .NET 10.0.11 were:

| Scenario | Mean | Allocated |
|---|---:|---:|
| `NestedAndNumberPattern` | 487.0 µs | 106.99 KB |
| `NestedOrNotState` | 295.9 µs | 358.54 KB |
| `NestedStateAndPattern` | 457.5 µs | 378.02 KB |
| `NestedDeepMixed` | 480.1 µs | 136.3 KB |
| `NestedNumberPassesPersonalized` | **587.3 µs** | 370.44 KB |

The current CPU bottleneck among typical nested compositions is `NestedNumberPassesPersonalized`. The largest allocation signal is `NestedStateAndPattern`, followed by `NestedNumberPassesPersonalized` and `NestedOrNotState`. The `OrFilter` partial-capability fix reduced `NestedDeepMixed` from roughly 772 µs / 529.79 KB to 480.1 µs / 136.3 KB.

This points to two next investigation areas: safe early exits inside Number/Passes/Personalized leaves, and repeated allocation/dispatch in nested State/Number compositions. Broad filter fusion and automatic tree flattening remain out of scope because earlier experiments regressed the measured catalog and can change short-circuit semantics.

## Filter coverage matrix

| Filter class | Scenario | Coverage |
|---|---|---|
| `NoFilter` | `LargeNoFilter` | direct |
| `AtLeastXXXTimesFilter` | `PatternFilter` | indirect through `PatternFilterHeuristicBuilder` |
| `ExactlyXXXTimesFilter` | `NumberFilter` | direct |
| `AtMostXXXTimesFilter` | `NumberAtMostFilter` | direct |
| `PersonalizedNumberFilter` | `PersonalizedNumberFilter` | direct |
| `StateFilter` | `ExactStateFilter` | direct |
| `StatePatternFilter` | `StateDontCareFilter`, `StateSelectiveFilter` | direct, unconstrained and selective |
| `FlexiblePatternFilter` | `PatternFilter` | indirect through `FilterBuilder.Pattern` |
| `RotationAwareFlexiblePatternFilter` | `RotationAwarePatternFilter` | direct |
| `NumberOfPassesFilter` | `NumberOfPassesFilter` | direct |
| `RightAmountOfBallsFilter` | `DefaultBallCountFilter` | direct through `WithDefault()` |
| `LocallyValidFilter` | `LocallyValidFilter` | direct |
| `AndFilter` | `PatternFilter` | indirect composition |
| `OrFilter` | `OrFilter` | direct composition |
| `NotFilter` | `NotFilter` | direct composition |

This covers every concrete `ISiteswapFilter` implementation in `Generator/Siteswaps.Generator.Core/Generator/Filter`. `NumberFilter` itself is abstract; `State`, `StatePattern`, and `PatternFilterHeuristicBuilder` are supporting types rather than additional filter implementations.

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
3. `FillCurrentPosition` rejects landing collisions before mutating the partial state. Setter updates reuse the normalized landing indexes.
4. `SiteswapGenerator` skips filter dispatch for `NoFilter`, performs the ball-count check only at completed leaves, and uses bitset-backed bound searches.
5. `NumberFilter` is rotation-invariant and specializes the common single-number case to direct integer comparison. A single filter is no longer wrapped in an unnecessary `AndFilter`.
6. Generated Siteswaps use a dedicated span-based construction path. The previous path created an array for `AsSpan().ToArray()` and then copied it again through `CreateFromCorrect(params ...)`; the new path performs one defensive copy.
7. `State.CalculateState` updates the state bitmask directly while reading the current `PartialSiteswap` rotation. This removes the per-check integer-array copy, LINQ aggregation, and temporary `State` records.
8. An all-`DontCare` state pattern is recognized once when the filter is created. It skips state calculation and does not advertise rotation awareness, avoiding a redundant rotation loop.
9. Scenario construction is centralized, QuickBench emits JSON, `--scenario` isolates workloads, and result-count validation is performed in both QuickBench and BenchmarkDotNet.
10. `NumberMask` replaces repeated Self/Pass `HashSet<int>.Contains` lookups for heights 0–63 and retains a safe overflow fallback. This was accepted only after the high-dimensional stress benchmark improved by 14.5% and reduced allocations by about 72%; the `CollectionsMarshal.AsSpan` alternative was rejected after a measured regression.

## Optimization process

Each candidate follows the same loop:

1. Identify a hot path or allocation from source inspection and a measured scenario.
2. Add one focused behavior/regression test and run it red when the behavior is new.
3. Implement one minimal change.
4. Run the focused test, the relevant generator tests, QuickBench, and a BenchmarkDotNet comparison.
5. Keep the change only when the intended workload improves without unacceptable regressions; otherwise revert it.

A lazy recursive generator was tested as a main-flow improvement. It was rejected because iterator state-machine allocations caused a large regression in `PatternFilter`, despite helping one state scenario. Reordering the object-count filter was also rejected after an identical-condition BDN comparison increased the selective workload from 77.61 ms to 81.51 ms. These rejected experiments are intentionally not part of the final code.
