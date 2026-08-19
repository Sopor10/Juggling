using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1707",
    Justification = "Benchmark names intentionally use underscores to describe their workload."
)]

[assembly: SuppressMessage(
    "Design",
    "CA1822",
    Justification = "BenchmarkDotNet requires benchmark methods to remain instance methods."
)]
