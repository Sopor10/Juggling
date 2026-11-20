using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

public sealed class CausalDiagramEditorTests(SharedBlazorFixture host)
    : IClassFixture<SharedBlazorFixture>
{
    [Fact]
    public async Task CustomFeedingState_CopiesAndRestoresCompleteDeeplink()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var baseUri = E2EBaseUrl.FromFixture(host.Fixture);
        await OpenEditor(session.Page, baseUri);
        await SaveDesignScreenshotAsync(session.Page, "causal-editor-desktop.png");

        await session.Page.GetByLabel("Vorlage").SelectOptionAsync("feeding-7-5");
        await session.Page.GetByLabel("Knoten auf Zeitleiste").SelectOptionAsync("P2");
        await session.Page.GetByLabel("Wiederholungen").FillAsync("3");
        await session.Page.GetByLabel("Wiederholungen").PressAsync("Tab");
        await session.Page.GetByLabel("Einrasten").UncheckAsync();
        var sourceNode = session.Page.Locator(".throw-hit-target").First;
        await sourceNode.FocusAsync();
        await sourceNode.PressAsync("Enter");
        await Assertions.Expect(session.Page.GetByText("Höhe (Schläge)")).ToBeVisibleAsync();
        (
            await session
                .Page.GetByRole(AriaRole.Heading, new() { Name = "Kausaler Diagramm-Editor" })
                .CountAsync()
        )
            .Should()
            .Be(1);
        (await session.Page.GetByLabel("Jongleurname").CountAsync()).Should().Be(0);
        (await session.Page.GetByLabel("Zyklus").CountAsync()).Should().Be(0);
        (await session.Page.Locator(".node-inspector input[type=number]").CountAsync())
            .Should()
            .Be(0);
        (
            await session
                .Page.GetByRole(AriaRole.Button, new() { Name = "Pfeil hinzufügen" })
                .CountAsync()
        )
            .Should()
            .Be(0);
        (
            await session
                .Page.Locator(".editor-header")
                .GetByText("PASSING ZONE", new() { Exact = false })
                .CountAsync()
        )
            .Should()
            .Be(0);
        (
            await session
                .Page.Locator(".editor-header")
                .GetByText("Würfe als Knoten", new() { Exact = false })
                .CountAsync()
        )
            .Should()
            .Be(0);
        await SaveDesignScreenshotAsync(session.Page, "causal-editor-desktop-node-selected.png");
        await session.Page.Locator(".node-inspector select").First.SelectOptionAsync("L");
        var targetSelect = session.Page.GetByLabel("Kausaler Zielknoten");
        var currentTarget = await targetSelect.InputValueAsync();
        var targetOptions = await targetSelect
            .Locator("option")
            .EvaluateAllAsync<string[]>("options => options.map(option => option.value)");
        var newTarget = targetOptions.First(value => value.Length > 0 && value != currentTarget);
        await targetSelect.SelectOptionAsync(newTarget);

        var url = session.Page.Url;
        url.Should().Contain("state=j3.");
        url.Length.Should().BeLessThan(2_048);
        await session.Context.GrantPermissionsAsync(
            ["clipboard-read", "clipboard-write"],
            new BrowserContextGrantPermissionsOptions
            {
                Origin = baseUri.GetLeftPart(UriPartial.Authority),
            }
        );
        await session
            .Page.GetByRole(AriaRole.Button, new() { Name = "Link kopieren" })
            .ClickAsync();
        await Assertions
            .Expect(
                session
                    .Page.GetByRole(AriaRole.Status)
                    .Filter(new() { HasText = "Deeplink kopiert" })
            )
            .ToBeVisibleAsync();
        var copiedUrl = await session.Page.EvaluateAsync<string>(
            "() => navigator.clipboard.readText()"
        );
        copiedUrl.Should().Be(url);

        var sharedPage = await session.Context.NewPageAsync();
        var browserErrors = new List<string>();
        var consoleErrors = new List<string>();
        sharedPage.PageError += (_, error) => browserErrors.Add(error);
        sharedPage.Console += (_, message) =>
        {
            if (message.Type == "error")
                consoleErrors.Add(message.Text);
        };
        var response = await sharedPage.GotoAsync(
            copiedUrl,
            new PageGotoOptions { WaitUntil = WaitUntilState.Load }
        );
        try
        {
            await Assertions
                .Expect(
                    sharedPage.GetByRole(
                        AriaRole.Heading,
                        new() { Name = "Kausaler Diagramm-Editor" }
                    )
                )
                .ToBeVisibleAsync();
        }
        catch (PlaywrightException exception)
        {
            throw new Xunit.Sdk.XunitException(
                $"status={response?.Status}; url={sharedPage.Url}; errors={string.Join(" | ", browserErrors)}; console={string.Join(" | ", consoleErrors)}; body={await sharedPage.Locator("body").InnerTextAsync()}; {exception.Message}"
            );
        }
        (await sharedPage.GetByLabel("Vorlage").InputValueAsync()).Should().Be("custom");
        (await sharedPage.GetByLabel("Knoten auf Zeitleiste").InputValueAsync()).Should().Be("P2");
        (await sharedPage.GetByLabel("Wiederholungen").InputValueAsync()).Should().Be("3");
        (await sharedPage.GetByLabel("Einrasten").IsCheckedAsync()).Should().BeFalse();
        (await sharedPage.Locator(".throw-hit-target").CountAsync()).Should().Be(36);
        (await sharedPage.Locator(".causal-arrow").CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CustomSiteswapAndFeedingInputs_LoadEditablePatterns()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var baseUri = E2EBaseUrl.FromFixture(host.Fixture);
        await OpenEditor(session.Page, baseUri);

        await session.Page.GetByText("Muster laden").ClickAsync();
        await session.Page.Locator("#custom-siteswap").FillAsync("423");
        await session.Page.Locator(".pattern-loader-grid > div:first-child button").ClickAsync();
        (await session.Page.Locator(".throw-hit-target").CountAsync()).Should().Be(12);

        await session
            .Page.Locator("#custom-feeding")
            .FillAsync("Ada: 5>B, 3>Ada, 1>Ada\nB: 3>Ada, 3>Ada, 3>B");
        await session.Page.Locator(".pattern-loader-grid > div:nth-child(2) button").ClickAsync();
        (await session.Page.Locator(".throw-hit-target").CountAsync()).Should().Be(12);
        await Assertions.Expect(session.Page.GetByLabel("Vorlage")).ToHaveValueAsync("custom");
        await Assertions.Expect(session.Page.GetByRole(AriaRole.Alert)).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task UnlinkedFeedingThrow_DoesNotDisplayAnIndependentHeight()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        await OpenEditor(session.Page, E2EBaseUrl.FromFixture(host.Fixture));
        await session.Page.GetByLabel("Vorlage").SelectOptionAsync("custom");
        await session.Page.GetByRole(AriaRole.Button, new() { Name = "+ Knoten" }).ClickAsync();

        var unlinkedThrow = session.Page.Locator(".throw-hit-target[aria-pressed='true']").First;
        var throwId = await unlinkedThrow.GetAttributeAsync("data-throw-id");
        await Assertions
            .Expect(session.Page.Locator($".pd-node-label[data-throw-id='{throwId}']").First)
            .ToHaveTextAsync("—");
        await Assertions
            .Expect(session.Page.Locator(".node-inspector output"))
            .ToHaveTextAsync("—");
        (await unlinkedThrow.GetAttributeAsync("aria-label")).Should().Contain("Wurf —");

        (await session.Page.Locator(".node-inspector input[type=number]").CountAsync())
            .Should()
            .Be(0);

        await session.Page.GetByLabel("Vorlage").SelectOptionAsync("custom");
        await session.Page.GetByRole(AriaRole.Button, new() { Name = "+ Knoten" }).ClickAsync();
        await session.Page.GetByRole(AriaRole.Button, new() { Name = "+ Knoten" }).ClickAsync();
        var targetOptionLabels = await session
            .Page.Locator(".node-inspector select")
            .Nth(1)
            .Locator("option")
            .AllTextContentsAsync();
        targetOptionLabels.Should().Contain(label => label.Contains('—'));
    }

    [Fact]
    public async Task PassistAnimation_UsesMappedEditorStateAndShowsUnavailableForIncompleteGraph()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        await OpenEditor(session.Page, E2EBaseUrl.FromFixture(host.Fixture));
        var animation = session.Page.Locator(".causal-editor-animation");
        await Assertions.Expect(animation).ToBeVisibleAsync();
        var stage = animation.Locator(".passist-animation-stage");
        await Assertions.Expect(stage).ToBeVisibleAsync();
        await Assertions
            .Expect(stage.Locator(".test-passist-widget"))
            .ToHaveAttributeAsync("data-time-scale", "1");
        await Assertions
            .Expect(stage.Locator(".test-passist-widget"))
            .ToHaveAttributeAsync("data-throws", "24");

        await session.Page.GetByLabel("Vorlage").SelectOptionAsync("siteswap-531");
        await Assertions.Expect(stage).ToBeVisibleAsync();
        await Assertions
            .Expect(animation.Locator("[role=img][aria-label='Passist-JIF-Animation']"))
            .ToBeVisibleAsync();
        await Assertions
            .Expect(stage.Locator(".test-passist-widget"))
            .ToHaveAttributeAsync("data-time-scale", "1");
        await session.Page.GetByLabel("Vorlage").SelectOptionAsync("custom");
        await Assertions
            .Expect(animation.GetByRole(AriaRole.Status))
            .ToContainTextAsync("nicht als Passist-Animation dargestellt werden");
        await Assertions.Expect(animation.Locator(".passist-animation-stage")).ToHaveCountAsync(0);

        await session.Page.GetByLabel("Vorlage").SelectOptionAsync("feeding-7-5");
        await Assertions.Expect(stage).ToBeVisibleAsync();
        await Assertions
            .Expect(stage.Locator(".test-passist-widget"))
            .ToHaveAttributeAsync("data-throws", "24");
    }

    [Fact]
    public async Task CausalNodes_RenderVisibleCirclesOnEveryTimelineAtInitialScrollPosition()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        await OpenEditor(session.Page, E2EBaseUrl.FromFixture(host.Fixture));
        await SaveDesignScreenshotAsync(session.Page, "causal-editor-visible-nodes.png");
        var geometryJson = await session.Page.EvaluateAsync<string>(
            """
            () => {
              const scroller = document.querySelector('.diagram-scroll');
              const scrollerBox = scroller.getBoundingClientRect();
              const nodes = [...document.querySelectorAll('.diagram-canvas circle.pd-node')];
              const rows = [...new Set(nodes.map(node => node.getAttribute('cy')))].map(cy => {
                const rowNodes = nodes.filter(node => node.getAttribute('cy') === cy);
                const sample = rowNodes[0];
                const rect = sample.getBoundingClientRect();
                const style = getComputedStyle(sample);
                return { cy: Number(cy), jugglerId: sample.dataset.jugglerId, throwId: sample.dataset.throwId,
                  cx: sample.getAttribute('cx'), count: rowNodes.length, diameter: rect.width, left: rect.left,
                  fill: style.fill, stroke: style.stroke, strokeWidth: style.strokeWidth };
              });
              return JSON.stringify({ scrollLeft: scroller.scrollLeft, scrollerLeft: scrollerBox.left, rows });
            }
            """
        );
        using var geometry = System.Text.Json.JsonDocument.Parse(geometryJson);
        var root = geometry.RootElement;
        root.GetProperty("scrollLeft").GetDouble().Should().Be(0);
        var rows = root.GetProperty("rows").EnumerateArray().ToArray();
        rows.Should().HaveCount(2);
        rows.Select(row => row.GetProperty("count").GetInt32()).Should().ContainInOrder(14, 10);
        var scrollerLeft = root.GetProperty("scrollerLeft").GetDouble();
        rows.Should()
            .AllSatisfy(row =>
            {
                row.GetProperty("diameter").GetDouble().Should().Be(40);
                row.GetProperty("fill").GetString().Should().NotBe("rgba(0, 0, 0, 0)");
                row.GetProperty("stroke").GetString().Should().Be("rgb(60, 40, 109)");
                row.GetProperty("strokeWidth").GetString().Should().Be("2px");
                row.GetProperty("left")
                    .GetDouble()
                    .Should()
                    .BeGreaterThan(
                        scrollerLeft,
                        $"row={row.GetProperty("jugglerId").GetString()} throw={row.GetProperty("throwId").GetString()} cx={row.GetProperty("cx").GetString()}"
                    );
            });
    }

    [Fact]
    public async Task MobileViewport_HasScrollableTimelineLargeTouchTargetsAndVisibleKeyboardFocus()
    {
        var browser =
            host.Fixture.Context?.Browser
            ?? throw new InvalidOperationException("Playwright browser is not available.");
        await using var context = await browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                IsMobile = true,
                HasTouch = true,
                ViewportSize = new ViewportSize { Width = 390, Height = 844 },
            }
        );
        await BlazorTestServerProxy.InstallAsync(context, host.Fixture);
        await E2ECulture.InstallAsync(context);
        var page = await context.NewPageAsync();
        var baseUri = E2EBaseUrl.FromFixture(host.Fixture);
        await OpenEditor(page, baseUri);
        await SaveDesignScreenshotAsync(page, "causal-editor-mobile.png");

        var metrics = await page.EvaluateAsync<MobileDiagramMetrics>(
            """
            () => {
            const scroller = document.querySelector('.diagram-scroll');
            const canvas = document.querySelector('.diagram-canvas');
            const target = document.querySelector('.throw-hit-target');
            const box = target.getBoundingClientRect();
            return {
                clientWidth: scroller.clientWidth,
                scrollWidth: scroller.scrollWidth,
                touchAction: getComputedStyle(canvas).touchAction,
                targetWidth: box.width,
                targetHeight: box.height
            };
            }
            """
        );
        metrics.ScrollWidth.Should().BeGreaterThan(metrics.ClientWidth);
        metrics.TouchAction.Should().Be("pan-x pan-y");
        metrics.TargetWidth.Should().BeGreaterThanOrEqualTo(44);
        metrics.TargetHeight.Should().BeGreaterThanOrEqualTo(44);

        var scroller = page.Locator(".diagram-scroll");
        await scroller.ScrollIntoViewIfNeededAsync();
        await scroller.EvaluateAsync("node => node.scrollLeft = 0");
        var swipePoint = await page.EvaluateAsync<SwipePoint>(
            """
            () => {
              const scroller = document.querySelector('.diagram-scroll').getBoundingClientRect();
              const svg = document.querySelector('.diagram-canvas').getBoundingClientRect();
              for (let y = Math.max(2, svg.top + 2); y < Math.min(window.innerHeight - 2, svg.bottom - 2); y += 8) {
                for (let x = Math.max(2, scroller.left + 2); x < Math.min(window.innerWidth - 2, scroller.right - 2); x += 8) {
                  const tag = document.elementFromPoint(x, y)?.tagName ?? '';
                  if (tag === 'svg') return { x, y, tag };
                }
              }
              return { x: 0, y: 0, tag: '' };
            }
            """
        );
        swipePoint.Tag.Should().Be("svg");
        var swipeX = swipePoint.X;
        var swipeY = swipePoint.Y;
        var cdp = await context.NewCDPSessionAsync(page);
        await DispatchTouch(cdp, "touchStart", swipeX, swipeY);
        await DispatchTouch(cdp, "touchMove", swipeX - 120, swipeY);
        await DispatchTouchEnd(cdp);
        await page.WaitForTimeoutAsync(100);
        var scrollLeft = await scroller.EvaluateAsync<double>("node => node.scrollLeft");
        scrollLeft.Should().BeGreaterThan(0);

        var target = page.Locator(".throw-hit-target").First;
        await scroller.EvaluateAsync("node => node.scrollLeft = 0");
        await target.FocusAsync();
        var positionBeforeArrow = await target.GetAttributeAsync("cx");
        var urlBeforeArrow = page.Url;
        await target.PressAsync("ArrowRight");
        await Assertions.Expect(target).Not.ToHaveAttributeAsync("cx", positionBeforeArrow!);
        page.Url.Should().NotBe(urlBeforeArrow);
        var focusStroke = await target.EvaluateAsync<string>(
            "node => getComputedStyle(node).stroke"
        );
        focusStroke.Should().Be("rgb(0, 179, 255)");

        await target.ScrollIntoViewIfNeededAsync();
        var targetBox =
            await target.BoundingBoxAsync()
            ?? throw new InvalidOperationException("The throw target has no layout box.");
        var startX = targetBox.X + targetBox.Width / 2;
        var startY = targetBox.Y + targetBox.Height / 2;
        var urlBeforeTouchDrag = page.Url;
        await DispatchTouch(cdp, "touchStart", startX, startY);
        await page.WaitForTimeoutAsync(100);
        await DispatchTouch(cdp, "touchMove", startX + 35, startY);
        await DispatchTouch(cdp, "touchMove", startX + 80, startY);
        await DispatchTouchEnd(cdp);
        await page.WaitForTimeoutAsync(500);
        page.Url.Should().NotBe(urlBeforeTouchDrag);
        await Assertions.Expect(page.Locator(".pd-node.selected")).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task MobileTimeline_TwentyRepetitionsKeepNodesSpacedAndFocusableAfterScroll()
    {
        var browser =
            host.Fixture.Context?.Browser
            ?? throw new InvalidOperationException("Playwright browser is not available.");
        await using var context = await browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                IsMobile = true,
                HasTouch = true,
                ViewportSize = new ViewportSize { Width = 390, Height = 844 },
            }
        );
        await BlazorTestServerProxy.InstallAsync(context, host.Fixture);
        await E2ECulture.InstallAsync(context);
        var page = await context.NewPageAsync();
        await OpenEditor(page, E2EBaseUrl.FromFixture(host.Fixture));
        var repetitions = page.GetByLabel("Wiederholungen");
        await repetitions.FillAsync("20");
        await repetitions.PressAsync("Tab");

        var scroller = page.Locator(".diagram-scroll");
        var geometry = await page.EvaluateAsync<TimelineGeometry>(
            """
            () => {
              const svg = document.querySelector('.diagram-canvas');
              const targets = [...document.querySelectorAll('.throw-hit-target')];
              return {
                clientWidth: document.querySelector('.diagram-scroll').clientWidth,
                scrollWidth: document.querySelector('.diagram-scroll').scrollWidth,
                svgWidth: svg.getBoundingClientRect().width,
                targetDiameter: targets[0].getBoundingClientRect().width,
                centerGap: Math.abs(Number(targets[1].getAttribute('cx')) - Number(targets[0].getAttribute('cx')))
              };
            }
            """
        );
        geometry.ScrollWidth.Should().BeGreaterThan(geometry.ClientWidth);
        geometry.SvgWidth.Should().BeGreaterThan(10_000);
        geometry.TargetDiameter.Should().BeGreaterThanOrEqualTo(44);
        geometry.CenterGap.Should().BeGreaterThanOrEqualTo(geometry.TargetDiameter);

        await scroller.EvaluateAsync("node => node.scrollLeft = node.scrollWidth");
        var scrollLeft = await scroller.EvaluateAsync<double>("node => node.scrollLeft");
        scrollLeft.Should().BeGreaterThan(0);
        var lastThrow = page.Locator(".throw-hit-target").Last;
        await lastThrow.FocusAsync();
        var visibleBox =
            await lastThrow.BoundingBoxAsync()
            ?? throw new InvalidOperationException("The final repeated throw is not laid out.");
        var visibleCenterX = visibleBox.X + visibleBox.Width / 2;
        visibleCenterX.Should().BeGreaterThan(0);
        visibleCenterX.Should().BeLessThan(390);
        var lastPosition = await lastThrow.GetAttributeAsync("cx");
        var urlBeforeArrow = page.Url;
        await lastThrow.PressAsync("ArrowRight");
        await Assertions.Expect(lastThrow).Not.ToHaveAttributeAsync("cx", lastPosition!);
        page.Url.Should().NotBe(urlBeforeArrow);
        var focusStroke = await lastThrow.EvaluateAsync<string>(
            "node => getComputedStyle(node).stroke"
        );
        focusStroke.Should().Be("rgb(0, 179, 255)");
    }

    private static async Task OpenEditor(IPage page, Uri baseUri)
    {
        var pageErrors = new List<string>();
        var consoleErrors = new List<string>();
        var failedResponses = new List<string>();
        page.PageError += (_, error) => pageErrors.Add(error);
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
                consoleErrors.Add(message.Text);
        };
        page.Response += (_, response) =>
        {
            if (response.Status >= 400)
                failedResponses.Add($"{response.Status} {response.Url}");
        };
        await page.Context.RouteAsync(
            "https://passist.org/api/animation-widget-standalone.mjs",
            route =>
                route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        Status = 200,
                        ContentType = "text/javascript",
                        Headers = new Dictionary<string, string>
                        {
                            ["Access-Control-Allow-Origin"] = "*",
                        },
                        Body = """
                        export default class AnimationWidget {
                          constructor({ target, props }) { this.target = target; this.props = props; this.render(); }
                          render() {
                            this.el?.remove();
                            this.el = document.createElement('div');
                            this.el.className = 'test-passist-widget';
                            this.el.dataset.throws = String(this.props.jif.throws?.length ?? 0);
                            this.el.dataset.timeScale = String(this.props.jif.timeStretchFactor ?? 0);
                            this.target.appendChild(this.el);
                          }
                          $set(props) { this.props = { ...this.props, ...props }; this.render(); }
                          $destroy() { this.el?.remove(); }
                        }
                        """,
                    }
                )
        );
        var response = await page.GotoAsync(
            new Uri(baseUri, "causal-diagram-editor").ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.Load }
        );
        var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Kausaler Diagramm-Editor" });
        try
        {
            await heading.WaitForAsync(
                new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30_000 }
            );
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException(
                $"Editor page did not render: base={baseUri}; url={page.Url}; status={response?.Status}; contentType={response?.Headers.GetValueOrDefault("content-type")}; body={await page.Locator("body").InnerTextAsync()}; pageErrors={string.Join(" | ", pageErrors)}; consoleErrors={string.Join(" | ", consoleErrors)}; failedResponses={string.Join(" | ", failedResponses)}"
            );
        }
    }

    private static Task SaveDesignScreenshotAsync(IPage page, string fileName)
    {
        var directory = Environment.GetEnvironmentVariable("CAUSAL_DIAGRAM_SCREENSHOT_DIR");
        return string.IsNullOrWhiteSpace(directory)
            ? Task.CompletedTask
            : page.ScreenshotAsync(
                new PageScreenshotOptions
                {
                    Path = Path.Combine(directory, fileName),
                    FullPage = true,
                }
            );
    }

    private static Task<System.Text.Json.JsonElement?> DispatchTouch(
        ICDPSession cdp,
        string type,
        double x,
        double y
    ) =>
        cdp.SendAsync(
            "Input.dispatchTouchEvent",
            new Dictionary<string, object>
            {
                ["type"] = type,
                ["touchPoints"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = 1,
                        ["x"] = x,
                        ["y"] = y,
                        ["radiusX"] = 2,
                        ["radiusY"] = 2,
                    },
                },
            }
        );

    private static Task<System.Text.Json.JsonElement?> DispatchTouchEnd(ICDPSession cdp) =>
        cdp.SendAsync(
            "Input.dispatchTouchEvent",
            new Dictionary<string, object>
            {
                ["type"] = "touchEnd",
                ["touchPoints"] = Array.Empty<object>(),
            }
        );

    private sealed class MobileDiagramMetrics
    {
        public double ClientWidth { get; set; }
        public double ScrollWidth { get; set; }
        public string TouchAction { get; set; } = "";
        public double TargetWidth { get; set; }
        public double TargetHeight { get; set; }
    }

    private sealed class SwipePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Tag { get; set; } = "";
    }

    private sealed class TimelineGeometry
    {
        public double ClientWidth { get; set; }
        public double ScrollWidth { get; set; }
        public double SvgWidth { get; set; }
        public double TargetDiameter { get; set; }
        public double CenterGap { get; set; }
    }
}
