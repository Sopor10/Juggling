using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

public class StateFilterDialogObject(IPage page)
{
    public async Task<StateFilterDialogObject> ToggleStateAtHeightAsync(int height)
    {
        var checkboxes = page.Locator("rz-checkbox");
        var checkbox = checkboxes.Nth(height);
        await checkbox.ClickAsync();
        return this;
    }

    public async Task<GeneratorPageObject> AddFilterAsync()
    {
        var addButton = page.Locator("rz-button").Filter(new() { HasText = "Add Filter" }).Last;
        await addButton.ClickAsync();
        return new(page);
    }
}
