using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Naming", "CA1707", Justification = "Test names intentionally use underscores to describe the behavior under test.")]
[assembly: SuppressMessage("Performance", "CA1861", Justification = "Test-case arrays are immutable test data and are intentionally local to each case.")]
[assembly: SuppressMessage("Design", "CA1822", Justification = "Test helpers are invoked through the test framework.")]
[assembly: SuppressMessage("Design", "CA1852", Justification = "Test helper types are discovered by the test framework.")]
