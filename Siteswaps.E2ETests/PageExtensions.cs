using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

/// <summary>Shared Playwright navigation helpers for E2E page objects.</summary>
public static class PageExtensions
{
    public static async Task<WizardPageObject> OpenWizardAsync(this IPage page, Uri baseUri)
    {
        var wizardUri = new Uri(baseUri, "wizard");
        await page.GotoAsync(
            wizardUri.ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }
        );
        return new WizardPageObject(page);
    }
}
