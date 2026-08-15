using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Ux;

/// <summary>Encodes navigation, history, and step-clarity UX contracts for the wizard at /.</summary>
public class WizardNavigationUxTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    private static readonly Regex WizardRootPath = new(@"/(wizard)?/?$", RegexOptions.IgnoreCase);

    /// <summary>Summary: Double-tapping Weiter must advance only one step, never skip to filters.</summary>
    [Fact]
    public async Task Double_Tap_Weiter_Advances_Only_One_Step()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        await wizard.NextOrGenerateButton.DblClickAsync();
        await wizard.ExpectStepAsync(1);
        await Assertions
            .Expect(page.Locator("#wizard-step-tab-2"))
            .ToHaveAttributeAsync("aria-selected", "false");
    }

    /// <summary>Summary: Browser back from step 2 must stay on the wizard root on step 1, not leave the flow.</summary>
    [Fact]
    public async Task Browser_Back_From_Step2_Stays_On_Wizard_Step1()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);

        await page.GoBackAsync();
        await Assertions.Expect(wizard.Root).ToBeVisibleAsync();
        await Assertions.Expect(page).ToHaveURLAsync(WizardRootPath);
        await wizard.ExpectStepAsync(0);
    }

    /// <summary>Summary: Browser back from results must restore the filter step, not an empty broken page.</summary>
    [Fact]
    public async Task Browser_Back_From_Results_Restores_Filter_Step()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();
        await wizard.AdvanceToGenerateAsync();
        await wizard.WaitForResultsAsync();

        await page.GoBackAsync();
        await Assertions.Expect(wizard.Root).ToBeVisibleAsync();
        await Assertions.Expect(page).ToHaveURLAsync(WizardRootPath);
        await Assertions.Expect(wizard.ActiveProgressDot).ToBeVisibleAsync();
        await wizard.ExpectStepAsync(2);
        await Assertions.Expect(wizard.Results).ToBeHiddenAsync();
    }

    /// <summary>Summary: Progress dots must expose visited vs current state and block jumping to unvisited steps.</summary>
    [Fact]
    public async Task Progress_Dots_Block_Unvisited_Steps_And_Mark_Current()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var page = session.Page;
        await WizardUxGeometry.EnsureMobileViewportAsync(page);
        var wizard = await page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        var step0 = page.Locator("#wizard-step-tab-0");
        var step1 = page.Locator("#wizard-step-tab-1");
        var step2 = page.Locator("#wizard-step-tab-2");

        await Assertions.Expect(step0).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(step1).ToBeDisabledAsync();
        await Assertions.Expect(step2).ToBeDisabledAsync();

        await wizard.ClickNextAsync();
        await wizard.ExpectStepAsync(1);

        await Assertions.Expect(step1).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(step0).ToBeEnabledAsync();
        await Assertions.Expect(step2).ToBeDisabledAsync();
    }
}
