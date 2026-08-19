using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Ux;

/// <summary>Encodes touch-target UX contracts for the mobile-first wizard.</summary>
public class WizardTouchTargetUxTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Period +/- stepper controls must offer at least 40x40px touch targets.</summary>
    [Fact]
    public async Task Period_Stepper_Buttons_Meet_Minimum_Touch_Target()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        await Assertions.Expect(wizard.PeriodStepperButtons.Nth(0)).ToBeVisibleAsync();
        await Assertions.Expect(wizard.PeriodStepperButtons.Nth(1)).ToBeVisibleAsync();
        for (var i = 0; i < 2; i++)
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
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        await Assertions.Expect(wizard.JugglerChips.First).ToBeVisibleAsync();
        var count = await wizard.JugglerChips.CountAsync();
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
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);

        await WizardUxGeometry.AssertMinTouchTargetAsync(
            wizard.DualRangeTrackWrap,
            "clubs dual-range track wrap"
        );
        await WizardUxGeometry.AssertWebkitThumbMinSizeAsync(page, ".wizard-dualrange-input");
    }

    /// <summary>
    /// Summary: Default 5–7 clubs must keep both range values and paint an inclusive fill, not thumbs stuck at min.
    /// </summary>
    [Fact]
    public async Task Clubs_DualRange_Default_Is_Inclusive_Five_To_Seven()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);

        await Assertions.Expect(wizard.ClubsEcho).ToContainTextAsync("5");
        await Assertions.Expect(wizard.ClubsEcho).ToContainTextAsync("7");
        await Assertions.Expect(wizard.DualRangeInputs.Nth(0)).ToHaveValueAsync("5");
        await Assertions.Expect(wizard.DualRangeInputs.Nth(1)).ToHaveValueAsync("7");

        var geometry = await page.EvaluateAsync<double[]>(
            @"() => {
                const fill = document.querySelector('.wizard-dualrange-fill');
                const wrap = document.querySelector('.wizard-dualrange-track-wrap');
                if (!fill || !wrap) {
                    return [0, 0, 0];
                }
                const f = fill.getBoundingClientRect();
                const w = wrap.getBoundingClientRect();
                return [f.left - w.left, f.width, w.width];
            }"
        );

        geometry[2].Should().BeGreaterThan(0);
        geometry[0]
            .Should()
            .BeGreaterThan(
                geometry[2] * 0.04,
                because: "min=5 on a 2–30 inclusive track must not sit at the far left"
            );
        geometry[1]
            .Should()
            .BeGreaterThan(
                geometry[2] * 0.08,
                because: "inclusive 5–7 must cover three slots, not an exclusive [5, 7) sliver"
            );
    }

    /// <summary>Summary: Throw-height chips on step 2 must stay above the sticky bottom nav when scrolled into view.</summary>
    [Fact]
    public async Task Throw_Chips_Are_Not_Covered_By_Sticky_Nav()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);

        var lastChip = wizard.ThrowChips.Last;
        await lastChip.ScrollIntoViewIfNeededAsync();
        var covered = await WizardUxGeometry.StickyNavCoversElementAsync(
            page,
            ".wizard-throws [aria-label='Erlaubte Würfe'] .wizard-chip:last-child"
        );
        covered.Should().BeFalse("last throw chip must remain clear of sticky wizard nav");
    }
}
