using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.Design.Tests.Infrastructure;

/// <summary>
/// Blazor WASM host on real Kestrel (so Docker Chromium can navigate without RouteAsync proxy)
/// plus Playwright connected to a version-pinned Docker browser.
/// </summary>
public sealed class DesignTestHostFixture
{
    private readonly PlaywrightDockerFixture _playwright = new();
    private WebApplicationFactory<Program>? _webApplicationFactory;

    public Uri RootUri { get; private set; } = null!;

    public IBrowser Browser => _playwright.Browser;

    public async Task InitializeAsync()
    {
        await _playwright.InitializeAsync();

        var factory = new WebApplicationFactory<Program>();
        // .NET 10+: bind a real loopback port so remote Chromium can load the WASM host.
        factory.UseKestrel(0);
        factory.StartServer();

        using var probe = factory.CreateClient();
        RootUri =
            probe.BaseAddress
            ?? throw new InvalidOperationException(
                "Kestrel WebApplicationFactory has no BaseAddress."
            );

        _webApplicationFactory = factory;
    }

    public async Task DisposeAsync()
    {
        if (_webApplicationFactory is not null)
        {
            await _webApplicationFactory.DisposeAsync();
        }

        await _playwright.DisposeAsync();
    }
}
