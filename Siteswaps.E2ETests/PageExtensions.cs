using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

/// <summary>Shared Playwright navigation helpers for E2E page objects.</summary>
public static class PageExtensions
{
    public static async Task<WizardPageObject> OpenWizardAsync(this IPage page, Uri baseUri)
    {
        await E2ECulture.InstallAsync(page.Context);
        // Blazor WASM keeps connections open; NetworkIdle is flaky especially under parallel load.
        await page.GotoAsync(
            baseUri.ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.Load }
        );
        return new WizardPageObject(page);
    }
}
