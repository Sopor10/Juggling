# Performance skill for the Siteswap generator

This project-local guide adapts the MIT-licensed [official .NET microbenchmarking skill](https://github.com/dotnet/skills/tree/v1.0.0/plugins/dotnet-diag/skills/microbenchmarking) and [trace collection skill](https://github.com/dotnet/skills/tree/v1.0.0/plugins/dotnet-diag/skills/dotnet-trace-collect). The repository copy is maintained under `.agents/skills/`.

## Required workflow

1. Use the Devcontainer and a Release build.
2. Keep one deterministic scenario catalog shared by QuickBench and BenchmarkDotNet.
3. Measure real `Generate()` consumption and Siteswap materialization.
4. Keep result counts stable; at most one scenario may intentionally be empty.
5. Run focused tests, then QuickBench, then BenchmarkDotNet.
6. Compare before/after with machine-readable JSON and retain runtime, container, commit, and scenario parameters.
7. Accept an optimization only when the benchmark distributions show a repeatable win without result-count or allocation regressions.

## Commands

```bash
dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj -- \
  --quick --json /tmp/siteswap-current.json

# Isolate a workload for profiling
dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj -- \
  --quick --scenario LargeNoFilter

# Validate BDN startup/configuration
dotnet run -c Release --no-restore \
  --project Generator/Siteswaps.Generator.Benchmarks/Siteswaps.Generator.Benchmarks.csproj -- \
  --job Dry --filter '*GenerateSiteswaps*'
```

Use `.agents/skills/dotnet-trace-collect/SKILL.md` for isolated CPU/GC traces. Do not use trace timings as benchmark evidence.
