using System.Globalization;
using System.Text.RegularExpressions;
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
    public async Task CustomFeedingState_PersistsAndRestoresCompleteDeeplink()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        var baseUri = E2EBaseUrl.FromFixture(host.Fixture);
        await OpenEditor(session.Page, baseUri);
        await SaveDesignScreenshotAsync(session.Page, "causal-editor-desktop.png");

        await session.Page.Locator(".diagram-options summary").ClickAsync();
        await SelectPresetAsync(session.Page, "feeding-7-5");
        await session.Page.Locator(".juggler-add-target[data-juggler-id='P2']").ClickAsync();
        await OpenDiagramOptions(session.Page);
        await session.Page.Locator(".diagram-options").GetByLabel("Wiederholungen").FillAsync("3");
        await session
            .Page.Locator(".diagram-options")
            .GetByLabel("Wiederholungen")
            .PressAsync("Tab");
        await session.Page.GetByLabel("Einrasten").UncheckAsync();
        var sourceNode = session.Page.Locator(".throw-hit-target").First;
        await sourceNode.FocusAsync();
        await sourceNode.PressAsync("Enter");
        await Assertions.Expect(session.Page.Locator(".node-inspector")).ToHaveCountAsync(0);
        (await session.Page.TitleAsync()).Should().Be("Kausaldiagramm-Editor");
        (await session.Page.GetByLabel("Jongleurname").CountAsync()).Should().Be(0);
        (await session.Page.GetByLabel("Zyklus", new() { Exact = true }).CountAsync())
            .Should()
            .Be(0);
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
                .Page.GetByRole(AriaRole.Heading, new() { Name = "Kausaldiagramm-Editor" })
                .CountAsync()
        )
            .Should()
            .Be(0);
        (
            await session
                .Page.GetByRole(AriaRole.Button, new() { Name = "Link kopieren" })
                .CountAsync()
        )
            .Should()
            .Be(0);
        await SaveDesignScreenshotAsync(session.Page, "causal-editor-desktop-node-selected.png");

        var url = session.Page.Url;
        url.Should().Contain("state=j4.");
        url.Length.Should().BeLessThan(2_048);
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
            url,
            new PageGotoOptions { WaitUntil = WaitUntilState.Load }
        );
        await sharedPage.Locator(".diagram-options summary").ClickAsync();
        try
        {
            await sharedPage
                .Locator(".diagram-options")
                .GetByLabel("Diagrammvorlage")
                .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        }
        catch (PlaywrightException exception)
        {
            throw new Xunit.Sdk.XunitException(
                $"status={response?.Status}; url={sharedPage.Url}; errors={string.Join(" | ", browserErrors)}; console={string.Join(" | ", consoleErrors)}; body={await sharedPage.Locator("body").InnerTextAsync()}; {exception.Message}"
            );
        }
        (await sharedPage.Locator(".juggler-add-target[data-juggler-id='P2']").CountAsync())
            .Should()
            .Be(1);
        (
            await sharedPage
                .Locator(".diagram-options")
                .GetByLabel("Wiederholungen")
                .InputValueAsync()
        )
            .Should()
            .Be("3");
        (await sharedPage.GetByLabel("Einrasten").IsCheckedAsync()).Should().BeFalse();
        (await sharedPage.Locator(".throw-hit-target").CountAsync()).Should().Be(36);
        (await sharedPage.Locator(".causal-arrow").CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UnlinkedFeedingThrow_DoesNotDisplayAnIndependentHeight()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        await OpenEditor(session.Page, E2EBaseUrl.FromFixture(host.Fixture));
        await session.Page.Locator(".diagram-options summary").ClickAsync();
        await SelectPresetAsync(session.Page, "custom");
        await session.Page.Locator(".juggler-add-target[data-juggler-id='A']").PressAsync("Enter");

        var unlinkedThrow = session.Page.Locator(".throw-hit-target").Last;
        await unlinkedThrow.FocusAsync();
        var throwId = await unlinkedThrow.GetAttributeAsync("data-throw-id");
        await Assertions
            .Expect(session.Page.Locator($".pd-node-label[data-throw-id='{throwId}']").First)
            .ToHaveTextAsync("—");
        (await session.Page.Locator(".node-inspector").CountAsync()).Should().Be(0);
        (await unlinkedThrow.GetAttributeAsync("aria-label")).Should().Contain("Wurf —");

        (await session.Page.Locator(".node-inspector input[type=number]").CountAsync())
            .Should()
            .Be(0);

        await SelectPresetAsync(session.Page, "custom");
        await session.Page.Locator(".juggler-add-target[data-juggler-id='A']").PressAsync("Enter");
        await session.Page.Locator(".juggler-add-target[data-juggler-id='A']").PressAsync("Enter");
        (await session.Page.Locator(".node-inspector").CountAsync()).Should().Be(0);
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
            .ToHaveAttributeAsync("data-time-scale", "2");
        await Assertions
            .Expect(stage.Locator(".test-passist-widget"))
            .ToHaveAttributeAsync("data-throws", "60");

        await SelectPresetAsync(session.Page, "siteswap-3");
        await Assertions.Expect(stage).ToBeVisibleAsync();
        await Assertions
            .Expect(stage.Locator(".test-passist-widget"))
            .ToHaveAttributeAsync("data-time-scale", "1");
        await SelectPresetAsync(session.Page, "custom");
        await Assertions
            .Expect(animation.GetByRole(AriaRole.Status))
            .ToContainTextAsync("nicht als Passist-Animation dargestellt werden");
        await Assertions.Expect(animation.Locator(".passist-animation-stage")).ToHaveCountAsync(0);

        await SelectPresetAsync(session.Page, "feeding-7-5");
        await Assertions.Expect(stage).ToBeVisibleAsync();
        await Assertions
            .Expect(stage.Locator(".test-passist-widget"))
            .ToHaveAttributeAsync("data-throws", "24");
    }

    [Fact]
    public async Task MultiplexCanBeAddedOnMobileAndAppearsInTableAndPassistAnimation()
    {
        var browser = host.Fixture.Context?.Browser
            ?? throw new InvalidOperationException("Playwright browser is not available.");
        await using var context = await browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                Locale = E2ECulture.Locale,
                IsMobile = true,
                HasTouch = true,
                ViewportSize = new ViewportSize { Width = 390, Height = 844 },
            }
        );
        await BlazorTestServerProxy.InstallAsync(context, host.Fixture);
        await E2ECulture.InstallAsync(context);
        var page = await context.NewPageAsync();
        await OpenEditor(page, E2EBaseUrl.FromFixture(host.Fixture));
        await page.Locator(".diagram-options summary").ClickAsync();
        await SelectPresetAsync(page, "feeding-7-5");
        (await page.Locator(".diagram-options").EvaluateAsync<bool>("details => details.open"))
            .Should()
            .BeFalse("the mobile canvas should be touch-accessible after applying a preset");
        var optionsToggle = page.Locator(".diagram-options summary");
        await optionsToggle.TapAsync();
        var timelineWidth = page.Locator(".diagram-options input[type=range]").First;
        await timelineWidth.FocusAsync();
        await timelineWidth.PressAsync("Home");
        await timelineWidth.PressAsync("Tab");
        (await timelineWidth.InputValueAsync()).Should().Be("280");
        await optionsToggle.TapAsync();

        var diagramScrollCue = page.Locator(".diagram-scroll-cue");
        await page.WaitForFunctionAsync(
            "() => document.querySelector('.diagram-scroll-frame')?.classList.contains('has-horizontal-overflow')"
        );
        await Assertions.Expect(diagramScrollCue).ToBeVisibleAsync();

        var throwTargets = page.Locator(".throw-hit-target");
        var originalCount = await throwTargets.CountAsync();
        var animationWidget = page.Locator(".causal-editor-animation .test-passist-widget");
        await AssertPassistRadioControlsAsync(page);
        var originalAnimatedThrows = int.Parse(
            await animationWidget.GetAttributeAsync("data-throws") ?? "0",
            CultureInfo.InvariantCulture
        );
        var originalTarget = page.Locator(
            ".throw-hit-target[data-throw-id='P1-1'][data-cycle='0']"
        );
        var originalId = await originalTarget.GetAttributeAsync("data-throw-id");
        await originalTarget.TapAsync();
        await Assertions.Expect(originalTarget).ToHaveAttributeAsync("aria-pressed", "true");
        var addMultiplex = page.GetByTestId("add-multiplex-throw");
        await Assertions.Expect(addMultiplex).ToBeEnabledAsync();
        var addBox = await addMultiplex.BoundingBoxAsync();
        addBox.Should().NotBeNull();
        addBox!.Width.Should().BeGreaterThanOrEqualTo(44);
        addBox.Height.Should().BeGreaterThanOrEqualTo(44);
        await addMultiplex.ScrollIntoViewIfNeededAsync();
        await addMultiplex.TapAsync();

        (await throwTargets.CountAsync()).Should().BeGreaterThan(originalCount);
        await page.WaitForFunctionAsync(
            "expected => Number(document.querySelector('.causal-editor-animation .test-passist-widget')?.getAttribute('data-throws') ?? '0') > expected",
            originalAnimatedThrows
        );
        var newId = await page.Locator(".throw-hit-target[aria-pressed='true']")
            .First.GetAttributeAsync("data-throw-id");
        newId.Should().NotBe(originalId);
        (
            await page.Locator($".throw-hit-target[data-throw-id='{newId}'][data-cycle='0']")
                .GetAttributeAsync("aria-label")
        )
            .Should()
            .Contain("Multiplex");
        var originalGroupBox = await originalTarget.BoundingBoxAsync();
        var duplicateGroupBox = await page.Locator(
                $".throw-hit-target[data-throw-id='{newId}'][data-cycle='0']"
            )
            .BoundingBoxAsync();
        originalGroupBox.Should().NotBeNull();
        duplicateGroupBox.Should().NotBeNull();
        Math.Abs(
                originalGroupBox!.X
                    + originalGroupBox.Width / 2
                    - duplicateGroupBox!.X
                    - duplicateGroupBox.Width / 2
            )
            .Should()
            .BeGreaterThanOrEqualTo(Math.Max(originalGroupBox.Width, duplicateGroupBox.Width));
        await AssertMinimumSpacingMatchesDisplayAsync(page, optionsToggle);

        foreach (var targetId in new[] { originalId, newId })
        {
            var target = page.Locator(
                $".throw-hit-target[data-throw-id='{targetId}'][data-cycle='0']"
            );
            await target.TapAsync();
            (
                await page.Locator(".throw-hit-target[aria-pressed='true']")
                    .First.GetAttributeAsync("data-throw-id")
            )
                .Should()
                .Be(targetId);
        }

        await AssertMultiplexTableAndScrollAsync(page);
        await DeleteMultiplexOrbitAsync(page, throwTargets, newId, originalAnimatedThrows);
        await AssertMobileHitTargetsAndNoPageOverflowAsync(page, throwTargets);
    }

    [Fact]
    public async Task ArrowTargetHandle_CanChangeDestinationWithKeyboard()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        await OpenEditor(session.Page, E2EBaseUrl.FromFixture(host.Fixture));

        var handle = session.Page.Locator(".arrow-target-handle").First;
        (await handle.GetAttributeAsync("aria-label")).Should().Contain("Enter");
        var originalTargetId = await handle.GetAttributeAsync("data-target-throw-id");
        originalTargetId.Should().NotBeNullOrWhiteSpace();
        await handle.FocusAsync();
        await handle.PressAsync("Enter");
        await Assertions
            .Expect(handle)
            .Not.ToHaveAttributeAsync("data-target-throw-id", originalTargetId!);
        var focusedBeforeTab = await session.Page.EvaluateAsync<string>(
            "() => { const item = document.activeElement; return [item?.getAttribute('class'), item?.getAttribute('data-edge-id'), item?.getAttribute('data-cycle')].join('|'); }"
        );
        await handle.PressAsync("Tab");
        var focusedAfterTab = await session.Page.EvaluateAsync<string>(
            "() => { const item = document.activeElement; return [item?.getAttribute('class'), item?.getAttribute('data-edge-id'), item?.getAttribute('data-cycle')].join('|'); }"
        );
        focusedAfterTab
            .Should()
            .NotBe(focusedBeforeTab, "Tab must move focus past the arrow endpoint");
    }

    [Fact]
    public async Task CausalNodes_RenderVisibleCirclesOnEveryTimelineAtInitialScrollPosition()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        await OpenEditor(session.Page, E2EBaseUrl.FromFixture(host.Fixture));
        await SelectPresetAsync(session.Page, "feeding-7-5");
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
        var browser = host.Fixture.Context?.Browser
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
        await target.EvaluateAsync(
            "node => node.addEventListener('pointerdown', event => window.__touchDragDownPrevented = event.defaultPrevented)"
        );
        await page.Locator(".diagram-canvas")
            .EvaluateAsync(
                "svg => svg.addEventListener('gotpointercapture', event => window.__touchDragSvgCaptured = svg.hasPointerCapture(event.pointerId))"
            );
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

        await target.EvaluateAsync("node => node.blur()");
        await scroller.EvaluateAsync("node => node.scrollLeft = 0");
        var targetBox = await target.BoundingBoxAsync()
            ?? throw new InvalidOperationException("The throw target has no layout box.");
        var startX = targetBox.X + targetBox.Width / 2;
        var startY = targetBox.Y + targetBox.Height / 2;
        var urlBeforeTouchDrag = page.Url;
        var positionBeforeTouchDrag = await target.GetAttributeAsync("cx");
        var scrollLeftBeforeTouchDrag = await scroller.EvaluateAsync<double>(
            "node => node.scrollLeft"
        );
        var scrollTopBeforeTouchDrag = await page.EvaluateAsync<double>("() => window.scrollY");
        await DispatchTouch(cdp, "touchStart", startX, startY);
        await DispatchTouch(cdp, "touchMove", startX + 35, startY);
        await DispatchTouch(cdp, "touchMove", startX + 80, startY);
        await DispatchTouchEnd(cdp);
        await page.WaitForTimeoutAsync(150);
        (await page.EvaluateAsync<bool>("() => window.__touchDragDownPrevented")).Should().BeTrue();
        page.Url.Should().NotBe(urlBeforeTouchDrag);
        var positionAfterTouchDrag = await target.GetAttributeAsync("cx");
        positionAfterTouchDrag.Should().NotBe(positionBeforeTouchDrag);
        (await page.EvaluateAsync<bool>("() => window.__touchDragSvgCaptured")).Should().BeTrue();
        await Assertions.Expect(page.Locator(".pd-node.selected")).ToHaveCountAsync(2);
        (await scroller.EvaluateAsync<double>("node => node.scrollLeft"))
            .Should()
            .Be(scrollLeftBeforeTouchDrag);
        (await page.EvaluateAsync<double>("() => window.scrollY"))
            .Should()
            .Be(scrollTopBeforeTouchDrag);
    }

    [Fact]
    public async Task MobileTimeline_TwentyRepetitionsKeepNodesSpacedAndFocusableAfterScroll()
    {
        var browser = host.Fixture.Context?.Browser
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
        await SelectPresetAsync(page, "feeding-7-5");
        await OpenDiagramOptions(page);
        var repetitions = page.Locator(".diagram-options").GetByLabel("Wiederholungen");
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
        var visibleBox = await lastThrow.BoundingBoxAsync()
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

    private static async Task OpenDiagramOptions(IPage page)
    {
        var options = page.Locator(".diagram-options");
        if (!await options.EvaluateAsync<bool>("element => element.open"))
            await options.Locator("summary").ClickAsync();
    }

    private static async Task SelectPresetAsync(IPage page, string presetId)
    {
        await OpenDiagramOptions(page);
        await page.Locator($".diagram-options [data-preset-id='{presetId}']").ClickAsync();
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
        await page.GetByRole(AriaRole.Button, new() { Name = "Weiter zum Diagramm" }).ClickAsync();
        var diagram = page.Locator(".diagram-scroll");
        try
        {
            await diagram.WaitForAsync(
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

    private static async Task<double> GetReportedMinimumThrowSpacing(IPage page)
    {
        var spacingOutput = await page.Locator(".diagram-options output").First.InnerTextAsync();
        return Regex
            .Matches(spacingOutput, @"min\. Abstand zwischen Wurfknoten ([0-9.]+) px")
            .Cast<Match>()
            .Select(match => double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
            .Min();
    }

    private static async Task AssertPassistRadioControlsAsync(IPage page)
    {
        var orbitToggle = page.GetByRole(AriaRole.Radio, new() { Name = "Orbits" });
        var passSelfToggle = page.GetByRole(AriaRole.Radio, new() { Name = "Pass / Self" });
        (await orbitToggle.BoundingBoxAsync())!.Height.Should().BeGreaterThanOrEqualTo(44);
        (await passSelfToggle.BoundingBoxAsync())!.Height.Should().BeGreaterThanOrEqualTo(44);
        await orbitToggle.FocusAsync();
        await orbitToggle.PressAsync("ArrowRight");
        await Assertions.Expect(passSelfToggle).ToBeFocusedAsync();
        await Assertions.Expect(passSelfToggle).ToHaveAttributeAsync("aria-checked", "true");
        await passSelfToggle.PressAsync("ArrowLeft");
        await Assertions.Expect(orbitToggle).ToBeFocusedAsync();
        await Assertions.Expect(orbitToggle).ToHaveAttributeAsync("aria-checked", "true");
    }

    private static async Task AssertMinimumSpacingMatchesDisplayAsync(
        IPage page,
        ILocator optionsToggle
    )
    {
        var minimumSameLaneGap = await page.EvaluateAsync<double>(
            """
            () => {
              const rows = new Map();
              for (const target of document.querySelectorAll('.throw-hit-target')) {
                const key = target.getAttribute('cy');
                const centers = rows.get(key) ?? [];
                centers.push(Number(target.getAttribute('cx')));
                rows.set(key, centers);
              }
              const gaps = [...rows.values()].flatMap(centers => {
                const ordered = centers.sort((a, b) => a - b);
                return ordered.slice(1).map((center, index) => center - ordered[index]);
              });
              return Math.min(...gaps);
            }
            """
        );
        minimumSameLaneGap.Should().BeGreaterThanOrEqualTo(56);
        await optionsToggle.TapAsync();
        (await GetReportedMinimumThrowSpacing(page))
            .Should()
            .BeApproximately(minimumSameLaneGap, 2);
        await optionsToggle.TapAsync();
    }

    private static async Task AssertMultiplexTableAndScrollAsync(IPage page)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "Tabelle einblenden" }).TapAsync();
        var hasMultiplexCell = await page.Locator(".causal-editor-pattern-table tbody td")
            .EvaluateAllAsync<bool>(
                "cells => cells.some(cell => cell.querySelectorAll('.sdv-beat').length > 1 && [...cell.querySelectorAll('.sdv-beat')].every(chip => chip.getAttribute('aria-label')?.includes('Hand')))"
            );
        hasMultiplexCell.Should().BeTrue();
        var geometry = await page.Locator(".causal-editor-table-scroll")
            .EvaluateAsync<double[]>(
                "element => { element.scrollLeft = 0; const timeHeader = element.querySelector('thead th:nth-child(2)'); const rowHeader = element.querySelector('tbody th[scope=row]'); const firstTimeX = timeHeader.getBoundingClientRect().x; const firstRowX = rowHeader.getBoundingClientRect().x; element.scrollLeft = Math.min(120, element.scrollWidth - element.clientWidth); const scrolledTimeX = timeHeader.getBoundingClientRect().x; const scrolledRowX = rowHeader.getBoundingClientRect().x; return [firstTimeX, scrolledTimeX, firstRowX, scrolledRowX, element.scrollLeft]; }"
            );
        (geometry[0] - geometry[1]).Should().BeGreaterThan(40);
        Math.Abs(geometry[2] - geometry[3]).Should().BeLessThan(4);
        geometry[4].Should().BeGreaterThan(40);
    }

    private static async Task DeleteMultiplexOrbitAsync(
        IPage page,
        ILocator throwTargets,
        string? newId,
        int originalAnimatedThrows
    )
    {
        var deleteThrow = page.GetByRole(
            AriaRole.Button,
            new() { Name = "Multiplex-Orbit löschen" }
        );
        await Assertions.Expect(deleteThrow).ToBeVisibleAsync();
        var deleteBox = await deleteThrow.BoundingBoxAsync();
        deleteBox.Should().NotBeNull();
        deleteBox!.Width.Should().BeGreaterThanOrEqualTo(44);
        deleteBox.Height.Should().BeGreaterThanOrEqualTo(44);
        var beforeDelete = await throwTargets.CountAsync();
        await deleteThrow.TapAsync();
        (await throwTargets.CountAsync()).Should().BeLessThan(beforeDelete);
        (await page.Locator($".throw-hit-target[data-throw-id='{newId}']").CountAsync())
            .Should()
            .Be(0);
        await page.WaitForFunctionAsync(
            "expected => Number(document.querySelector('.causal-editor-animation .test-passist-widget')?.getAttribute('data-throws') ?? '0') === expected",
            originalAnimatedThrows
        );
    }

    private static async Task AssertMobileHitTargetsAndNoPageOverflowAsync(
        IPage page,
        ILocator throwTargets
    )
    {
        var hitTarget = await throwTargets.First.BoundingBoxAsync();
        hitTarget.Should().NotBeNull();
        hitTarget!.Width.Should().BeGreaterThanOrEqualTo(44);
        hitTarget.Height.Should().BeGreaterThanOrEqualTo(44);
        (
            await page.EvaluateAsync<int>(
                "document.documentElement.scrollWidth <= window.innerWidth ? 0 : 1"
            )
        )
            .Should()
            .Be(0);
    }

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
