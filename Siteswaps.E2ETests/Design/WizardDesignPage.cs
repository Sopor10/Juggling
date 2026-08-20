using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Design;

/// <summary>Design-focused locators and computed-style reads for the /wizard Passing Zone UI.</summary>
public sealed class WizardDesignPage : IAsyncDisposable
{
    private readonly IBrowserContext _context;
    private readonly IPage _page;

    private WizardDesignPage(IBrowserContext context, IPage page, WizardPageObject wizard)
    {
        _context = context;
        _page = page;
        Wizard = wizard;
    }

    public WizardPageObject Wizard { get; }

    public ILocator Header => _page.Locator(".wizard-header");

    public ILocator Logo => _page.Locator(".pznav-logo");

    public ILocator StepSheet => _page.Locator(".wizard-steps");

    public ILocator DisplaySample => _page.Locator(".wizard-period-value");

    public ILocator PrimaryForwardButton =>
        _page.Locator(
            ".wizard-nav-buttons .wizard-btn-primary, .wizard-nav-buttons .wizard-btn-generate"
        );

    public ILocator GenerateButton => _page.Locator(".wizard-nav-buttons .wizard-btn-generate");

    public ILocator GhostBackButton => _page.Locator(".wizard-nav-buttons .wizard-btn-ghost");

    public ILocator ActiveProgressDot => _page.Locator(".wizard-dot.active");

    public ILocator SwipeHint => _page.Locator(".wizard-swipe-hint");

    public ILocator ActiveJugglerChip =>
        _page.Locator(".wizard-juggler-picker .wizard-chip.active");

    public ILocator BottomSheet => _page.Locator(".wizard-bottom-sheet");

    public static async Task<WizardDesignPage> OpenAsync(BlazorWebassemblyFixture<Program> fixture)
    {
        var session = await WizardBrowserSession.CreateAsync(fixture);
        var wizardPage = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizardPage.WaitUntilLoadedAsync();
        return new WizardDesignPage(session.Context, session.Page, wizardPage);
    }

    public async Task<string> CssVarAsync(string name) =>
        await _page
            .Locator(".wizard-page")
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

    public ValueTask DisposeAsync() => _context.DisposeAsync();
}
