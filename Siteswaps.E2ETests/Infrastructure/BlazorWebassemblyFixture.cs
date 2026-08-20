using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using Xunit;

namespace PlaywrightTesting.Infrastructure;

/// <summary>
/// Hosts a Blazor WASM app via <see cref="WebApplicationFactory{TProgram}"/> and proxies
/// browser traffic through Playwright routing. Kept in-repo so it compiles against the
/// current Microsoft.Playwright API (RouteAsync return type changed in 1.62).
/// </summary>
public sealed class BlazorWebassemblyFixture<TProgram> : IAsyncLifetime, IAsyncDisposable
    where TProgram : class
{
    private readonly PlaywrightFixture _fixture = new();
    private readonly WebApplicationFactory<TProgram> _webApplicationFactory = new();
    private HttpClient? _httpClient;

    public Uri RootUri { get; } = new("http://127.0.0.1");

    public IBrowserContext? Context { get; private set; }

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
        _httpClient = _webApplicationFactory.CreateClient();
        Context = await _fixture.Browser.NewContextAsync();
        await Context.RouteAsync(
            $"{RootUri.AbsoluteUri}**",
            async route =>
            {
                var request = route.Request;
                var content = request.PostDataBuffer is { } postDataBuffer
                    ? new ByteArrayContent(postDataBuffer)
                    : null;
                var requestMessage = new HttpRequestMessage(
                    new HttpMethod(request.Method),
                    request.Url
                )
                {
                    Content = content,
                };
                foreach (var header in request.Headers)
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var response = await _httpClient.SendAsync(requestMessage);
                var responseBody = await response.Content.ReadAsByteArrayAsync();
                var responseHeaders = response.Content.Headers.Select(h =>
                    KeyValuePair.Create(h.Key, string.Join(",", h.Value))
                );
                await route.FulfillAsync(
                    new RouteFulfillOptions
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
        await _fixture.DisposeAsync();
        _httpClient?.Dispose();
        await _webApplicationFactory.DisposeAsync();
    }

    ValueTask IAsyncDisposable.DisposeAsync() => new(DisposeAsync());
}
