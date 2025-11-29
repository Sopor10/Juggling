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
        result.Should().NotBeNull();
        result.OriginalSiteswap.Should().Be(siteswap);
        result.NewSiteswap.Should().NotBeNullOrWhiteSpace();
        result.ThrowValue.Should().BeGreaterThan(0);
        result.StartingState.Should().NotBeNullOrWhiteSpace();
        result.EndingState.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void SimulateThrow_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SimulateThrowTool();

        // Act & Assert
        var act = () => tool.SimulateThrow(string.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void SimulateThrow_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new SimulateThrowTool();

        // Act & Assert
        var act = () => tool.SimulateThrow("43");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid siteswap: 43*");
    }
}

