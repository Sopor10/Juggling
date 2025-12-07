using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class NormalizeSiteswapToolTests
{
    [Test]
    public void NormalizeSiteswap_With_Valid_Siteswap_Returns_Normalized()
    {
        // Arrange
        var tool = new NormalizeSiteswapTool();
        var siteswap = "531";

        // Act
        var result = tool.NormalizeSiteswap(siteswap);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void NormalizeSiteswap_With_Rotated_Siteswap_Returns_Same_Normalized()
    {
        // Arrange
        var tool = new NormalizeSiteswapTool();
        var siteswap1 = "441";
        var siteswap2 = "414";

        // Act
        var result1 = tool.NormalizeSiteswap(siteswap1);
        var result2 = tool.NormalizeSiteswap(siteswap2);

        // Assert
        result1.Should().Be(result2);
    }

    [Test]
    public void NormalizeSiteswap_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new NormalizeSiteswapTool();

        // Act & Assert
        var act = () => tool.NormalizeSiteswap(string.Empty);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void NormalizeSiteswap_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new NormalizeSiteswapTool();

        // Act & Assert
        var act = () => tool.NormalizeSiteswap("43");
        act.Should().Throw<ArgumentException>().WithMessage("Invalid siteswap: 43*");
    }
}
