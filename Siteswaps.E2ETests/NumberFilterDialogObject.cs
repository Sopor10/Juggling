using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

public class NumberFilterDialogObject(IPage page)
{
    public async Task<NumberFilterDialogObject> SetFilterTypeAsync(string type)
    {
        await page.Locator("select#numberFilterType").SelectOptionAsync(type);
        return this;
    }

    public async Task<NumberFilterDialogObject> SetAmountAsync(int amount)
    {
        await page.Locator("input#numberFilterAmount").FillAsync(amount.ToString());
        return this;
    }

    public async Task<NumberFilterDialogObject> SetThrowAsync(string throwName)
    {
        var dropdown = page.Locator("rz-dropdown").Last;
        await dropdown.ClickAsync();
        await page.GetByText(throwName, new() { Exact = true }).ClickAsync();
        return this;
    }

    public async Task<GeneratorPageObject> AddFilterAsync()
    {
        var addButton = page.Locator("rz-button").Filter(new() { HasText = "Add Filter" }).Last;
        await addButton.ClickAsync();
        return new(page);
    }
}
