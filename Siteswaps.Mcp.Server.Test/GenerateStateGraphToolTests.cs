using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class GenerateStateGraphToolTests
{
    [Test]
    public void GenerateStateGraph_With_Valid_Siteswap_Returns_StateGraph()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var siteswap = "531";

        // Act
        var result = tool.GenerateStateGraph(siteswap);

        // Assert
        result.Should().NotBeNull();
        result.Siteswap.Should().Be(siteswap);
        result.Nodes.Should().NotBeEmpty();
        result.Edges.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateStateGraph_With_441_Returns_Correct_Graph()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var siteswap = "441";

        // Act
        var result = tool.GenerateStateGraph(siteswap);

        // Assert
        result.Should().NotBeNull();
        result.Siteswap.Should().Be(siteswap);
        result.Nodes.Should().NotBeEmpty();
        result.Edges.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateStateGraph_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();

        // Act & Assert
        var act = () => tool.GenerateStateGraph(string.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void GenerateStateGraph_With_Null_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();

        // Act & Assert
        var act = () => tool.GenerateStateGraph(null!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void GenerateStateGraph_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var invalidSiteswap = "43";

        // Act & Assert
        var act = () => tool.GenerateStateGraph(invalidSiteswap);
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid siteswap: {invalidSiteswap}*");
    }

    [Test]
    public void GenerateStateGraph_Returns_Valid_Nodes()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var siteswap = "531";

        // Act
        var result = tool.GenerateStateGraph(siteswap);

        // Assert
        result.Nodes.Should().NotBeEmpty();
        result.Nodes.Should().OnlyContain(n => !string.IsNullOrWhiteSpace(n));
    }

    [Test]
    public void GenerateStateGraph_Returns_Valid_Edges()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var siteswap = "531";

        // Act
        var result = tool.GenerateStateGraph(siteswap);

        // Assert
        result.Edges.Should().NotBeEmpty();
        result.Edges.Should().OnlyContain(e => 
            !string.IsNullOrWhiteSpace(e.FromState) && 
            !string.IsNullOrWhiteSpace(e.ToState) && 
            e.ThrowValue > 0);
    }

    [Test]
    public void GenerateStateGraph_Edges_Reference_Valid_Nodes()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var siteswap = "531";

        // Act
        var result = tool.GenerateStateGraph(siteswap);

        // Assert
        var nodeSet = result.Nodes.ToHashSet();
        result.Edges.Should().OnlyContain(e => 
            nodeSet.Contains(e.FromState) && 
            nodeSet.Contains(e.ToState));
    }

    [Test]
    public void GenerateStateGraph_With_Simple_Siteswap_Returns_Correct_Graph()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var siteswap = "3";

        // Act
        var result = tool.GenerateStateGraph(siteswap);

        // Assert
        result.Should().NotBeNull();
        result.Siteswap.Should().Be(siteswap);
        result.Nodes.Should().NotBeEmpty();
        result.Edges.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateStateGraph_With_Complex_Siteswap_Returns_Correct_Graph()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var siteswap = "97531";

        // Act
        var result = tool.GenerateStateGraph(siteswap);

        // Assert
        result.Should().NotBeNull();
        result.Siteswap.Should().Be(siteswap);
        result.Nodes.Should().NotBeEmpty();
        result.Edges.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateStateGraph_Returns_Unique_Nodes()
    {
        // Arrange
        var tool = new GenerateStateGraphTool();
        var siteswap = "531";

        // Act
        var result = tool.GenerateStateGraph(siteswap);

        // Assert
        result.Nodes.Should().OnlyHaveUniqueItems();
    }
}

