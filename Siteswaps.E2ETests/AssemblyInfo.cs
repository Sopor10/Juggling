using Xunit;

// Parallelize across test classes; each test uses its own browser context.
[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 4)]
