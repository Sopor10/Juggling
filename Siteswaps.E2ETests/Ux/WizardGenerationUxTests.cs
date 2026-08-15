using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Ux;

/// <summary>Encodes generation feedback, empty states, and silent-clamp UX contracts.</summary>
public class WizardGenerationUxTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Generate must show loading feedback immediately and disable repeat starts.</summary>
    [Fact]
    public async Task Generate_Shows_Loading_Feedback_And_Disables_Repeat()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();

        await wizard.ClickGenerateAsync();
        await wizard.GeneratingSpinner.WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5_000 }
        );
        (await wizard.ResultsTitle.InnerTextAsync()).Should().Contain("Generiere");
        (await page.Locator(".wizard-btn-generate").CountAsync())
            .Should()
            .Be(0, because: "generate CTA must leave the editing chrome while generating");
    }

    /// <summary>Summary: Extreme period values must clamp visibly with user feedback, not silently.</summary>
    [Fact]
    public async Task Extreme_Period_Input_Provides_Clamp_Feedback()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        await wizard.SetPeriodAsync(999);
        (await wizard.PeriodInput.InputValueAsync()).Should().Be("30");
        (await wizard.ValueClampFeedback.CountAsync())
            .Should()
            .BeGreaterThan(
                0,
                because: "clamping period to the max must surface status/alert feedback"
            );
        await Assertions
            .Expect(wizard.ValueClampFeedback.First)
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 5_000 });
    }

    /// <summary>Summary: Extreme juggler exact input must clamp with visible feedback, not a silent rewrite.</summary>
    [Fact]
    public async Task Extreme_Juggler_Input_Provides_Clamp_Feedback()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        await wizard.SetExactJugglersAsync(999);
        (await wizard.JugglerExactInput.InputValueAsync()).Should().Be("20");
        (await wizard.ValueClampFeedback.CountAsync())
            .Should()
            .BeGreaterThan(
                0,
                because: "clamping jugglers to the max must surface status/alert feedback"
            );
    }

    /// <summary>Summary: Zero-result runs must show a clear empty state, not a blank results shell.</summary>
    [Fact]
    public async Task Zero_Results_Shows_Helpful_Empty_State()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);
        await wizard.DeselectAllThrowsAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(2);
        await wizard.ClickGenerateAsync();
        await wizard.WaitForResultsAsync();

        (await wizard.ParseResultCountAsync()).Should().Be(0);
        await Assertions.Expect(wizard.ResultsEmptyMessage).ToBeVisibleAsync();
        (await wizard.ResultsEmptyMessage.InnerTextAsync())
            .Should()
            .Contain("Keine passenden Muster");
    }

    /// <summary>Summary: Results chrome must not cover the last siteswap card after scrolling to the end.</summary>
    [Fact]
    public async Task Results_Footer_Does_Not_Cover_Last_Card()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToGenerateAsync();
        await wizard.WaitForResultsAsync();

        var cardCount = await wizard.SiteswapCards.CountAsync();
        if (cardCount == 0)
        {
            await Assertions.Expect(wizard.ResultsEmptyMessage).ToBeVisibleAsync();
            return;
        }

        await wizard.SiteswapCards.Last.ScrollIntoViewIfNeededAsync();
        await wizard.ResultsActions.ScrollIntoViewIfNeededAsync();
        var covers = await WizardUxGeometry.ResultsActionsCoverLastCardAsync(page);
        covers.Should().BeFalse("results actions must sit below the last card, not overlap it");
    }
}
