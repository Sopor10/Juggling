using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class AnalyzeSiteswapToolTests
{
    [Test]
    public void AnalyzeSiteswap_With_Valid_Siteswap_Returns_Analysis()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();
        var validSiteswap = "531";

        // Act
        var result = tool.AnalyzeSiteswap(validSiteswap);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Siteswap.Should().Be("531");
        result.Data.Period.Should().Be(3);
        result.Data.NumberOfObjects.Should().Be(3);
        result.Data.Length.Should().Be(3);
        result.Data.Orbits.Should().NotBeEmpty();
        result.Data.AllStates.Should().NotBeEmpty();
    }

    [Test]
    public void AnalyzeSiteswap_With_441_Returns_Correct_Analysis()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();
        var siteswap = "441";

        // Act
        var result = tool.AnalyzeSiteswap(siteswap);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Siteswap.Should().Be("441");
        result.Data.Period.Should().Be(3);
        result.Data.NumberOfObjects.Should().BeApproximately(3, 0.01m);
        result.Data.Length.Should().Be(3);
        result.Data.MaxHeight.Should().BeGreaterThan(0);
    }

    [Test]
    public void AnalyzeSiteswap_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();

        // Act & Assert
        var result = tool.AnalyzeSiteswap(string.Empty);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be null or empty");
    }

    [Test]
    public void AnalyzeSiteswap_With_Null_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();

        // Act & Assert
        var result = tool.AnalyzeSiteswap(null!);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be null or empty");
    }

    [Test]
    public void AnalyzeSiteswap_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();
        var invalidSiteswap = "43";

        // Act & Assert
        var result = tool.AnalyzeSiteswap(invalidSiteswap);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain($"Invalid siteswap: {invalidSiteswap}");
    }

    [Test]
    public void AnalyzeSiteswap_Returns_Orbits()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();
        var siteswap = "531";

        // Act
        var result = tool.AnalyzeSiteswap(siteswap);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Orbits.Should().NotBeEmpty();
        result.Data.Orbits.Should().OnlyContain(o => !string.IsNullOrWhiteSpace(o.DisplayValue));
        result.Data.Orbits.Should().OnlyContain(o => o.Items.Any());
    }

    [Test]
    public void AnalyzeSiteswap_Returns_AllStates()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();
        var siteswap = "531";

        // Act
        var result = tool.AnalyzeSiteswap(siteswap);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.AllStates.Should().NotBeEmpty();
        result.Data.AllStates.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.State));
        result.Data.AllStates.Should().OnlyContain(s => s.Siteswaps.Any());
    }

    [Test]
    public void AnalyzeSiteswap_Returns_CurrentState()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();
        var siteswap = "531";

        // Act
        var result = tool.AnalyzeSiteswap(siteswap);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CurrentState.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void AnalyzeSiteswap_With_Complex_Siteswap_Returns_Correct_Analysis()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();
        var siteswap = "97531";

        // Act
        var result = tool.AnalyzeSiteswap(siteswap);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Siteswap.Should().Be("97531");
        result.Data.Period.Should().Be(5);
        result.Data.Length.Should().Be(5);
        result.Data.MaxHeight.Should().Be(9);
    }
}
