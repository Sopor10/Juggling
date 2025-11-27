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

        // Act
            var tool = new GenerateSiteswapsTool();
            var results = await tool.GenerateSiteswaps(
                period: 3,
                numberOfObjects: 3,
                minHeight: 2,
                maxHeight: 5,
                maxResults: 10,
                timeoutSeconds: 5,
                minOccurrence: null,
                maxOccurrence: null,
                exactOccurrence: null,
                numberOfPasses: null,
                numberOfJugglers: null,
                pattern: null,
                state: null,
                flexiblePattern: null,
                useDefaultFilter: true,
                useNoFilter: false,
                jugglerIndex: null,
                rotationAwarePattern: null,
                personalizedNumberFilter: null,
                cancellationToken);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }
}
