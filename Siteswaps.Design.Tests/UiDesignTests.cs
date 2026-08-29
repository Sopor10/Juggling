using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Playwright;
using NUnit.Framework;
using Siteswaps.Design.Tests.Infrastructure;
using Webassembly.Components.DesignTests;

namespace Siteswaps.Design.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
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

        // Surface mismatch pairs in test results / CI logs for easier triage.
        VerifierSettings.OnVerifyMismatch(
            (filePair, _, _) =>
            {
                if (File.Exists(filePair.ReceivedPath))
                {
                    TestContext.AddTestAttachment(filePair.ReceivedPath);
                }

                if (File.Exists(filePair.VerifiedPath))
                {
                    TestContext.AddTestAttachment(filePair.VerifiedPath);
                }

                return Task.CompletedTask;
            }
        );
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
            var attribute = type.GetCustomAttribute<DesignTestComponentAttribute>()!;
            var fullName = type.FullName!;
            var relative = fullName
                .Replace(FixtureRootNamespace, string.Empty, StringComparison.Ordinal)
                .Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (relative.Length < 2)
            {
                throw new InvalidOperationException(
                    $"Design fixture '{fullName}' must live under at least one folder namespace "
                        + $"(e.g. {FixtureRootNamespace}Home.BrandMark)."
                );
            }

            // Tests/{Area}/{Component}/{width}.verified.png
            var directory = Path.Combine(["Tests", .. relative]);
            var fixturePath = string.Join('/', relative);

            foreach (var width in attribute.ResolveWidths())
            {
                var height = DesignTestComponentAttribute.HeightForWidth(width);
                var displayName = $"{fixturePath}/{width}";
                yield return new TestCaseData(
                    fullName,
                    directory,
                    width.ToString(CultureInfo.InvariantCulture),
                    width,
                    height
                ).SetName(displayName);
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public async Task DesignFixture_MatchesVerifiedScreenshot(
        string typeName,
        string verifyDirectory,
        string verifyFileName,
        int viewportWidth,
        int viewportHeight
    )
    {
        var timing = Timing.Enabled;
        var caseSw = timing ? System.Diagnostics.Stopwatch.StartNew() : null;
        var sw = timing ? System.Diagnostics.Stopwatch.StartNew() : null;

        var context = await _host.ContextPool.RentAsync(viewportWidth, viewportHeight);
        IPage? page = null;
        try
        {
            page = await context.NewPageAsync();
            page.SetDefaultTimeout(90_000);
            Timing.Log(sw, $"case[{verifyDirectory}/{verifyFileName}].new-context+page");

            var consoleErrors = new List<string>();
            page.Console += (_, msg) =>
            {
                if (string.Equals(msg.Type, "error", StringComparison.OrdinalIgnoreCase))
                {
                    consoleErrors.Add(msg.Text);
                }
            };

            var uri =
                $"{_host.RootUri.AbsoluteUri.TrimEnd('/')}/test?type={Uri.EscapeDataString(typeName)}";
            sw?.Restart();
            await page.GotoAsync(uri, new PageGotoOptions { WaitUntil = WaitUntilState.Load });
            Timing.Log(sw, $"case[{verifyDirectory}/{verifyFileName}].goto-load");

            var element = page.Locator(UsedForTestSelector);
            try
            {
                sw?.Restart();
                await element.WaitForAsync(
                    new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 90_000,
                    }
                );
                Timing.Log(sw, $"case[{verifyDirectory}/{verifyFileName}].wait-usedForTest");
            }
            catch (TimeoutException ex)
            {
                var html = await page.ContentAsync();
                var path = Path.Combine(
                    Path.GetTempPath(),
                    $"design-fail-{verifyFileName}-{Guid.NewGuid():N}.html"
                );
                await File.WriteAllTextAsync(path, html);
                throw new TimeoutException(
                    $"usedForTest not visible for {typeName} @{viewportWidth}x{viewportHeight} at {uri}. "
                        + $"Console errors:{Environment.NewLine}"
                        + string.Join(Environment.NewLine, consoleErrors)
                        + $"{Environment.NewLine}HTML dumped to {path}",
                    ex
                );
            }

            // Prefer fonts.ready over NetworkIdle — Blazor WASM + Google Fonts rarely go fully idle.
            sw?.Restart();
            await page.EvaluateAsync("() => document.fonts.ready");
            await WaitForStableBoundingBoxAsync(element);
            Timing.Log(sw, $"case[{verifyDirectory}/{verifyFileName}].fonts+stable-bbox");

            // Avoid accidental hover styles affecting the snapshot.
            await page.Mouse.MoveAsync(0, 0);

            var screenshotPath = Path.Combine(
                Path.GetTempPath(),
                $"{verifyFileName}_{viewportWidth}_{Guid.NewGuid():N}.png"
            );
            try
            {
                sw?.Restart();
                await element.ScreenshotAsync(
                    new LocatorScreenshotOptions { Path = screenshotPath }
                );
                Timing.Log(sw, $"case[{verifyDirectory}/{verifyFileName}].screenshot");

                sw?.Restart();
                await VerifyFile(screenshotPath)
                    .UseDirectory(verifyDirectory)
                    .UseFileName(verifyFileName);
                Timing.Log(sw, $"case[{verifyDirectory}/{verifyFileName}].verify");
            }
            finally
            {
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
            }

            Timing.Log(caseSw, $"case[{verifyDirectory}/{verifyFileName}].total");
        }
        finally
        {
            if (page is not null)
            {
                await page.CloseAsync();
            }

            await _host.ContextPool.ReturnAsync(context, viewportWidth, viewportHeight);
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
