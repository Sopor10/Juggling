using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

/// <summary>Step state preservation and reload contracts for /wizard (history/double-next live in Ux).</summary>
public class WizardNavigationTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Jugglers and period chosen on step 1 must survive forward and back navigation.</summary>
    [Fact]
    public async Task Jugglers_And_Period_Survive_Step_Navigation()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        var page = session.Page;
        await wizard.WaitUntilLoadedAsync();

        await wizard.SelectJugglerChipAsync(3);
        await wizard.SetPeriodAsync(7);
        await Assertions.Expect(wizard.JugglerExactInput).ToHaveValueAsync("3");
        await Assertions.Expect(wizard.PeriodInput).ToHaveValueAsync("7");
        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);
        await wizard.ClickBackAsync();
        await wizard.ExpectStepAsync(0);

        await Assertions.Expect(wizard.PeriodInput).ToHaveValueAsync("7");
        await Assertions.Expect(wizard.JugglerExactInput).ToHaveValueAsync("3");
    }

    /// <summary>Summary: Full reload must open a coherent editing session at step 1 with default inputs.</summary>
    [Fact]
    public async Task Reload_Opens_Default_Editing_Session()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        var page = session.Page;
        await wizard.WaitUntilLoadedAsync();

        await wizard.SelectJugglerChipAsync(4);
        await wizard.SetPeriodAsync(9);
        await wizard.AdvanceToFiltersAsync();

        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        wizard = new WizardPageObject(page);
        await wizard.WaitUntilLoadedAsync();

        await wizard.ExpectStepAsync(0);
        await Assertions.Expect(wizard.PeriodInput).ToHaveValueAsync("5");
        await Assertions.Expect(wizard.JugglerExactInput).ToHaveValueAsync("2");
        await Assertions.Expect(wizard.Results).ToBeHiddenAsync();
    }
}
