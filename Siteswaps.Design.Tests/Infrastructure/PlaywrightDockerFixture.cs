using System.Diagnostics;
using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Playwright;

namespace Siteswaps.Design.Tests.Infrastructure;

/// <summary>
/// Hosts Playwright Chromium inside a version-pinned Docker image and connects via CDP websocket.
/// Prefer <c>PLAYWRIGHT_WS</c> when set; otherwise start (or reuse) a Testcontainers Playwright server.
/// </summary>
public sealed class PlaywrightDockerFixture
{
    private const int ContainerPort = 3000;
    private const string ReuseLabelKey = "juggling.design.playwright";
    private const string ReuseLabelValue = "true";

    private IContainer? _container;
    private IPlaywright? _playwright;
    private bool _reuseContainer;

    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var timing = Timing.Enabled;
        var total = timing ? Stopwatch.StartNew() : null;

        var wsUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_WS");
        if (!string.IsNullOrWhiteSpace(wsUrl))
        {
            await ConnectToEndpointAsync(wsUrl.Trim()).ConfigureAwait(false);
            Timing.Log(total, "playwright.total");
            return;
        }

        var sw = timing ? Stopwatch.StartNew() : null;
        var version =
            typeof(Playwright)
                .Assembly.CustomAttributes.Single(x =>
                    x.AttributeType == typeof(AssemblyFileVersionAttribute)
                )
                .ConstructorArguments.First()
                .Value?.ToString()
            ?? string.Empty;
        Timing.Log(sw, "playwright.resolve-version");

        var image = $"mcr.microsoft.com/playwright:v{version}-jammy";

        sw?.Restart();
        _reuseContainer = true;
        _container = new ContainerBuilder(image)
            .WithEntrypoint("/bin/sh")
            .WithCommand(
                "-c",
                $"cd /home/pwuser && npx -y playwright@v{version} run-server --port {ContainerPort} --host 0.0.0.0"
            )
            .WithPortBinding(ContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Listening on"))
            .WithReuse(true)
            .WithLabel(ReuseLabelKey, ReuseLabelValue)
            .Build();
        Timing.Log(sw, "playwright.container-build");

        sw?.Restart();
        await _container.StartAsync().ConfigureAwait(false);
        Timing.Log(sw, "playwright.container-start+wait-listening");

        var mappedPort = _container.GetMappedPublicPort(ContainerPort);
        await ConnectToEndpointAsync($"ws://127.0.0.1:{mappedPort}/").ConfigureAwait(false);
        Timing.Log(total, "playwright.total");
    }

    private async Task ConnectToEndpointAsync(string wsEndpoint)
    {
        var timing = Timing.Enabled;
        var sw = timing ? Stopwatch.StartNew() : null;

        _playwright = await Playwright.CreateAsync().ConfigureAwait(false);
        Timing.Log(sw, "playwright.create-api");

        sw?.Restart();
        Browser = await _playwright
            .Chromium.ConnectAsync(
                wsEndpoint,
                new BrowserTypeConnectOptions
                {
                    Timeout = 3 * 60 * 1000,
                    ExposeNetwork = "<loopback>",
                }
            )
            .ConfigureAwait(false);
        Timing.Log(sw, "playwright.connect-chromium");
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync().ConfigureAwait(false);
        }

        _playwright?.Dispose();

        // Skip Dispose on reused containers so the next run keeps the same host port.
        if (_container is not null && !_reuseContainer)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
