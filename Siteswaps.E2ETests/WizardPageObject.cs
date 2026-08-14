using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

/// <summary>Page object for the /wizard Blazor route (<c>.wizard-page</c> UI).</summary>
public class WizardPageObject(IPage page)
{
    public IPage Page => page;

    public ILocator Root => page.Locator(".wizard-page");

    public ILocator ActiveStepTitle => page.Locator(".wizard-step.active .wizard-step-title");

    public ILocator NextOrGenerateButton =>
        page.Locator(".wizard-nav-buttons .wizard-btn-primary, .wizard-nav-buttons .wizard-btn-generate");

    public ILocator BackButton => page.Locator(".wizard-back-btn");

    public ILocator AddFilterButton => page.Locator(".wizard-add-filter-btn");

    public ILocator Results => page.Locator(".wizard-results");

    public ILocator ResultsTitle => page.Locator(".wizard-results-title");

    public ILocator ResultsEmptyMessage => page.Locator(".wizard-results .wizard-filters-empty");

    public ILocator SiteswapCards => page.Locator(".pz-siteswap-card");

    public ILocator PeriodInput => page.Locator("#periodExactInput");

    public ILocator JugglerExactInput => page.Locator("#jugglerExactInput");

    public ILocator FilterSheet => page.Locator(".wizard-bottom-sheet.open");

    public ILocator FilterCards => page.Locator(".wizard-filter-card");

    public ILocator FiltersEmptyMessage => page.Locator(".wizard-filters-empty");

    public ILocator ThrowChips =>
        page.Locator(".wizard-throws [aria-label='Erlaubte Würfe'] .wizard-chip");

    public ILocator ClubsEcho => page.Locator(".wizard-dualrange-echo");

    public ILocator ValidationAlert => page.Locator("[role='alert']");

    public ILocator PeriodStepperButtons => page.Locator(".wizard-stepper-btn");

    public ILocator JugglerChips => page.Locator(".wizard-juggler-picker .wizard-chip");

    public ILocator DualRangeTrackWrap => page.Locator(".wizard-dualrange-track-wrap");

    public ILocator DualRangeInputs => page.Locator(".wizard-dualrange-input");

    public ILocator StickyNav => page.Locator(".wizard-nav");

    public ILocator ProgressDotButtons => page.Locator(".wizard-dots .wizard-dot");

    public ILocator GeneratingSpinner => page.Locator(".wizard-spinner");

    public ILocator ResultsActions => page.Locator(".wizard-results-actions");

    public ILocator FilterSheetBackdrop => page.Locator(".wizard-sheet-backdrop.open");

    public ILocator CancelGenerationButton =>
        page.Locator(".wizard-results-header button")
            .Filter(new LocatorFilterOptions { HasText = "Abbrechen" });

    public ILocator ValueClampFeedback =>
        page.Locator(
            ".wizard-period [role='status'], .wizard-period [role='alert'], .wizard-juggler-picker [role='status'], .wizard-juggler-picker [role='alert'], .wizard-clamp-feedback, .wizard-validation"
        );

    public async Task WaitUntilLoadedAsync(float timeoutMs = 60_000)
    {
        await Root.WaitForAsync(
            new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs,
            }
        );
        await ActiveStepTitle.First.WaitForAsync(
            new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs,
            }
        );
    }

    public async Task<bool> IsLoadedAsync()
    {
        return await Root.IsVisibleAsync() && await ActiveStepTitle.First.IsVisibleAsync();
    }

    public async Task ClickNextAsync()
    {
        await NextOrGenerateButton.ClickAsync();
    }

    public async Task ClickBackAsync()
    {
        await BackButton.ClickAsync();
    }

    public async Task ClickGenerateAsync()
    {
        await page.Locator(".wizard-btn-generate").ClickAsync();
    }

    public async Task AdvanceToFiltersAsync()
    {
        await ClickNextAsync();
        await ExpectStepTitleAsync("Keulen & Würfe");
        await ClickNextAsync();
        await ExpectStepTitleAsync("Noch Filter?");
    }

    public async Task AdvanceToGenerateAsync()
    {
        await AdvanceToFiltersAsync();
        await ClickGenerateAsync();
    }

    public async Task WaitForResultsAsync(float timeoutMs = 120_000)
    {
        await Results.WaitForAsync(
            new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs,
            }
        );
        await page.Locator(".wizard-results-title")
            .Filter(new LocatorFilterOptions { HasNotText = "Generiere" })
            .WaitForAsync(
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = timeoutMs,
                }
            );
    }

    public async Task ExpectStepTitleAsync(string title)
    {
        await Assertions
            .Expect(ActiveStepTitle.First)
            .ToHaveTextAsync(title, new LocatorAssertionsToHaveTextOptions { Timeout = 15_000 });
    }

    public async Task OpenAddFilterSheetAsync()
    {
        await AddFilterButton.ClickAsync();
        await FilterSheet.WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible }
        );
    }

    public async Task JumpToStepAsync(int stepIndex)
    {
        await page.Locator($"#wizard-step-tab-{stepIndex}").ClickAsync();
    }

    public async Task SetPeriodAsync(int period)
    {
        await PeriodInput.FillAsync(period.ToString());
        await PeriodInput.PressAsync("Tab");
    }

    public async Task SetExactJugglersAsync(int jugglers)
    {
        await JugglerExactInput.FillAsync(jugglers.ToString());
        await JugglerExactInput.PressAsync("Tab");
    }

    public async Task SelectJugglerChipAsync(int jugglers)
    {
        await page.Locator(".wizard-juggler-picker .wizard-chip")
            .Filter(new LocatorFilterOptions { HasText = jugglers.ToString() })
            .ClickAsync();
    }

    public async Task SetClubsRangeAsync(int min, int max)
    {
        await page.Locator("input[aria-label='Keulen Minimum']").FillAsync(min.ToString());
        await page.Locator("input[aria-label='Keulen Maximum']").FillAsync(max.ToString());
    }

    public async Task DeselectAllThrowsAsync()
    {
        var chips = ThrowChips;
        var count = await chips.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var chip = chips.Nth(i);
            if (await chip.GetAttributeAsync("aria-pressed") == "true")
            {
                await chip.ClickAsync();
            }
        }
    }

    public async Task ToggleThrowChipAsync(string displayName)
    {
        await ThrowChips
            .Filter(new LocatorFilterOptions { HasText = displayName })
            .ClickAsync();
    }

    public async Task SaveDefaultNumberFilterAsync()
    {
        await OpenAddFilterSheetAsync();
        await page.Locator(".wizard-btn-filter-primary").ClickAsync();
        await FilterSheet.WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Hidden }
        );
    }

    public async Task SaveNumberFilterAsync(string comparison, int amount, string throwName)
    {
        await OpenAddFilterSheetAsync();
        await SelectWizardOptionAsync("numComparison", comparison);
        await page.Locator("#numAmount").FillAsync(amount.ToString());
        await SelectWizardOptionAsync("numThrow", throwName);
        await page.Locator(".wizard-btn-filter-primary").ClickAsync();
        await FilterSheet.WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Hidden }
        );
    }

    private async Task SelectWizardOptionAsync(string selectId, string label)
    {
        await page.Locator($"#{selectId}-trigger").ClickAsync();
        await page.Locator($"#{selectId}-listbox [role='option']")
            .Filter(new LocatorFilterOptions { HasText = label })
            .ClickAsync();
    }

    public async Task RemoveFirstFilterAsync()
    {
        await FilterCards.First.Locator(".wizard-icon-btn-danger").ClickAsync();
    }

    public async Task<string> GetResultsTitleTextAsync()
    {
        return (await ResultsTitle.InnerTextAsync()).Trim();
    }

    public async Task<int> ParseResultCountAsync()
    {
        var text = await GetResultsTitleTextAsync();
        var digits = new string(text.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var count) ? count : -1;
    }
}
