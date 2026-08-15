using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

/// <summary>Generation contracts for the wizard flow with stock and constrained inputs.</summary>
public class WizardGenerationTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Stock jugglers/period/clubs/throws must produce at least one siteswap.</summary>
    [Fact]
    public async Task Default_Params_Generate_Produces_Results()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        await wizard.AdvanceToGenerateAsync();
        await wizard.WaitForResultsAsync();

        await Assertions.Expect(wizard.SiteswapCards).Not.ToHaveCountAsync(0);
        await Assertions.Expect(wizard.ResultsTitle).ToHaveTextAsync(new Regex(@"^[1-9]"));
    }

    /// <summary>Summary: An impossible number filter must yield zero results for otherwise valid defaults.</summary>
    [Fact]
    public async Task Impossible_Number_Filter_Yields_Zero_Results()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();

        await wizard.SaveNumberFilterAsync("Genau", 30, "Heff");
        await wizard.ClickGenerateAsync();
        await wizard.WaitForResultsAsync();

        await Assertions.Expect(wizard.ResultsTitle).ToContainTextAsync("0");
        await Assertions.Expect(wizard.ResultsEmptyMessage).ToBeVisibleAsync();
    }

    /// <summary>Summary: Result cards must report club counts inside the selected clubs range.</summary>
    [Fact]
    public async Task Clubs_Range_Is_Reflected_In_Result_Cards()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);
        await wizard.SetClubsRangeAsync(6, 6);
        await Assertions.Expect(wizard.ClubsEcho).ToContainTextAsync("6");
        await wizard.ClickNextAsync();
        await wizard.ClickGenerateAsync();
        await wizard.WaitForResultsAsync();

        var clubValues = wizard.Page.Locator(".pz-siteswap-card-clubs-value");
        await Assertions.Expect(clubValues).Not.ToHaveCountAsync(0);
        await Assertions
            .Expect(wizard.Page.Locator(".pz-siteswap-card-clubs-value:not(:text-is(\"6\"))"))
            .ToHaveCountAsync(0);
    }
}
