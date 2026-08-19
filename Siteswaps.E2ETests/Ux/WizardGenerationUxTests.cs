using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Ux;

/// <summary>Encodes generation feedback, empty states, and silent-clamp UX contracts.</summary>
public class WizardGenerationUxTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Generate must leave editing chrome and finish on the results view.</summary>
    [Fact]
    public async Task Generate_Reaches_Results_Without_Repeat_Generate_Cta()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();

        await wizard.ClickGenerateAsync();
        await wizard.WaitForResultsAsync();

        await Assertions.Expect(wizard.ResultsActions).ToBeVisibleAsync();
        await Assertions.Expect(wizard.GenerateButton).ToHaveCountAsync(0);
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
        await Assertions.Expect(wizard.PeriodInput).ToHaveValueAsync("30");
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
        await Assertions.Expect(wizard.JugglerExactInput).ToHaveValueAsync("20");
        await Assertions
            .Expect(wizard.ValueClampFeedback.First)
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 5_000 });
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

        await Assertions.Expect(wizard.ResultsTitle).ToContainTextAsync("0");
        await Assertions.Expect(wizard.ResultsEmptyMessage).ToBeVisibleAsync();
        await Assertions
            .Expect(wizard.ResultsEmptyMessage)
            .ToContainTextAsync("Keine passenden Muster");
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

        await Assertions.Expect(wizard.SiteswapCards).Not.ToHaveCountAsync(0);
        await wizard.SiteswapCards.Last.ScrollIntoViewIfNeededAsync();
        await wizard.ResultsActions.ScrollIntoViewIfNeededAsync();
        var covers = await WizardUxGeometry.ResultsActionsCoverLastCardAsync(page);
        covers.Should().BeFalse("results actions must sit below the last card, not overlap it");
    }

    /// <summary>Summary: Dense mode hides juggler sequence preview on result cards.</summary>
    [Fact]
    public async Task Dense_Mode_Hides_Juggler_Preview()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToGenerateAsync();
        await wizard.WaitForResultsAsync();

        await Assertions.Expect(wizard.SiteswapCardJugglers).Not.ToHaveCountAsync(0);
        await wizard.DenseModeToggle.ClickAsync();
        await Assertions.Expect(wizard.SiteswapCardJugglers).ToHaveCountAsync(0);
        await Assertions
            .Expect(wizard.DenseModeToggle)
            .ToHaveAttributeAsync("aria-pressed", "true");
        await wizard.DenseModeToggle.ClickAsync();
        await Assertions.Expect(wizard.SiteswapCardJugglers).Not.ToHaveCountAsync(0);
        await Assertions
            .Expect(wizard.DenseModeToggle)
            .ToHaveAttributeAsync("aria-pressed", "false");
    }
}
