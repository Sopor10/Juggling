---
name: microbenchmarking
description: Use for BenchmarkDotNet benchmark design, validation, comparison, and regression gates in this repository.
license: MIT (adapted from dotnet/skills)
---

# Siteswap microbenchmarking

This repository adapts the official [.NET `microbenchmarking` skill](https://github.com/dotnet/skills/tree/v1.0.0/plugins/dotnet-diag/skills/microbenchmarking), commit `113bb7fc905acf7fbcd8ce49fa59fc22e48c151c`.

## Rules

1. Benchmark real generation, not factory construction or an unconsumed lazy sequence.
2. Run Release builds inside the Devcontainer.
3. Keep input scenarios deterministic and shared by QuickBench and BenchmarkDotNet.
4. Return or consume benchmark results so dead-code elimination cannot remove work.
5. Keep setup outside the timed method (`GlobalSetup`); do not add manual loops to BDN methods.
6. Use `[MemoryDiagnoser]`; allocations are a first-class metric for this generator.
7. Validate with `--job Dry`, iterate with `--job Short`, and use the default/longer job for final claims.
8. Compare one change at a time. A result is inconclusive when error ranges overlap materially.
9. Record wall time, CPU time, allocated bytes, result count, and the exact commit/runtime/container.
10. Do not change a workload, stop criterion, or result count merely to improve a ratio.

## Project commands

```bash
dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj -- \
  --quick

dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj -- \
  --job Dry --filter '*GenerateSiteswaps*'
```

The QuickBench command is a correctness gate: every scenario must generate at least one Siteswap except the explicitly documented empty scenario, and no more than one scenario may be empty. A non-zero exit code is required on violation.

## Performance gate

For a baseline/current JSON pair, use the repository comparison tool. A strict 2x gate means every comparable scenario must have `current_wall_ms <= baseline_wall_ms / 2`; result counts must remain identical. Never report the aggregate as a success when an individual scenario regresses.

## Profiling escalation

Use `dotnet-trace` or `dotnet-counters` only after a reproducible benchmark identifies a hot scenario. Keep profiling runs separate from timing runs because diagnostics perturb timings.
