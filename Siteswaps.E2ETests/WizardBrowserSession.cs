using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests;

/// <summary>
/// Isolated Playwright browser context + page for a single E2E test.
/// </summary>
public sealed class WizardBrowserSession : IAsyncDisposable
{
    private WizardBrowserSession(IBrowserContext context, IPage page)
    {
        Context = context;
        Page = page;
    }

    public IBrowserContext Context { get; }

    public IPage Page { get; }

    public static async Task<WizardBrowserSession> CreateAsync(
        BlazorWebassemblyFixture<Program> fixture
    )
    {
        var browser =
            fixture.Context?.Browser
            ?? throw new InvalidOperationException("Playwright browser is not available.");
        var context = await browser.NewContextAsync(E2ECulture.NewContextOptions());
        await BlazorTestServerProxy.InstallAsync(context, fixture);
        await E2ECulture.InstallAsync(context);
        var page = await context.NewPageAsync();
        return new WizardBrowserSession(context, page);
    }

    public ValueTask DisposeAsync() => Context.DisposeAsync();
}
