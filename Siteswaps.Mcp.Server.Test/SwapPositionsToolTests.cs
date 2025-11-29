using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class SwapPositionsToolTests
{
    [Test]
    public void SwapPositions_With_Valid_Inputs_Returns_Modified_Siteswap()
    {
        // Arrange
        var tool = new SwapPositionsTool();
        var siteswap = "531";
        var position1 = 0;
        var position2 = 1;

        // Act
        var result = tool.SwapPositions(siteswap, position1, position2);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotBe(siteswap); // Should be different after swap
    }

    [Test]
    public void SwapPositions_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SwapPositionsTool();

        // Act & Assert
        var act = () => tool.SwapPositions(string.Empty, 0, 1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void SwapPositions_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SwapPositionsTool();

        // Act & Assert
        var act = () => tool.SwapPositions("43", 0, 1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid siteswap: 43*");
    }

    [Test]
    public void SwapPositions_With_Negative_Position_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SwapPositionsTool();

        // Act & Assert
        var act = () => tool.SwapPositions("531", -1, 1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Position indices must be non-negative.*");
    }
}

