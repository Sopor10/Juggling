using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

/// <summary>Filter sheet and list contracts for wizard step 3 (clamp feedback lives in Ux).</summary>
public class WizardFilterTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Saving a number filter must show it in the list and removing it must clear the list.</summary>
    [Fact]
    public async Task Number_Filter_Can_Be_Added_And_Removed()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();

        await wizard.SaveNumberFilterAsync("Genau", 2, "Heff");
        await Assertions.Expect(wizard.FilterCards).ToHaveCountAsync(1);
        await Assertions
            .Expect(wizard.FilterCards.First.Locator(".wizard-filter-desc"))
            .ToContainTextAsync("Genau 2× Heff");

        await wizard.RemoveFirstFilterAsync();
        await Assertions.Expect(wizard.FilterCards).ToHaveCountAsync(0);
        await Assertions.Expect(wizard.FiltersEmptyMessage).ToBeVisibleAsync();
    }

    /// <summary>Summary: State filter beat buttons cycle from don't-care through occupied and free back to don't-care.</summary>
    [Fact]
    public async Task State_Filter_Cycles_To_DontCare()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        var page = session.Page;
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();
        await wizard.OpenAddFilterSheetAsync();
        await page.Locator("#wizard-filter-tab-state").ClickAsync();

        var notation = page.Locator(".wizard-state-notation");
        var firstBeat = page.Locator(".wizard-state-grid .wizard-chip").First;
        await firstBeat.ClickAsync();
        await firstBeat.ClickAsync();
        await firstBeat.ClickAsync();

        await Assertions.Expect(notation).ToHaveTextAsync(new Regex(@"^\s*\*"));
    }
}
