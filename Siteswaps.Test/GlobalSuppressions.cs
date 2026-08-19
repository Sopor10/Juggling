using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1707",
    Justification = "Test names intentionally use underscores to describe the behavior under test."
)]

[assembly: SuppressMessage(
    "Performance",
    "CA1861",
    Justification = "Test-case arrays are immutable test data and are intentionally local to each case."
)]
