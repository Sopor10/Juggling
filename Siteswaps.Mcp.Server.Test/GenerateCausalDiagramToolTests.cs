using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class GenerateCausalDiagramToolTests
{
    [Test]
    public void GenerateCausalDiagram_With_Valid_Siteswap_Returns_Diagram()
    {
        // Arrange
        var tool = new GenerateCausalDiagramTool();
        var siteswap = "531";

        // Act
        var result = tool.GenerateCausalDiagram(siteswap);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Siteswap.Should().Be("5,3,1");
        result.Data.NumberOfHands.Should().Be(2);
        result.Data.Throws.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateCausalDiagram_With_Custom_NumberOfHands_Returns_Diagram()
    {
        // Arrange
        var tool = new GenerateCausalDiagramTool();
        var siteswap = "531";
        var numberOfHands = 4;

        // Act
        var result = tool.GenerateCausalDiagram(siteswap, numberOfHands);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.NumberOfHands.Should().Be(numberOfHands);
        result.Data.Throws.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateCausalDiagram_With_Zero_Hands_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateCausalDiagramTool();

        // Act & Assert
        var result = tool.GenerateCausalDiagram("531", 0);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Number of hands must be at least 1");
    }

    [Test]
    public void GenerateCausalDiagram_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateCausalDiagramTool();

        // Act & Assert
        var result = tool.GenerateCausalDiagram(string.Empty);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be null or empty");
    }
}
