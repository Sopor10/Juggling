using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

/// <summary>PWA install CTA is production-only; localhost / E2E host must not show it.</summary>
public sealed class PwaInstallTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    [Fact]
    public async Task Install_Button_Is_Hidden_Off_Production_Host()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var wizard = await session.Page.OpenWizardAsync(E2EBaseUrl.FromFixture(host.Fixture));
        await wizard.WaitUntilLoadedAsync();

        await Assertions
            .Expect(session.Page.GetByTestId("pwa-install-desktop"))
            .ToHaveCountAsync(0);

        var canPrompt = await session.Page.EvaluateAsync<bool>(
            "() => window.pwaInstall.canPrompt()"
        );
        canPrompt.Should().BeFalse();
    }
}
