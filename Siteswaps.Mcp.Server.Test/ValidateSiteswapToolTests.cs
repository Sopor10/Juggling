using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class ValidateSiteswapToolTests
{
    [Test]
    [TestCase("531", true)]
    [TestCase("441", true)]
    [TestCase("3", true)]
    [TestCase("97531", true)]
    [TestCase("7566", true)]
    [TestCase("aaa00", true)]
    [TestCase("43", false)]
    [TestCase("21", false)]
    [TestCase("", false)]
    [TestCase("   ", false)]
    public void ValidateSiteswap_With_Valid_And_Invalid_Inputs_Returns_Correct_Result(
        string siteswap,
        bool expectedResult
    )
    {
        // Arrange
        var tool = new ValidateSiteswapTool();

        // Act
        var result = tool.ValidateSiteswap(siteswap);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public void ValidateSiteswap_With_Null_Input_Returns_False()
    {
        // Arrange
        var tool = new ValidateSiteswapTool();

        // Act
        var result = tool.ValidateSiteswap(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void ValidateSiteswap_With_Valid_Siteswap_Returns_True()
    {
        // Arrange
        var tool = new ValidateSiteswapTool();
        var validSiteswap = "531";

        // Act
        var result = tool.ValidateSiteswap(validSiteswap);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ValidateSiteswap_With_Invalid_Siteswap_Returns_False()
    {
        // Arrange
        var tool = new ValidateSiteswapTool();
        var invalidSiteswap = "43";

        // Act
        var result = tool.ValidateSiteswap(invalidSiteswap);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void ValidateSiteswap_With_Empty_String_Returns_False()
    {
        // Arrange
        var tool = new ValidateSiteswapTool();

        // Act
        var result = tool.ValidateSiteswap(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void ValidateSiteswap_With_Whitespace_Only_Returns_False()
    {
        // Arrange
        var tool = new ValidateSiteswapTool();

        // Act
        var result = tool.ValidateSiteswap("   ");

        // Assert
        result.Should().BeFalse();
    }
}
