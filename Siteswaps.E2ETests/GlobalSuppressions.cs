using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1707",
    Justification = "E2E test names intentionally use underscores to describe the behavior under test."
)]
[assembly: SuppressMessage(
    "Performance",
    "CA1822",
    Justification = "Design page helpers keep an instance API for fluent test readability.",
    Scope = "type",
    Target = "~T:Siteswaps.E2ETests.Design.WizardDesignPage"
)]
