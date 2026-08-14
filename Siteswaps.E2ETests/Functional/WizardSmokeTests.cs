using FluentAssertions;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

/// <summary>Self-hosted smoke test via <see cref="BlazorWebassemblyFixture{TEntryPoint}"/> (does not require Aspire).</summary>
[Collection(WizardE2ECollection.Name)]
public class WizardSmokeTests(BlazorWebassemblyFixture<Program> fixture)
{
    [Fact]
    public async Task Wizard_Page_Loads()
    {
        var page = await fixture.Context!.NewPageAsync();
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        (await wizard.IsLoadedAsync()).Should().BeTrue();
    }
}
