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

    /// <summary>Summary: Pattern palette must offer don't-care ("frei") and new sequences default to all frei.</summary>
    [Fact]
    public async Task Pattern_Filter_Defaults_To_Frei_And_Exposes_DontCare_Palette()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        var page = session.Page;
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();
        await wizard.OpenAddFilterSheetAsync();
        await page.Locator("#wizard-filter-tab-pattern").ClickAsync();

        var palette = page.Locator(".wizard-pattern-palette .wizard-chip");
        await Assertions.Expect(palette.Filter(new() { HasText = "frei" })).ToHaveCountAsync(1);

        var slots = page.Locator(".wizard-pattern-slots .wizard-pattern-slot");
        await Assertions.Expect(slots).Not.ToHaveCountAsync(0);
        await Assertions
            .Expect(
                page.Locator(".wizard-pattern-slots .wizard-pattern-slot:not(:text-is(\"frei\"))")
            )
            .ToHaveCountAsync(0);
    }

    /// <summary>Summary: State filter must show classic x/_ notation that updates with beat toggles.</summary>
    [Fact]
    public async Task State_Filter_Shows_Occupied_Free_Notation()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        var page = session.Page;
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();
        await wizard.OpenAddFilterSheetAsync();
        await page.Locator("#wizard-filter-tab-state").ClickAsync();

        var notation = page.Locator(".wizard-state-notation");
        await Assertions.Expect(notation).ToBeVisibleAsync();
        await Assertions.Expect(notation).ToHaveTextAsync(new Regex(@"^[\s_]+$"));

        var firstBeat = page.Locator(".wizard-state-grid .wizard-chip").First;
        await firstBeat.ClickAsync();
        await Assertions.Expect(notation).ToHaveTextAsync(new Regex(@"^\s*x"));
    }

    /// <summary>Summary: State filter beat buttons cycle through occupied, don't-care, and free.</summary>
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

        await Assertions.Expect(notation).ToHaveTextAsync(new Regex(@"^\s*\*"));
    }
}
