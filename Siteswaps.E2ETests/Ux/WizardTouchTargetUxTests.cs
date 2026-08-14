using FluentAssertions;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Ux;


/// <summary>Encodes touch-target UX contracts for the mobile-first wizard.</summary>
[Collection(WizardE2ECollection.Name)]
public class WizardTouchTargetUxTests(BlazorWebassemblyFixture<Program> fixture)
{
    /// <summary>Summary: Period +/- stepper controls must offer at least 40x40px touch targets.</summary>
    [Fact]
    public async Task Period_Stepper_Buttons_Meet_Minimum_Touch_Target()
    {
        var page = await fixture.Context!.NewPageAsync();
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();

        var count = await wizard.PeriodStepperButtons.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(2);
        for (var i = 0; i < count; i++)
        {
            await WizardUxGeometry.AssertMinTouchTargetAsync(
                wizard.PeriodStepperButtons.Nth(i),
                $"period stepper button[{i}]"
            );
        }
    }

    /// <summary>Summary: Juggler quick-pick chips must remain comfortably tappable on mobile.</summary>
    [Fact]
    public async Task Juggler_Chips_Meet_Minimum_Touch_Target()
    {
        var page = await fixture.Context!.NewPageAsync();
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();

        var count = await wizard.JugglerChips.CountAsync();
        count.Should().BeGreaterThan(0);
        for (var i = 0; i < count; i++)
        {
            await WizardUxGeometry.AssertMinTouchTargetAsync(
                wizard.JugglerChips.Nth(i),
                $"juggler chip[{i}]"
            );
        }
    }

    /// <summary>Summary: Dual-range clubs thumbs must be at least 40x40px, not track-thin hit areas.</summary>
    [Fact]
    public async Task Clubs_DualRange_Thumbs_Meet_Minimum_Touch_Target()
    {
        var page = await fixture.Context!.NewPageAsync();
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepTitleAsync("Keulen & Würfe");

        await WizardUxGeometry.AssertMinTouchTargetAsync(
            wizard.DualRangeTrackWrap,
            "clubs dual-range track wrap"
        );
        await WizardUxGeometry.AssertWebkitThumbMinSizeAsync(page, ".wizard-dualrange-input");
    }

    /// <summary>Summary: Throw-height chips on step 2 must stay above the sticky bottom nav when scrolled into view.</summary>
    [Fact]
    public async Task Throw_Chips_Are_Not_Covered_By_Sticky_Nav()
    {
        var page = await fixture.Context!.NewPageAsync();
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepTitleAsync("Keulen & Würfe");

        var lastChip = wizard.ThrowChips.Last;
        await lastChip.ScrollIntoViewIfNeededAsync();
        var covered = await WizardUxGeometry.StickyNavCoversElementAsync(
            page,
            ".wizard-throws [aria-label='Erlaubte Würfe'] .wizard-chip:last-child"
        );
        covered.Should().BeFalse("last throw chip must remain clear of sticky wizard nav");
    }
}
