using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class SimulateThrowToolTests
{
    [Test]
    public void SimulateThrow_With_Valid_Siteswap_Returns_Result()
    {
        // Arrange
        var tool = new SimulateThrowTool();
        var siteswap = "531";

        // Act
        var result = tool.SimulateThrow(siteswap);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.OriginalSiteswap.Should().Be(siteswap);
        result.Data.NewSiteswap.Should().NotBeNullOrWhiteSpace();
        result.Data.ThrowValue.Should().BeGreaterThan(0);
        result.Data.StartingState.Should().NotBeNullOrWhiteSpace();
        result.Data.EndingState.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void SimulateThrow_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SimulateThrowTool();

        // Act & Assert
        var result = tool.SimulateThrow(string.Empty);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be null or empty");
    }

    [Test]
    public void SimulateThrow_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SimulateThrowTool();

        // Act & Assert
        var result = tool.SimulateThrow("43");
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Invalid siteswap: 43");
    }
}
