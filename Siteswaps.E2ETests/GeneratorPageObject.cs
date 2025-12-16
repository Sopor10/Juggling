using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

public class GeneratorPageObject(IPage page)
{
    public async Task<GeneratedSiteswapsResultPageObject> GenerateSiteswapsAsync()
    {
        await page.GetByTestId("generate-button").ClickAsync();
        return new(page);
    }

    public async Task<GeneratorPageObject> SetPeriodAsync(int period)
    {
        await page.GetByTestId("numeric-input-period").FillAsync(period.ToString());
        await page.WaitForTimeoutAsync(100);
        return this;
    }

    public async Task<GeneratorPageObject> SetNumberOfJugglersAsync(int numberOfJugglers)
    {
        await page.GetByTestId("numeric-input-numberOfJugglers").FillAsync(numberOfJugglers.ToString());
        await page.WaitForTimeoutAsync(100);
        return this;
    }

    public async Task<GeneratorPageObject> SetMinClubsAsync(int minClubs)
    {
        var minInput = page.Locator("rz-numeric").First;
        await minInput.ClickAsync();
        await minInput.Locator("input").FillAsync(minClubs.ToString());
        await page.WaitForTimeoutAsync(100);
        return this;
    }

    public async Task<GeneratorPageObject> SetMaxClubsAsync(int maxClubs)
    {
        var maxInput = page.Locator("rz-numeric").Nth(1);
        await maxInput.ClickAsync();
        await maxInput.Locator("input").FillAsync(maxClubs.ToString());
        await page.WaitForTimeoutAsync(100);
        return this;
    }

    public async Task<GeneratorPageObject> AddThrowAsync(string throwName)
    {
        await page.GetByTestId($"add-throw-{throwName}").ClickAsync();
        return this;
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

    public async Task<NumberFilterDialogObject> AddNumberFilterAsync(
        Func<NumberFilterDialogObject, Task>? configure = null
    )
    {
        await page.GetByTestId("filter-node-AND").ClickAsync();
        await page.GetByText("Add Filter").ClickAsync();
        await page.GetByText("Number Filter").ClickAsync();

        var numberFilterDialogObject = new NumberFilterDialogObject(page);
        if (configure != null)
        {
            await configure(numberFilterDialogObject);
        }

        return numberFilterDialogObject;
    }

    public async Task<StateFilterDialogObject> AddStateFilterAsync(
        Func<StateFilterDialogObject, Task>? configure = null
    )
    {
        await page.GetByTestId("filter-node-AND").ClickAsync();
        await page.GetByText("Add Filter").ClickAsync();
        await page.GetByText("State Filter").ClickAsync();

        var stateFilterDialogObject = new StateFilterDialogObject(page);
        if (configure != null)
        {
            await configure(stateFilterDialogObject);
        }

        return stateFilterDialogObject;
    }
}
