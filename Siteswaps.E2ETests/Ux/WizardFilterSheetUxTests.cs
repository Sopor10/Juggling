using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Ux;

/// <summary>Encodes filter bottom-sheet focus, dismiss, and contrast UX contracts.</summary>
public class WizardFilterSheetUxTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Filter sheet must trap Tab focus inside the dialog while open.</summary>
    [Fact]
    public async Task Filter_Sheet_Traps_Keyboard_Focus()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();
        await wizard.OpenAddFilterSheetAsync();

        for (var i = 0; i < 12; i++)
        {
            await page.Keyboard.PressAsync("Tab");
            var summary = await WizardUxGeometry.ActiveElementSummaryAsync(page);
            summary
                .Should()
                .EndWith(":true", because: "focus must stay inside the open filter sheet");
        }
    }

    /// <summary>Summary: Escape and backdrop dismiss must close the filter sheet without trapping the user.</summary>
    [Fact]
    public async Task Filter_Sheet_Dismisses_With_Escape_And_Backdrop()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();

        await wizard.OpenAddFilterSheetAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(wizard.FilterSheet).ToBeHiddenAsync();

        await wizard.OpenAddFilterSheetAsync();
        await wizard.FilterSheetBackdrop.ClickAsync(
            new LocatorClickOptions
            {
                Position = new Position { X = 8, Y = 8 },
            }
        );
        await Assertions.Expect(wizard.FilterSheet).ToBeHiddenAsync();
    }
}
