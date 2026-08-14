using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests;

/// <summary>
/// Shares one <see cref="BlazorWebassemblyFixture{TEntryPoint}"/> across wizard E2E classes
/// so the reusable Playwright Docker container is not stopped between classes.
/// </summary>
[CollectionDefinition(Name)]
public sealed class WizardE2ECollection : ICollectionFixture<BlazorWebassemblyFixture<Program>>
{
    public const string Name = "WizardE2E";
}
