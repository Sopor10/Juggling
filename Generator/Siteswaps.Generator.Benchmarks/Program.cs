using BenchmarkDotNet.Running;
using Siteswaps.Generator.Benchmarks;

if (args.Length == 0 || args[0].Equals("--quick", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await QuickBench.Run(args);
}
else
{
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
