using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Ux;

/// <summary>Encodes navigation, history, and step-clarity UX contracts for /wizard.</summary>
[Collection(WizardE2ECollection.Name)]
public class WizardNavigationUxTests(BlazorWebassemblyFixture<Program> fixture)
{
    /// <summary>Summary: Double-tapping Weiter must advance only one step, never skip to filters.</summary>
    [Fact]
    public async Task Double_Tap_Weiter_Advances_Only_One_Step()
    {
        var page = await fixture.Context!.NewPageAsync();
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();

        await wizard.NextOrGenerateButton.DblClickAsync();
        await wizard.ExpectStepTitleAsync("Keulen & Würfe");
        (await wizard.ActiveStepTitle.First.InnerTextAsync()).Trim().Should().NotBe("Noch Filter?");
    }

    /// <summary>Summary: Browser back from step 2 must stay inside /wizard on step 1, not leave the flow.</summary>
    [Fact]
    public async Task Browser_Back_From_Step2_Stays_On_Wizard_Step1()
    {
        var page = await fixture.Context!.NewPageAsync();
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepTitleAsync("Keulen & Würfe");

        await page.GoBackAsync();
        await wizard.Root.WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15_000 }
        );
        page.Url.Should().Contain("/wizard");
        await wizard.ExpectStepTitleAsync("Jongleure & Periode");
    }

    /// <summary>Summary: Browser back from results must restore the filter step, not an empty broken page.</summary>
    [Fact]
    public async Task Browser_Back_From_Results_Restores_Filter_Step()
    {
        var page = await fixture.Context!.NewPageAsync();
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToGenerateAsync();
        await wizard.WaitForResultsAsync();

        await page.GoBackAsync();
        await wizard.Root.WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15_000 }
        );
        page.Url.Should().Contain("/wizard");
        (await wizard.IsLoadedAsync()).Should().BeTrue();
        await wizard.ExpectStepTitleAsync("Noch Filter?");
        (await wizard.Results.CountAsync()).Should().Be(0);
    }

    /// <summary>Summary: Progress dots must expose visited vs current state and block jumping to unvisited steps.</summary>
    [Fact]
    public async Task Progress_Dots_Block_Unvisited_Steps_And_Mark_Current()
    {
        var page = await fixture.Context!.NewPageAsync();
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(fixture));
        await wizard.WaitUntilLoadedAsync();

        var step0 = page.Locator("#wizard-step-tab-0");
        var step1 = page.Locator("#wizard-step-tab-1");
        var step2 = page.Locator("#wizard-step-tab-2");

        (await step0.GetAttributeAsync("aria-selected")).Should().Be("true");
        (await step1.IsDisabledAsync()).Should().BeTrue();
        (await step2.IsDisabledAsync()).Should().BeTrue();

        await wizard.ClickNextAsync();
        await wizard.ExpectStepTitleAsync("Keulen & Würfe");

        (await step1.GetAttributeAsync("aria-selected")).Should().Be("true");
        (await step0.IsDisabledAsync()).Should().BeFalse();
        (await step2.IsDisabledAsync()).Should().BeTrue();
    }
}
