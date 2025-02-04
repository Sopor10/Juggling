using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

public class GeneratorPageObject(IPage page)
{
    public async Task<GeneratedSiteswapsResultPageObject> GenerateSiteswapsAsync()
    {
        await page.GetByTestId("generate-button").ClickAsync();
        return new(page);
    }

    public async Task<PatternFilterDialogObject> AddPatternFilterAsync(
        Func<PatternFilterDialogObject, Task>? configure = null
    )
    {
        await page.GetByTestId("filter-node-AND").ClickAsync();
        await page.GetByText("Add Filter").ClickAsync();

        var patternFilterDialogObject = new PatternFilterDialogObject(page);
        if (configure != null)
        {
            await configure(patternFilterDialogObject);
        }

        return patternFilterDialogObject;
    }
}
