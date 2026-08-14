using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

/// <summary>Filter sheet and list contracts for wizard step 3 (clamp feedback lives in Ux).</summary>
[Collection(WizardE2ECollection.Name)]
public class WizardFilterTests(BlazorWebassemblyFixture<Program> fixture)
{
    /// <summary>Summary: Saving a number filter must show it in the list and removing it must clear the list.</summary>
    [Fact]
    public async Task Number_Filter_Can_Be_Added_And_Removed()
    {
        var page = await fixture.Context!.NewPageAsync();
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();

        await wizard.SaveNumberFilterAsync("Genau", 2, "Heff");
        await Assertions.Expect(wizard.FilterCards).ToHaveCountAsync(1);
        await Assertions
            .Expect(wizard.FilterCards.First.Locator(".wizard-filter-desc"))
            .ToContainTextAsync("Genau 2× Heff");

        await wizard.RemoveFirstFilterAsync();
        await Assertions.Expect(wizard.FilterCards).ToHaveCountAsync(0);
        await Assertions.Expect(wizard.FiltersEmptyMessage).ToBeVisibleAsync();
    }
}
