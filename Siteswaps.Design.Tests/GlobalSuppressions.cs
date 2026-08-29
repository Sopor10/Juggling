using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

[assembly: LevelOfParallelism(6)]

[assembly: SuppressMessage(
    "Naming",
    "CA1707",
    Justification = "Design test names intentionally use underscores to describe the behavior under test."
)]
[assembly: SuppressMessage(
    "Design",
    "CA1001",
    Justification = "DesignTestHostFixture lifetime is owned by NUnit OneTimeSetUp/TearDown.",
    Scope = "type",
    Target = "~T:Siteswaps.Design.Tests.Infrastructure.DesignTestHostFixture"
)]
[assembly: SuppressMessage(
    "Design",
    "CA1001",
    Justification = "PlaywrightDockerFixture lifetime is owned by DesignTestHostFixture DisposeAsync.",
    Scope = "type",
    Target = "~T:Siteswaps.Design.Tests.Infrastructure.PlaywrightDockerFixture"
)]
[assembly: SuppressMessage(
    "Design",
    "CA1001",
    Justification = "BrowserContextPool lifetime is owned by DesignTestHostFixture DisposeAsync.",
    Scope = "type",
    Target = "~T:Siteswaps.Design.Tests.Infrastructure.BrowserContextPool"
)]
