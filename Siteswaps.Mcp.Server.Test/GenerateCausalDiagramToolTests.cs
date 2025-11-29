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
        result.Should().NotBeNull();
        result.Siteswap.Should().Be(siteswap);
        result.NumberOfHands.Should().Be(2);
        result.Throws.Should().NotBeEmpty();
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
        result.Should().NotBeNull();
        result.NumberOfHands.Should().Be(numberOfHands);
        result.Throws.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateCausalDiagram_With_Zero_Hands_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateCausalDiagramTool();

        // Act & Assert
        var act = () => tool.GenerateCausalDiagram("531", 0);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Number of hands must be at least 1.*");
    }

    [Test]
    public void GenerateCausalDiagram_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateCausalDiagramTool();

        // Act & Assert
        var act = () => tool.GenerateCausalDiagram(string.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }
}

