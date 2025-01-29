using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;

namespace Siteswaps.E2ETests;

public class BlazorTest : IAsyncLifetime
{
    private PlaywrightFixture Fixture { get; } = new();

    public readonly Uri RootUri = new("http://127.0.0.1");
    private readonly WebApplicationFactory<Siteswaps.E2ETests.Server.Program> _webApplicationFactory = new();
    private HttpClient? _httpClient;
    public IBrowserContext? Context { get; private set; }
    

    public async Task InitializeAsync()
    {
        await Fixture.InitializeAsync();
        _httpClient = _webApplicationFactory.CreateClient();
        Context = await Fixture.Browser.NewContextAsync();
        await Context.RouteAsync(
            $"{RootUri.AbsoluteUri}**", async route =>
            {
                var request = route.Request;
                var content = request.PostDataBuffer is { } postDataBuffer
                    ? new ByteArrayContent(postDataBuffer)
                    : null;
                var requestMessage = new HttpRequestMessage(new(request.Method), request.Url)
                {
                    Content = content,
                };
                foreach (var header in request.Headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
                var response = await _httpClient.SendAsync(requestMessage);
                var responseBody = await response.Content.ReadAsByteArrayAsync();
                var responseHeaders =
                    response.Content.Headers.Select(h => KeyValuePair.Create(h.Key, string.Join(",", h.Value)));
                await route.FulfillAsync(
                    new()
                    {
                        BodyBytes = responseBody,
                        Headers = responseHeaders,
                        Status = (int)response.StatusCode,
                    }
                );
            }
        );
    }

    public async Task DisposeAsync()
    {
        await Fixture.DisposeAsync();
        _httpClient?.Dispose();
    }
}