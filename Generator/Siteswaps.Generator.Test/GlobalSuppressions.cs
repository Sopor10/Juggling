using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1707",
    Justification = "Test names intentionally use underscores to describe the behavior under test."
)]

[assembly: SuppressMessage(
    "Performance",
    "CA1861",
    Justification = "NUnit test-case arrays are immutable test data and are intentionally local to each case."
)]

[assembly: SuppressMessage(
    "Design",
    "CA1822",
    Justification = "NUnit and snapshot test helpers are invoked through the test framework."
)]

[assembly: SuppressMessage(
    "Design",
    "CA1852",
    Justification = "NUnit test source helpers are discovered by the test framework."
)]

[assembly: SuppressMessage(
    "Design",
    "CA1010",
    Justification = "NUnit test case source types use the framework's non-generic discovery contract."
)]

[assembly: SuppressMessage(
    "Naming",
    "CA1710",
    Justification = "NUnit test case source names follow the established test convention."
)]
