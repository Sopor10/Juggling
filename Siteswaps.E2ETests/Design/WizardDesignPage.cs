using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Design;

/// <summary>Design-focused locators and computed-style reads for the /wizard Passing Zone UI.</summary>
public sealed class WizardDesignPage(IPage page, WizardPageObject wizard)
{
    public WizardPageObject Wizard => wizard;

    public ILocator Header => page.Locator(".wizard-header");

    public ILocator Logo => page.Locator(".pznav-logo");

    public ILocator StepSheet => page.Locator(".wizard-steps");

    public ILocator DisplaySample => page.Locator(".wizard-period-value");

    public ILocator PrimaryForwardButton =>
        page.Locator(
            ".wizard-nav-buttons .wizard-btn-primary, .wizard-nav-buttons .wizard-btn-generate"
        );

    public ILocator GenerateButton => page.Locator(".wizard-nav-buttons .wizard-btn-generate");

    public ILocator GhostBackButton => page.Locator(".wizard-nav-buttons .wizard-btn-ghost");

    public ILocator ActiveProgressDot => page.Locator(".wizard-dot.active");

    public ILocator SwipeHint => page.Locator(".wizard-swipe-hint");

    public ILocator ActiveJugglerChip => page.Locator(".wizard-juggler-picker .wizard-chip.active");

    public ILocator BottomSheet => page.Locator(".wizard-bottom-sheet");

    public static async Task<WizardDesignPage> OpenAsync(BlazorWebassemblyFixture<Program> fixture)
    {
        var browserPage = await fixture.Context!.NewPageAsync();
        var wizardPage = await browserPage.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizardPage.WaitUntilLoadedAsync();
        return new WizardDesignPage(browserPage, wizardPage);
    }

    public async Task<string> CssVarAsync(string name) =>
        await page.Locator(".wizard-page")
            .EvaluateAsync<string>(
                "(el, n) => getComputedStyle(el).getPropertyValue(n).trim()",
                name
            );

    public async Task<string> StyleAsync(ILocator locator, string property) =>
        await locator.EvaluateAsync<string>(
            "(el, prop) => getComputedStyle(el).getPropertyValue(prop).trim()",
            property
        );

    public async Task FocusVisibleAsync(ILocator locator)
    {
        await locator.EvaluateAsync("el => el.focus({ focusVisible: true })");
    }
}
