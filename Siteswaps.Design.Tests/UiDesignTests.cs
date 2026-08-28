using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Playwright;
using NUnit.Framework;
using Siteswaps.Design.Tests.Infrastructure;
using Webassembly.Components.DesignTests;

namespace Siteswaps.Design.Tests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class UiDesignTests
{
    private const double DefaultThreshold = 0.0055;
    private const string FixtureRootNamespace = "Webassembly.Components.DesignTests.";
    private const string UsedForTestSelector = "[usedForTest='true']";

    private DesignTestHostFixture _host = null!;

    [ModuleInitializer]
    internal static void InitVerify()
    {
        VerifyImageMagick.Initialize();
        VerifyImageMagick.RegisterComparers(DefaultThreshold);

        if (Environment.GetEnvironmentVariable("AutoVerify") == "1")
        {
            VerifierSettings.AutoVerify();
        }
    }

    [OneTimeSetUp]
    public Task OneTimeSetUpAsync()
    {
        _host = new DesignTestHostFixture();
        return _host.InitializeAsync();
    }

    [OneTimeTearDown]
    public Task OneTimeTearDownAsync() => _host.DisposeAsync();

    public static IEnumerable<TestCaseData> GetTestCases()
    {
        var components = typeof(DesignTestComponentAttribute)
            .Assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<DesignTestComponentAttribute>() is not null)
            .OrderBy(t => t.FullName, StringComparer.Ordinal);

        foreach (var type in components)
        {
            var fullName = type.FullName!;
            var displayName = fullName
                .Replace(FixtureRootNamespace, string.Empty, StringComparison.Ordinal)
                .Replace('.', '_');
            yield return new TestCaseData(fullName, displayName).SetName(displayName);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public async Task DesignFixture_MatchesVerifiedScreenshot(string typeName, string displayName)
    {
        var context = await _host.Browser.NewContextAsync(DesignCulture.NewContextOptions());
        await DesignCulture.InstallAsync(context);

        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        var consoleErrors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (string.Equals(msg.Type, "error", StringComparison.OrdinalIgnoreCase))
            {
                consoleErrors.Add(msg.Text);
            }
        };

        try
        {
            var uri =
                $"{_host.RootUri.AbsoluteUri.TrimEnd('/')}/test?type={Uri.EscapeDataString(typeName)}";
            await page.GotoAsync(uri, new PageGotoOptions { WaitUntil = WaitUntilState.Load });

            var element = page.Locator(UsedForTestSelector);
            try
            {
                await element.WaitForAsync(
                    new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 90_000,
                    }
                );
            }
            catch (TimeoutException ex)
            {
                var html = await page.ContentAsync();
                var path = Path.Combine(
                    Path.GetTempPath(),
                    $"design-fail-{displayName}-{Guid.NewGuid():N}.html"
                );
                await File.WriteAllTextAsync(path, html);
                throw new TimeoutException(
                    $"usedForTest not visible for {displayName} at {uri}. Console errors:{Environment.NewLine}"
                        + string.Join(Environment.NewLine, consoleErrors)
                        + $"{Environment.NewLine}HTML dumped to {path}",
                    ex
                );
            }

            // Prefer fonts.ready over NetworkIdle — Blazor WASM + Google Fonts rarely go fully idle.
            await page.EvaluateAsync("() => document.fonts.ready");
            await WaitForStableBoundingBoxAsync(element);

            // Avoid accidental hover styles affecting the snapshot.
            await page.Mouse.MoveAsync(0, 0);

            var screenshotPath = Path.Combine(
                Path.GetTempPath(),
                $"{displayName}_{Guid.NewGuid():N}.png"
            );
            try
            {
                await element.ScreenshotAsync(
                    new LocatorScreenshotOptions { Path = screenshotPath }
                );
                await VerifyFile(screenshotPath).UseFileName(displayName);
            }
            finally
            {
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
            }
        }
        finally
        {
            await page.CloseAsync();
            await context.DisposeAsync();
        }
    }

    private static async Task WaitForStableBoundingBoxAsync(ILocator element)
    {
        LocatorBoundingBoxResult? previous = null;
        for (var attempt = 0; attempt < 15; attempt++)
        {
            var current = await element.BoundingBoxAsync();
            if (
                previous is not null
                && current is not null
                && Math.Abs(current.Height - previous.Height) < 0.5
                && Math.Abs(current.Width - previous.Width) < 0.5
            )
            {
                return;
            }

            previous = current;
            await Task.Delay(200);
        }
    }
}
