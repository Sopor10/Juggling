using FluentAssertions;
using MCP.SiteswapGenerator.Tools;

namespace MCP.SiteswapGenerator.Test;

public class GenerateSiteswapsToolTests
{
    [Test]
    public async Task GenerateSiteswaps_With_Valid_Parameters_Returns_Siteswaps()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var results = new List<string>();

        // Act
        await foreach (var siteswap in GenerateSiteswapsTool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 2,
            maxHeight: 5,
            maxResults: 10,
            timeoutSeconds: 5,
            cancellationToken))
        {
            results.Add(siteswap);
        }

        // Assert
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }
}
