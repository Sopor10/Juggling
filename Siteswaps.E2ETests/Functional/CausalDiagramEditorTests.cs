using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Siteswaps.Components.CausalDiagram;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Functional;

public sealed class CausalDiagramEditorTests(SharedBlazorFixture host)
    : IClassFixture<SharedBlazorFixture>
{
    [Fact]
    public async Task RunawayPhaseNavigation_LoadsThePhaseGraphAndPreservesItsJ5Deeplink()
    {
        await using var session = await WizardBrowserSession.CreateAsync(host.Fixture);
        await OpenEditor(session.Page, E2EBaseUrl.FromFixture(host.Fixture));

        await SelectPresetAsync(session.Page, "highgate-thirteenclubrunaway-initial-urban-terror");
        var navigation = session.Page.Locator(".runaway-sequence");
        await Assertions
            .Expect(
                navigation.GetByRole(
                    AriaRole.Button,
                    new() { Name = "1. Initial urban-terror feed" }
                )
            )
            .ToHaveAttributeAsync("aria-pressed", "true");
        var firstPhaseToken = GetStateToken(session.Page.Url);
        firstPhaseToken.Should().StartWith("j5.");
        CausalDiagramEditorState
            .TryDecode(firstPhaseToken, out var firstPhaseState)
            .Should()
            .BeTrue();
        firstPhaseState!.SequenceId.Should().Be("highgate-thirteenclubrunaway");
        firstPhaseState.SequencePhaseIndex.Should().Be(0);
        firstPhaseState.PresetId.Should().Be("highgate-thirteenclubrunaway-initial-urban-terror");
        await Assertions
            .Expect(
                navigation.GetByText(
                    new System.Text.RegularExpressions.Regex("Daniel: Walks across")
                )
            )
            .ToBeVisibleAsync();

        await navigation
            .GetByRole(AriaRole.Button, new() { Name = "2. Pass-pass-self after Daniel walks" })
            .ClickAsync();
        await Assertions
            .Expect(
                navigation.GetByRole(
                    AriaRole.Button,
                    new() { Name = "2. Pass-pass-self after Daniel walks" }
                )
            )
            .ToHaveAttributeAsync("aria-pressed", "true");
        var phaseTwoUrl = session.Page.Url;
        var phaseTwoToken = GetStateToken(phaseTwoUrl);
        phaseTwoToken.Should().StartWith("j5.");
        CausalDiagramEditorState.TryDecode(phaseTwoToken, out var phaseTwoState).Should().BeTrue();
        phaseTwoState!.SequenceId.Should().Be("highgate-thirteenclubrunaway");
        phaseTwoState!.SequencePhaseIndex.Should().Be(1);
        phaseTwoState.PresetId.Should().Be("highgate-thirteenclubrunaway-after-daniel-walk");

        var restoredPage = await session.Context.NewPageAsync();
        var restoredUri = new UriBuilder(phaseTwoUrl);
        restoredUri.Query = string.Join(
            '&',
            restoredUri
                .Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Where(parameter => !parameter.StartsWith("step=", StringComparison.Ordinal))
                .Append("step=2")
        );
        await restoredPage.GotoAsync(restoredUri.Uri.ToString());
        await restoredPage
            .Locator(".diagram-scroll")
            .WaitForAsync(
                new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30_000 }
            );
        await Assertions.Expect(restoredPage.Locator(".runaway-sequence")).ToBeVisibleAsync();
        await Assertions
            .Expect(
                restoredPage
                    .Locator(".runaway-sequence")
                    .GetByRole(
                        AriaRole.Button,
                        new() { Name = "2. Pass-pass-self after Daniel walks" }
                    )
            )
            .ToHaveAttributeAsync("aria-pressed", "true");
        var restoredToken = GetStateToken(restoredPage.Url);
        CausalDiagramEditorState.TryDecode(restoredToken, out var restoredState).Should().BeTrue();
        restoredState!.SequencePhaseIndex.Should().Be(1);
        restoredState.PresetId.Should().Be(phaseTwoState.PresetId);
    }

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
        await page.Locator(".juggler-add-target[data-juggler-id='P1']").PressAsync("Enter");
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

    [Fact]
    public async Task MobileSynchronousThrows_KeepHandTracksAndTableEventsDistinct()
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

        var state = CausalDiagramEditorState.CreateFeedingPattern(3, 3, "custom");
        state.Repetitions = 1;
        var firstJugglerId = state.Jugglers[0].Id;
        var firstJugglerName = state.Jugglers[0].Name;
        var source = state.Throws.First(item => item.JugglerId == firstJugglerId);
        var targetId = state.CausalEdges.Single(edge => edge.FromThrowId == source.Id).ToThrowId;
        var synchronousThrow = new EditorThrow(
            $"{firstJugglerId}-simultaneous",
            source.JugglerId,
            source.Hand == "R" ? "L" : "R",
            source.StartTime
        );
        state.Throws.Add(synchronousThrow);
        state.CausalEdges.Add(
            new EditorCausalEdge("A-simultaneous-edge", synchronousThrow.Id, targetId)
        );
        state.NormalizeCausalEdgeOffsets();
        var anneLeftThrows = state.Throws.Count(item =>
            item.JugglerId == firstJugglerId && item.Hand == "L"
        );
        var anneRightThrows = state.Throws.Count(item =>
            item.JugglerId == firstJugglerId && item.Hand == "R"
        );
        var token = Uri.EscapeDataString(state.Encode());
        var response = await page.GotoAsync(
            new Uri(
                E2EBaseUrl.FromFixture(host.Fixture),
                $"causal-diagram-editor?state={token}"
            ).AbsoluteUri
        );
        Assert.Equal(200, response?.Status);
        await page.Locator(".diagram-scroll").WaitForAsync();

        var pair = await page.EvaluateAsync<SimultaneousNodeMetrics>(
            """
            jugglerId => {
              const nodes = [...document.querySelectorAll('.pd-node')]
                .filter(node => node.dataset.jugglerId === jugglerId);
              const byX = new Map();
              for (const node of nodes) {
                const key = node.getAttribute('cx');
                byX.set(key, [...(byX.get(key) ?? []), node]);
              }
              const pair = [...byX.values()].find(items => items.length > 1);
              if (!pair) return null;
              const throwIds = pair.map(node => node.dataset.throwId);
              const targets = throwIds.map(id => document.querySelector(`.throw-hit-target[data-throw-id="${id}"]`));
              const centers = targets.map(target => {
                const rect = target.getBoundingClientRect();
                return { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2,
                  width: rect.width, height: rect.height, id: target.dataset.throwId };
              });
              return { centers };
            }
            """,
            firstJugglerId
        );
        Assert.NotNull(pair);
        Assert.Equal(2, pair!.Centers.Length);
        Assert.All(pair.Centers, target =>
        {
            Assert.True(target.Width >= 44);
            Assert.True(target.Height >= 44);
            Assert.InRange(target.X, 0, 390);
            Assert.InRange(target.Y, 0, 844);
        });
        Assert.True(Math.Abs(pair.Centers[0].Y - pair.Centers[1].Y) >= 56);

        await page.GetByRole(AriaRole.Button, new() { Name = "Tabelle einblenden" })
            .ScrollIntoViewIfNeededAsync();
        var tableToggle = page.GetByRole(AriaRole.Button, new() { Name = "Tabelle einblenden" });
        var toggleBox = await tableToggle.BoundingBoxAsync();
        Assert.NotNull(toggleBox);
        await page.Touchscreen.TapAsync(
            (float)(toggleBox!.X + toggleBox.Width / 2),
            (float)(toggleBox.Y + toggleBox.Height / 2)
        );
        var leftRowHeader = page.GetByRole(
            AriaRole.Rowheader,
            new() { Name = $"{firstJugglerName} · L" }
        );
        var rightRowHeader = page.GetByRole(
            AriaRole.Rowheader,
            new() { Name = $"{firstJugglerName} · R" }
        );
        await Assertions.Expect(leftRowHeader).ToBeVisibleAsync();
        await Assertions.Expect(rightRowHeader).ToBeVisibleAsync();
        var leftRow = leftRowHeader.Locator("..");
        var rightRow = rightRowHeader.Locator("..");
        Assert.Equal(anneLeftThrows, await leftRow.Locator(".sdv-beat").CountAsync());
        Assert.Equal(anneRightThrows, await rightRow.Locator(".sdv-beat").CountAsync());

        var touchTarget = page.Locator($".throw-hit-target[data-throw-id='{synchronousThrow.Id}']");
        await touchTarget.ScrollIntoViewIfNeededAsync();
        var touchBox = await touchTarget.BoundingBoxAsync();
        Assert.NotNull(touchBox);
        await touchTarget.EvaluateAsync(
            "node => node.addEventListener('pointerdown', event => window.__syncTouchDownPrevented = event.defaultPrevented)"
        );
        await page.Locator(".diagram-canvas")
            .EvaluateAsync(
                "svg => svg.addEventListener('gotpointercapture', event => window.__syncTouchCaptured = svg.hasPointerCapture(event.pointerId))"
            );
        var originalX = await touchTarget.GetAttributeAsync("cx");
        var startX = touchBox!.X + touchBox.Width / 2;
        var startY = touchBox.Y + touchBox.Height / 2;
        var cdp = await context.NewCDPSessionAsync(page);
        await DispatchTouch(cdp, "touchStart", startX, startY);
        await DispatchTouch(cdp, "touchMove", startX + 35, startY);
        await DispatchTouch(cdp, "touchMove", startX + 80, startY);
        await DispatchTouchEnd(cdp);
        await Assertions.Expect(touchTarget).Not.ToHaveAttributeAsync("cx", originalX!);
        Assert.True(await page.EvaluateAsync<bool>("() => window.__syncTouchDownPrevented"));
        Assert.True(await page.EvaluateAsync<bool>("() => window.__syncTouchCaptured"));
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

    private static string GetStateToken(string url)
    {
        var stateParameter = new Uri(url)
            .Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Single(parameter => parameter.StartsWith("state=", StringComparison.Ordinal));
        return Uri.UnescapeDataString(stateParameter["state=".Length..]);
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

    private sealed class SimultaneousNodeMetrics
    {
        public required SimultaneousNodeCenter[] Centers { get; init; }
    }

    private sealed class SimultaneousNodeCenter
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public required string Id { get; init; }
    }
}
