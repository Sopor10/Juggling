using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Playwright;

namespace Siteswaps.Design.Tests.Infrastructure;

/// <summary>
/// Hosts Playwright Chromium inside a version-pinned Docker image and connects via CDP websocket.
/// </summary>
public sealed class PlaywrightDockerFixture
{
    private const int ContainerPort = 3000;

    private IContainer? _container;
    private IPlaywright? _playwright;

    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var version =
            typeof(Playwright)
                .Assembly.CustomAttributes.Single(x =>
                    x.AttributeType == typeof(AssemblyFileVersionAttribute)
                )
                .ConstructorArguments.First()
                .Value?.ToString()
            ?? string.Empty;

        var image = $"mcr.microsoft.com/playwright:v{version}-jammy";

        _container = new ContainerBuilder(image)
            .WithEntrypoint("/bin/sh")
            .WithCommand(
                "-c",
                $"cd /home/pwuser && npx -y playwright@v{version} run-server --port {ContainerPort} --host 0.0.0.0"
            )
            .WithPortBinding(ContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Listening on"))
            .Build();

        await _container.StartAsync();

        _playwright = await Playwright.CreateAsync();
        var mappedPort = _container.GetMappedPublicPort(ContainerPort);
        Browser = await _playwright.Chromium.ConnectAsync(
            $"ws://127.0.0.1:{mappedPort}/",
            new BrowserTypeConnectOptions { Timeout = 3 * 60 * 1000, ExposeNetwork = "<loopback>" }
        );
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        _playwright?.Dispose();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
