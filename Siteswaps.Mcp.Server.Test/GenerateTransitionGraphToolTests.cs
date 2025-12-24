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
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Siteswaps.Should().Be(siteswaps);
        result.Data.MaxLength.Should().Be(maxLength);
        result.Data.Nodes.Should().NotBeEmpty();
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
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.MaxLength.Should().Be(maxLength);
    }

    [Test]
    public void GenerateTransitionGraph_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateTransitionGraphTool();

        // Act & Assert
        var result = tool.GenerateTransitionGraph(string.Empty, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Siteswaps string cannot be null or empty");
    }

    [Test]
    public void GenerateTransitionGraph_With_Zero_MaxLength_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateTransitionGraphTool();

        // Act & Assert
        var result = tool.GenerateTransitionGraph("531", 0);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Maximum transition length must be at least 1");
    }

    [Test]
    public void GenerateTransitionGraph_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GenerateTransitionGraphTool();

        // Act & Assert
        var result = tool.GenerateTransitionGraph("43", 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Invalid siteswap: 43");
    }
}
