using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

/// <summary>Generation contracts for the wizard flow with stock and constrained inputs.</summary>
[Collection(WizardE2ECollection.Name)]
public class WizardGenerationTests(BlazorWebassemblyFixture<Program> fixture)
{
    /// <summary>Summary: Stock jugglers/period/clubs/throws must produce at least one siteswap.</summary>
    [Fact]
    public async Task Default_Params_Generate_Produces_Results()
    {
        var page = await fixture.Context!.NewPageAsync();
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();

        await wizard.AdvanceToGenerateAsync();
        await wizard.WaitForResultsAsync();

        var count = await wizard.ParseResultCountAsync();
        count.Should().BeGreaterThan(0);
        (await wizard.SiteswapCards.CountAsync()).Should().BeGreaterThan(0);
    }

    /// <summary>Summary: An impossible number filter must yield zero results for otherwise valid defaults.</summary>
    [Fact]
    public async Task Impossible_Number_Filter_Yields_Zero_Results()
    {
        var page = await fixture.Context!.NewPageAsync();
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToFiltersAsync();

        await wizard.SaveNumberFilterAsync("Genau", 30, "Heff");
        await wizard.ClickGenerateAsync();
        await wizard.WaitForResultsAsync();

        (await wizard.ParseResultCountAsync()).Should().Be(0);
        await Assertions.Expect(wizard.ResultsEmptyMessage).ToBeVisibleAsync();
    }

    /// <summary>Summary: Result cards must report club counts inside the selected clubs range.</summary>
    [Fact]
    public async Task Clubs_Range_Is_Reflected_In_Result_Cards()
    {
        var page = await fixture.Context!.NewPageAsync();
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();

        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);
        await wizard.SetClubsRangeAsync(6, 6);
        await Assertions.Expect(wizard.ClubsEcho).ToContainTextAsync("6");
        await wizard.ClickNextAsync();
        await wizard.ClickGenerateAsync();
        await wizard.WaitForResultsAsync();

        (await wizard.ParseResultCountAsync()).Should().BeGreaterThan(0);
        var clubValues = wizard.Page.Locator(".pz-siteswap-card-clubs-value");
        var n = await clubValues.CountAsync();
        n.Should().BeGreaterThan(0);
        for (var i = 0; i < n; i++)
        {
            (await clubValues.Nth(i).InnerTextAsync()).Trim().Should().Be("6");
        }
    }
}
