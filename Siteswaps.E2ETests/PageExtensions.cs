using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

public static class PageExtensions
{
    public static async Task<GeneratorPageObject> OpenGeneratorAsync(this IPage page, string uri)
    {
        await page.GotoAsync(uri);
        return new(page);
    }
}
