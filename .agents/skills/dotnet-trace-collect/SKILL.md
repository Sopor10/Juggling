---
name: dotnet-trace-collect
description: Use for targeted .NET CPU and GC profiling of generator benchmark processes in the Devcontainer.
license: MIT (adapted from dotnet/skills)
---

# .NET trace collection for Siteswap generation

This repository adapts the official [.NET `dotnet-trace-collect` skill](https://github.com/dotnet/skills/tree/v1.0.0/plugins/dotnet-diag/skills/dotnet-trace-collect), commit `113bb7fc905acf7fbcd8ce49fa59fc22e48c151c`.

## When to use

Use this only to explain a benchmark result after QuickBench/BenchmarkDotNet has isolated a scenario. Do not mix trace collection with timing claims.

## Container workflow

Install the diagnostic tool inside the Devcontainer, not on the host:

```bash
dotnet tool install --global dotnet-trace
dotnet tool install --global dotnet-counters
```

Verify the target process before attaching:

```bash
dotnet-trace ps
```

For a running benchmark process, collect a short CPU/GC trace:

```bash
dotnet-trace collect -p <PID> \
  --profile cpu-sampling \
  --output /tmp/siteswap-generation.nettrace
```

For GC/allocation investigation:

```bash
dotnet-trace collect -p <PID> \
  --profile gc-verbose \
  --output /tmp/siteswap-generation-gc.nettrace
```

Use `dotnet-counters monitor -p <PID> --counters System.Runtime` for live signals such as allocation rate and GC counts. Capture the runtime version, container image, commit, PID, exact command, and artifact path with every trace.

Do not treat a trace as proof of a speedup. Verify the candidate again with the deterministic benchmark and result-count gate after the change.
