using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;

namespace Siteswaps.E2ETests.Functional;

public sealed class CausalDiagramArrowDragTests(SharedBlazorFixture host)
    : IClassFixture<SharedBlazorFixture>
{
    [Fact]
    public async Task DraggingArrowOntoOccupiedThrowMovesItAndKeepsTemporaryGraphState()
    {
        var page = await host.Fixture.Context!.NewPageAsync();
        page.SetDefaultTimeout(15_000);
        await page.GotoAsync(
            new Uri(E2EBaseUrl.FromFixture(host.Fixture), "causal-diagram-editor").AbsoluteUri
        );
        await page.GetByRole(AriaRole.Button, new() { Name = "Weiter zum Diagramm" }).ClickAsync();
        await page.Locator(".arrow-target-handle").First.WaitForAsync();
        Assert.Equal("1", await page.Locator(".pd-node-label").First.TextContentAsync());
        Assert.Equal(0, await page.Locator(".animation-unavailable").CountAsync());

        await page.Locator(".diagram-options summary").ClickAsync();
        var timelineWidth = page.Locator(".diagram-options input[type=range]").Nth(0);
        await timelineWidth.FocusAsync();
        await timelineWidth.PressAsync("ArrowRight");
        await timelineWidth.PressAsync("Tab");
        Assert.Equal("600", await timelineWidth.InputValueAsync());
        Assert.Contains("state=j3.", page.Url);
        await page.Locator(".diagram-options summary").ClickAsync();

        var arrowHandle = page.Locator(".arrow-target-handle").First;
        var edgeId = await arrowHandle.GetAttributeAsync("data-edge-id");
        var originalX = await arrowHandle.GetAttributeAsync("cx");
        var originalUrl = page.Url;
        var destination = await page.Locator(".throw-hit-target").Nth(1).BoundingBoxAsync();
        var handle = await arrowHandle.BoundingBoxAsync();
        Assert.NotNull(destination);
        Assert.NotNull(handle);

        await page.Mouse.MoveAsync(handle!.X + handle.Width / 2, handle.Y + handle.Height / 2);
        await page.Mouse.DownAsync();
        await page.WaitForTimeoutAsync(100);
        await page.Mouse.MoveAsync(
            destination!.X + destination.Width / 2,
            destination.Y + destination.Height / 2,
            new MouseMoveOptions { Steps = 8 }
        );
        await page.WaitForFunctionAsync(
            "({ edgeId, originalX }) => { const item = [...document.querySelectorAll('.arrow-target-handle')].find(node => node.dataset.edgeId === edgeId); return item && item.getAttribute('cx') !== originalX; }",
            new { edgeId, originalX }
        );
        await page.Mouse.UpAsync();

        await page.WaitForFunctionAsync(
            "({ edgeId, originalX }) => { const item = [...document.querySelectorAll('.arrow-target-handle')].find(node => node.dataset.edgeId === edgeId); return item && item.getAttribute('cx') !== originalX; }",
            new { edgeId, originalX }
        );
        Assert.NotEqual(originalUrl, page.Url);
        Assert.Contains("state=j3.", page.Url);
    }

    [Fact]
    public async Task TouchDraggingArrowHandleDoesNotScrollTimelineOrCancelPointer()
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
        page.SetDefaultTimeout(15_000);
        await page.GotoAsync(
            new Uri(E2EBaseUrl.FromFixture(host.Fixture), "causal-diagram-editor").AbsoluteUri
        );
        await page.GetByRole(AriaRole.Button, new() { Name = "Weiter zum Diagramm" }).ClickAsync();
        await page.Locator(".arrow-target-handle").First.WaitForAsync();

        var scroller = page.Locator(".diagram-scroll");
        await scroller.EvaluateAsync("element => element.scrollLeft = 0");
        var point = await page.EvaluateAsync<ArrowTouchPoints>(
            """
            () => {
              const scroller = document.querySelector('.diagram-scroll');
              const svg = document.querySelector('.diagram-canvas');
              const scroll = scroller.getBoundingClientRect();
              const handles = [...svg.querySelectorAll('.arrow-target-handle')].map(element => {
                const rect = element.getBoundingClientRect();
                return { element, rect, x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
              }).filter(item => item.x >= scroll.left && item.x <= scroll.right
                && item.y >= scroll.top && item.y <= scroll.bottom
                && item.x >= 0 && item.x <= innerWidth && item.y >= 0 && item.y <= innerHeight);
              const targets = [...svg.querySelectorAll('.throw-hit-target')].map(element => {
                const rect = element.getBoundingClientRect();
                return { rect, x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
              }).filter(item => item.x >= scroll.left && item.x <= scroll.right
                && item.y >= scroll.top && item.y <= scroll.bottom
                && item.x >= 0 && item.x <= innerWidth && item.y >= 0 && item.y <= innerHeight);
              for (const handle of handles) {
                const target = targets.find(item => Math.hypot(item.x - handle.x, item.y - handle.y) > 50);
                if (target) return {
                  startX: handle.x, startY: handle.y, endX: target.x, endY: target.y,
                  handleWidth: handle.rect.width, handleHeight: handle.rect.height,
                  edgeId: handle.element.dataset.edgeId,
                  originalX: handle.element.getAttribute('cx'),
                  touchAction: getComputedStyle(handle.element).touchAction
                };
              }
              return null;
            }
            """
        );
        Assert.NotNull(point);
        Assert.Equal("none", point!.TouchAction);
        Assert.True(point.HandleWidth >= 44);
        Assert.True(point.HandleHeight >= 44);

        await page.Locator(".diagram-canvas")
            .EvaluateAsync(
                """
                svg => {
                  window.__arrowDownPrevented = false;
                  window.__arrowCaptured = false;
                  window.__arrowCancels = 0;
                  window.__arrowMoves = 0;
                  window.__arrowEvents = [];
                  svg.addEventListener('pointerdown', event => {
                    if (event.target.matches('.arrow-target-handle')) window.__arrowDownPrevented = event.defaultPrevented;
                  });
                  svg.addEventListener('gotpointercapture', event => window.__arrowCaptured = svg.hasPointerCapture(event.pointerId));
                  for (const name of ['pointerdown', 'pointermove', 'pointerup', 'pointercancel', 'gotpointercapture', 'lostpointercapture']) {
                    document.addEventListener(name, event => window.__arrowEvents.push({ name, pointerType: event.pointerType, target: event.target.tagName, className: event.target.getAttribute('class'), defaultPrevented: event.defaultPrevented }), true);
                  }
                  document.addEventListener('pointercancel', () => window.__arrowCancels++, true);
                  document.addEventListener('pointermove', () => window.__arrowMoves++);
                }
                """
            );
        var scrollLeftBefore = await scroller.EvaluateAsync<double>(
            "element => element.scrollLeft"
        );
        var scrollTopBefore = await page.EvaluateAsync<double>("() => window.scrollY");
        var cdp = await context.NewCDPSessionAsync(page);
        await DispatchTouch(cdp, "touchStart", point.StartX, point.StartY);
        const int steps = 8;
        for (var step = 1; step <= steps; step++)
        {
            var progress = (double)step / steps;
            await DispatchTouch(
                cdp,
                "touchMove",
                point.StartX + (point.EndX - point.StartX) * progress,
                point.StartY + (point.EndY - point.StartY) * progress
            );
        }
        await DispatchTouchEnd(cdp);
        await page.WaitForTimeoutAsync(200);

        Assert.True(await page.EvaluateAsync<bool>("() => window.__arrowDownPrevented"));
        Assert.True(await page.EvaluateAsync<bool>("() => window.__arrowCaptured"));
        var pointerEvents = await page.EvaluateAsync<string>(
            "() => JSON.stringify(window.__arrowEvents)"
        );
        Assert.True(
            await page.EvaluateAsync<int>("() => window.__arrowCancels") == 0,
            pointerEvents
        );
        Assert.True(await page.EvaluateAsync<int>("() => window.__arrowMoves") > 0);
        Assert.Equal(
            scrollLeftBefore,
            await scroller.EvaluateAsync<double>("element => element.scrollLeft")
        );
        Assert.Equal(scrollTopBefore, await page.EvaluateAsync<double>("() => window.scrollY"));
        var finalX = await page.Locator($".arrow-target-handle[data-edge-id='{point.EdgeId}']")
            .First.GetAttributeAsync("cx");
        Assert.NotEqual(point.OriginalX, finalX);
    }

    [Fact]
    public async Task ClickingThrowSelectsItWithoutOpeningEditingActions()
    {
        var page = await host.Fixture.Context!.NewPageAsync();
        page.SetDefaultTimeout(15_000);
        page.PageError += (_, error) => Console.WriteLine($"PAGE ERROR: {error}");
        page.Console += (_, message) =>
            Console.WriteLine($"BROWSER CONSOLE: {message.Type} {message.Text}");
        await page.GotoAsync(
            new Uri(E2EBaseUrl.FromFixture(host.Fixture), "causal-diagram-editor").AbsoluteUri
        );
        await page.GetByRole(AriaRole.Button, new() { Name = "Weiter zum Diagramm" }).ClickAsync();
        await page.Locator(".arrow-target-handle").First.WaitForAsync();
        var throwTarget = page.Locator(".throw-hit-target").First;
        await throwTarget.ClickAsync();

        Assert.True(await page.Locator(".pd-node.selected").CountAsync() > 0);
        Assert.Equal(0, await page.Locator(".throw-delete-action[role=button]").CountAsync());
        Assert.Equal(0, await page.Locator(".node-inspector").CountAsync());
    }

    [Fact]
    public async Task AddingJugglerCreatesOneThrowPerRepetition()
    {
        var page = await host.Fixture.Context!.NewPageAsync();
        page.SetDefaultTimeout(15_000);
        await page.GotoAsync(
            new Uri(E2EBaseUrl.FromFixture(host.Fixture), "causal-diagram-editor").AbsoluteUri
        );
        await page.GetByRole(AriaRole.Button, new() { Name = "Weiter zum Diagramm" }).ClickAsync();
        await page.Locator(".diagram-options summary").ClickAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Jongleur hinzufügen" }).ClickAsync();

        Assert.Equal(3, await page.Locator(".pd-juggler-label").CountAsync());
        Assert.Equal(
            "1",
            await page.Locator(".pd-node-label[data-throw-id='J3-1']").First.TextContentAsync()
        );
        Assert.Equal(0, await page.Locator(".animation-unavailable").CountAsync());
        Assert.False(await page.Locator("#blazor-error-ui").IsVisibleAsync());
        Assert.Contains("state=j3.", page.Url);
    }

    private sealed class ArrowTouchPoints
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public double HandleWidth { get; set; }
        public double HandleHeight { get; set; }
        public string EdgeId { get; set; } = "";
        public string OriginalX { get; set; } = "";
        public string TouchAction { get; set; } = "";
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
}
