using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

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
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
    }

    [Test]
    public async Task GenerateSiteswaps_With_MinOcc_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 2,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            filter: "minOcc(5,1)",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
        foreach (var siteswap in results)
        {
            siteswap.Should().Contain("5");
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_MaxOcc_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 2,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            filter: "maxOcc(5,1)",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
        foreach (var siteswap in results)
        {
            siteswap.Count(c => c == '5').Should().BeLessThanOrEqualTo(1);
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_ExactOcc_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 2,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            filter: "exactOcc(5,1)",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
        foreach (var siteswap in results)
        {
            siteswap.Count(c => c == '5').Should().Be(1);
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Or_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            filter: "minOcc(5,1) OR minOcc(7,1)",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
        foreach (var siteswap in results)
        {
            var has5 = siteswap.Contains('5');
            var has7 = siteswap.Contains('7');
            (has5 || has7).Should().BeTrue();
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_And_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 0,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            filter: "minOcc(5,1) AND noZeros",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
        foreach (var siteswap in results)
        {
            siteswap.Should().Contain("5");
            siteswap.Should().NotContain("0");
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Not_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            filter: "NOT minOcc(5,2)",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
        foreach (var siteswap in results)
        {
            siteswap.Count(c => c == '5').Should().BeLessThan(2);
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Complex_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 0,
            maxHeight: 7,
            maxResults: 20,
            timeoutSeconds: 5,
            filter: "(minOcc(5,1) OR minOcc(7,1)) AND noZeros",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
    }

    [Test]
    public async Task GenerateSiteswaps_With_NoZeros_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 0,
            maxHeight: 7,
            maxResults: 10,
            timeoutSeconds: 5,
            filter: "noZeros",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
        foreach (var siteswap in results)
        {
            siteswap.Should().NotContain("0");
        }
    }

    [Test]
    public async Task GenerateSiteswaps_With_Ground_Filter_Works()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var tool = new GenerateSiteswapsTool();

        // Act
        var results = await tool.GenerateSiteswaps(
            period: 3,
            numberOfObjects: 3,
            minHeight: 1,
            maxHeight: 7,
            maxResults: 10,
            timeoutSeconds: 5,
            filter: "ground",
            cancellationToken: cancellationToken
        );

        // Assert
        results.Should().NotBeEmpty();
    }
}
