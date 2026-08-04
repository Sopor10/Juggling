```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7840/25H2/2025Update/HudsonValley2)
Intel Core Ultra 9 185H 2.50GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.103
  [Host]   : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3
  ShortRun : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method              | Period | Mean      | Error      | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------- |------- |----------:|-----------:|----------:|------:|--------:|----------:|------------:|
| **Modulo**              | **5**      |  **72.53 ns** |   **6.087 ns** |  **0.334 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| IndexCache          | 5      |  31.53 ns |  17.716 ns |  0.971 ns |  0.43 |    0.01 |         - |          NA |
| DuplicatedArray     | 5      |  24.70 ns |  11.451 ns |  0.628 ns |  0.34 |    0.01 |         - |          NA |
| BitMaskPowerOf2     | 5      |        NA |         NA |        NA |     ? |       ? |        NA |           ? |
| ConditionalSubtract | 5      |  35.09 ns |  10.490 ns |  0.575 ns |  0.48 |    0.01 |         - |          NA |
|                     |        |           |            |           |       |         |           |             |
| **Modulo**              | **10**     | **298.79 ns** |  **18.754 ns** |  **1.028 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| IndexCache          | 10     | 130.75 ns |  63.943 ns |  3.505 ns |  0.44 |    0.01 |         - |          NA |
| DuplicatedArray     | 10     |  94.21 ns |  53.209 ns |  2.917 ns |  0.32 |    0.01 |         - |          NA |
| BitMaskPowerOf2     | 10     |        NA |         NA |        NA |     ? |       ? |        NA |           ? |
| ConditionalSubtract | 10     | 107.34 ns |  33.327 ns |  1.827 ns |  0.36 |    0.01 |         - |          NA |
|                     |        |           |            |           |       |         |           |             |
| **Modulo**              | **14**     | **562.55 ns** |  **78.445 ns** |  **4.300 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| IndexCache          | 14     | 263.01 ns | 198.894 ns | 10.902 ns |  0.47 |    0.02 |         - |          NA |
| DuplicatedArray     | 14     | 169.87 ns | 159.834 ns |  8.761 ns |  0.30 |    0.01 |         - |          NA |
| BitMaskPowerOf2     | 14     |        NA |         NA |        NA |     ? |       ? |        NA |           ? |
| ConditionalSubtract | 14     | 228.69 ns |  76.224 ns |  4.178 ns |  0.41 |    0.01 |         - |          NA |

Benchmarks with issues:
  CyclicArrayBenchmarks.BitMaskPowerOf2: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3) [Period=5]
  CyclicArrayBenchmarks.BitMaskPowerOf2: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3) [Period=10]
  CyclicArrayBenchmarks.BitMaskPowerOf2: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3) [Period=14]
