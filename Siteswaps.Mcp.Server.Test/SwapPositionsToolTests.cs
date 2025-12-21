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
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrWhiteSpace();
        result.Data.Should().NotBe(siteswap); // Should be different after swap
    }

    [Test]
    public void SwapPositions_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SwapPositionsTool();

        // Act & Assert
        var result = tool.SwapPositions(string.Empty, 0, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be null or empty");
    }

    [Test]
    public void SwapPositions_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SwapPositionsTool();

        // Act & Assert
        var result = tool.SwapPositions("43", 0, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Invalid siteswap: 43");
    }

    [Test]
    public void SwapPositions_With_Negative_Position_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SwapPositionsTool();

        // Act & Assert
        var result = tool.SwapPositions("531", -1, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("non-negative");
    }

    [Test]
    public void SwapPositions_With_Position_Out_Of_Range_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SwapPositionsTool();

        // Act & Assert
        var result = tool.SwapPositions("531", 0, 99);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("position2 (99) is out of range");
    }
}
