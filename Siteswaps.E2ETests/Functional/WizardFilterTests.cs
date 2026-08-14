using FluentAssertions;
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

    /// <summary>Summary: Pattern palette must offer don't-care ("frei") and new sequences default to all frei.</summary>
    [Fact]
    public async Task Pattern_Filter_Defaults_To_Frei_And_Exposes_DontCare_Palette()
    {
        var page = await fixture.Context!.NewPageAsync();
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();
        await wizard.OpenAddFilterSheetAsync();
        await page.Locator("#wizard-filter-tab-pattern").ClickAsync();

        var palette = page.Locator(".wizard-pattern-palette .wizard-chip");
        await Assertions.Expect(palette.Filter(new() { HasText = "frei" })).ToHaveCountAsync(1);

        var slots = page.Locator(".wizard-pattern-slots .wizard-pattern-slot");
        var slotCount = await slots.CountAsync();
        slotCount.Should().BeGreaterThan(0);
        for (var i = 0; i < slotCount; i++)
        {
            await Assertions.Expect(slots.Nth(i)).ToHaveTextAsync("frei");
        }
    }

    /// <summary>Summary: State filter must show classic x/_ notation that updates with beat toggles.</summary>
    [Fact]
    public async Task State_Filter_Shows_Occupied_Free_Notation()
    {
        var page = await fixture.Context!.NewPageAsync();
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();
        await wizard.OpenAddFilterSheetAsync();
        await page.Locator("#wizard-filter-tab-state").ClickAsync();

        var notation = page.Locator(".wizard-state-notation");
        await Assertions.Expect(notation).ToBeVisibleAsync();
        var initial = (await notation.InnerTextAsync()).Trim();
        initial.Replace(" ", "").Should().MatchRegex("^_+$");

        var firstBeat = page.Locator(".wizard-state-grid .wizard-chip").First;
        await firstBeat.ClickAsync();
        var after = (await notation.InnerTextAsync()).Trim();
        after.Should().StartWith("x");
        after.Should().NotBe(initial);
    }
}
