using Microsoft.Playwright;
using Xunit;

namespace Siteswaps.E2ETests.Functional;

public sealed class CausalDiagramArrowDragTests(ExternalBlazorAppFixture host)
    : IClassFixture<ExternalBlazorAppFixture>
{
    [Fact]
    public async Task DraggingArrowOntoOccupiedThrowMovesItAndKeepsTemporaryGraphState()
    {
        var page = await host.Context.NewPageAsync();
        page.SetDefaultTimeout(15_000);
        await page.GotoAsync(new Uri(host.BaseUri, "causal-diagram-editor").AbsoluteUri);
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
        await page.Mouse.MoveAsync(destination!.X + destination.Width / 2, destination.Y + destination.Height / 2, new MouseMoveOptions { Steps = 8 });
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
    public async Task ThrowInspectorChangesHeightAndOmitsRedundantTargetSelector()
    {
        var page = await host.Context.NewPageAsync();
        page.SetDefaultTimeout(15_000);
        page.PageError += (_, error) => Console.WriteLine($"PAGE ERROR: {error}");
        page.Console += (_, message) => Console.WriteLine($"BROWSER CONSOLE: {message.Type} {message.Text}");
        await page.GotoAsync(new Uri(host.BaseUri, "causal-diagram-editor").AbsoluteUri);
        await page.Locator(".arrow-target-handle").First.WaitForAsync();
        await page.Locator(".throw-hit-target").First.ClickAsync();

        var height = page.GetByLabel("Höhe (Schläge)");
        Assert.Equal("1", await height.InputValueAsync());
        Assert.Equal(0, await page.GetByLabel("Kausaler Zielknoten").CountAsync());
        Assert.Equal(0, await page.GetByText("Animationsgeschwindigkeit", new() { Exact = true }).CountAsync());

        var choices = await height.Locator("option").EvaluateAllAsync<string[]>("items => items.map(item => item.value)");
        Assert.Contains("1", choices);
        var nextHeight = choices.First(value => value != "1");
        await height.SelectOptionAsync(nextHeight);
        Assert.Equal(nextHeight, await height.InputValueAsync());
        Assert.Contains("state=j3.", page.Url);
    }

    [Fact]
    public async Task AddingJugglerCreatesOneThrowPerRepetition()
    {
        var page = await host.Context.NewPageAsync();
        page.SetDefaultTimeout(15_000);
        await page.GotoAsync(new Uri(host.BaseUri, "causal-diagram-editor").AbsoluteUri);

        await page.GetByRole(AriaRole.Button, new() { Name = "Jongleur hinzufügen" }).ClickAsync();

        Assert.Equal(3, await page.Locator(".pd-juggler-label").CountAsync());
        Assert.Equal("true", await page.Locator(".juggler-add-target[data-juggler-id='J3']").GetAttributeAsync("aria-pressed"));
        Assert.Equal("1", await page.Locator(".pd-node-label[data-throw-id='J3-1']").First.TextContentAsync());
        Assert.Equal(0, await page.Locator(".animation-unavailable").CountAsync());
        Assert.False(await page.Locator("#blazor-error-ui").IsVisibleAsync());
        Assert.Contains("state=j3.", page.Url);
    }

}
