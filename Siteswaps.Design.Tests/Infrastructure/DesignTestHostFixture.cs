using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.Design.Tests.Infrastructure;

/// <summary>
/// Blazor WASM host on real Kestrel (so Docker Chromium can navigate without RouteAsync proxy)
/// plus Playwright connected to a version-pinned Docker browser.
/// RootUri is immutable after <see cref="InitializeAsync"/>; concurrent page navigations are safe.
/// </summary>
public sealed class DesignTestHostFixture
{
    private readonly PlaywrightDockerFixture _playwright = new();
    private WebApplicationFactory<Program>? _webApplicationFactory;
    private BrowserContextPool? _contextPool;

    public Uri RootUri { get; private set; } = null!;

    public IBrowser Browser => _playwright.Browser;

    public BrowserContextPool ContextPool =>
        _contextPool
        ?? throw new InvalidOperationException(
            "Context pool is not ready; call InitializeAsync first."
        );

    public async Task InitializeAsync()
    {
        var timing = Timing.Enabled;
        var total = timing ? Stopwatch.StartNew() : null;

        var sw = timing ? Stopwatch.StartNew() : null;
        await _playwright.InitializeAsync();
        Timing.Log(sw, "host.playwright-init");

        _contextPool = new BrowserContextPool(Browser);

        sw?.Restart();
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
        Timing.Log(sw, "host.kestrel-webassembly-start");
        Timing.Log(total, "host.total");
        if (timing)
        {
            Console.Error.WriteLine($"[design-timing] host.rootUri={RootUri}");
        }
    }

    public async Task DisposeAsync()
    {
        if (_contextPool is not null)
        {
            await _contextPool.DisposeAsync();
        }

        if (_webApplicationFactory is not null)
        {
            await _webApplicationFactory.DisposeAsync();
        }

        await _playwright.DisposeAsync();
    }
}
