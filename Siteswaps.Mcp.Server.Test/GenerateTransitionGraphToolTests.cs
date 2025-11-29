using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class GenerateTransitionGraphToolTests
{
    [Test]
    public void GenerateTransitionGraph_With_Valid_Inputs_Returns_Graph()
    {
        // Arrange
        var tool = new GenerateTransitionGraphTool();
        var siteswaps = "531,441";
        var maxLength = 2;

        // Act
        var result = tool.GenerateTransitionGraph(siteswaps, maxLength);

        // Assert
        result.Should().NotBeNull();
        result.Siteswaps.Should().Be(siteswaps);
        result.MaxLength.Should().Be(maxLength);
        result.Nodes.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateTransitionGraph_With_Single_Siteswap_Returns_Graph()
    {
        // Arrange
        var tool = new GenerateTransitionGraphTool();
        var siteswaps = "531";
        var maxLength = 1;

        // Act
        var result = tool.GenerateTransitionGraph(siteswaps, maxLength);

        // Assert
        result.Should().NotBeNull();
        // With a single siteswap, there are no transitions to other siteswaps, so nodes may be empty
        // This is expected behavior - the graph only contains nodes when there are transitions
        result.MaxLength.Should().Be(maxLength);
    }

    [Test]
    public void GenerateTransitionGraph_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateTransitionGraphTool();

        // Act & Assert
        var act = () => tool.GenerateTransitionGraph(string.Empty, 1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswaps string cannot be null or empty.*");
    }

    [Test]
    public void GenerateTransitionGraph_With_Zero_MaxLength_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateTransitionGraphTool();

        // Act & Assert
        var act = () => tool.GenerateTransitionGraph("531", 0);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Maximum transition length must be at least 1.*");
    }

    [Test]
    public void GenerateTransitionGraph_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateTransitionGraphTool();

        // Act & Assert
        var act = () => tool.GenerateTransitionGraph("43", 1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid siteswap: 43*");
    }
}

