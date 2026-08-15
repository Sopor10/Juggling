using System.Reflection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests;

/// <summary>
/// Mirrors <see cref="BlazorWebassemblyFixture{TProgram}"/> request proxying so additional
/// browser contexts can reach the in-memory test server via <c>http://127.0.0.1/**</c>.
/// </summary>
internal static class BlazorTestServerProxy
{
    private static readonly FieldInfo FactoryField =
        typeof(BlazorWebassemblyFixture<Program>).GetField(
            "_webApplicationFactory",
            BindingFlags.Instance | BindingFlags.NonPublic
        )
        ?? throw new InvalidOperationException(
            "BlazorWebassemblyFixture._webApplicationFactory field was not found."
        );

    public static async Task InstallAsync(
        IBrowserContext context,
        BlazorWebassemblyFixture<Program> fixture
    )
    {
        var factory =
            FactoryField.GetValue(fixture) as WebApplicationFactory<Program>
            ?? throw new InvalidOperationException(
                "BlazorWebassemblyFixture WebApplicationFactory is not initialized."
            );

        // One client per browser context so parallel tests do not share a single handler pipeline.
        var httpClient = factory.CreateClient();
        context.SetDefaultTimeout(60_000);

        var root = fixture.RootUri.AbsoluteUri.TrimEnd('/') + "/";
        await context.RouteAsync(
            root + "**",
            async route =>
            {
                var request = route.Request;
                using var message = new HttpRequestMessage(
                    new HttpMethod(request.Method),
                    request.Url
                );

                if (request.PostDataBuffer is { Length: > 0 } body)
                {
                    message.Content = new ByteArrayContent(body);
                }

                foreach (var header in request.Headers)
                {
                    if (
                        !message.Headers.TryAddWithoutValidation(header.Key, header.Value)
                        && message.Content is not null
                    )
                    {
                        message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                using var response = await httpClient.SendAsync(message);
                var bodyBytes = await response.Content.ReadAsByteArrayAsync();
                var headers = response
                    .Content.Headers.Concat(response.Headers)
                    .Select(h => KeyValuePair.Create(h.Key, string.Join(",", h.Value)));

                await route.FulfillAsync(
                    new RouteFulfillOptions
                    {
                        BodyBytes = bodyBytes,
                        Headers = headers,
                        Status = (int)response.StatusCode,
                    }
                );
            }
        );
    }
}
