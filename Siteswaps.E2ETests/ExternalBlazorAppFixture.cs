using Microsoft.Playwright;
using Xunit;

namespace Siteswaps.E2ETests;

/// <summary>
/// Playwright fixture that targets an already-running Blazor host (e.g. Aspire at http://localhost:7021).
/// </summary>
public sealed class ExternalBlazorAppFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;

    public IBrowser Browser { get; private set; } = null!;

    public IBrowserContext Context { get; private set; } = null!;

    public Uri BaseUri { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var configured =
            Environment.GetEnvironmentVariable(E2EBaseUrl.EnvironmentVariableName)
            ?? E2EBaseUrl.AspireDefault;
        BaseUri = E2EBaseUrl.EnsureTrailingSlash(new Uri(configured.Trim()));

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true }
        );
        Context = await Browser.NewContextAsync();
    }

    public async Task DisposeAsync()
    {
        if (Context is not null)
        {
            await Context.DisposeAsync();
        }

        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        Playwright?.Dispose();
    }
}
