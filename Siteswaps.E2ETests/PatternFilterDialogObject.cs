using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

public class PatternFilterDialogObject(IPage page)
{
    public async Task<PatternFilterDialogObject> AddThrowAsync(string name)
    {
        await page.GetByTestId($"add-throw-{name}").ClickAsync();
        return this;
    }

    public async Task<GeneratorPageObject> AddFilterAsync()
    {
        await page.GetByTestId("add-pattern-filter").ClickAsync();
        return new(page);
    }
}
