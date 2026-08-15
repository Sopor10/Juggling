using FluentAssertions;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

/// <summary>Self-hosted smoke test via <see cref="BlazorWebassemblyFixture{TEntryPoint}"/> (does not require Aspire).</summary>
public class WizardSmokeTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    [Fact]
    public async Task Wizard_Page_Loads()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        (await wizard.IsLoadedAsync()).Should().BeTrue();
    }
}
