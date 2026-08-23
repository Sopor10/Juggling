using Microsoft.Playwright;
using Xunit;

namespace PlaywrightTesting.Infrastructure;

/// <summary>
/// Local Playwright browser host compiled against the repo's Microsoft.Playwright version.
/// Replaces <c>Sopor10.Playwright.Testing</c> which was built against an older RouteAsync shape.
/// </summary>
public sealed class PlaywrightFixture : IAsyncLifetime
{
    public IBrowser Browser { get; private set; } = null!;

    private IPlaywright PlaywrightInstance { get; set; } = null!;

    public async Task InitializeAsync()
    {
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true }
        );
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        PlaywrightInstance.Dispose();
    }
}
