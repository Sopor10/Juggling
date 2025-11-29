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
        result.Should().NotBeNull();
        result.Siteswap.Should().Be("531");
        result.Period.Should().Be(3);
        result.NumberOfObjects.Should().Be(3);
        result.Length.Should().Be(3);
        result.Orbits.Should().NotBeEmpty();
        result.AllStates.Should().NotBeEmpty();
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
        result.Should().NotBeNull();
        result.Siteswap.Should().Be("441");
        result.Period.Should().Be(3);
        result.NumberOfObjects.Should().BeApproximately(3, 0.01m);
        result.Length.Should().Be(3);
        result.MaxHeight.Should().BeGreaterThan(0);
    }

    [Test]
    public void AnalyzeSiteswap_With_Empty_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();

        // Act & Assert
        var act = () => tool.AnalyzeSiteswap(string.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void AnalyzeSiteswap_With_Null_String_Throws_ArgumentException()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();

        // Act & Assert
        var act = () => tool.AnalyzeSiteswap(null!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void AnalyzeSiteswap_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new AnalyzeSiteswapTool();
        var invalidSiteswap = "43";

        // Act & Assert
        var act = () => tool.AnalyzeSiteswap(invalidSiteswap);
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid siteswap: {invalidSiteswap}*");
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
        result.Orbits.Should().NotBeEmpty();
        result.Orbits.Should().OnlyContain(o => !string.IsNullOrWhiteSpace(o.DisplayValue));
        result.Orbits.Should().OnlyContain(o => o.Items.Any());
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
        result.AllStates.Should().NotBeEmpty();
        result.AllStates.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.State));
        result.AllStates.Should().OnlyContain(s => s.Siteswaps.Any());
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
        result.CurrentState.Should().NotBeNullOrWhiteSpace();
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
        result.Should().NotBeNull();
        result.Siteswap.Should().Be("97531");
        result.Period.Should().Be(5);
        result.Length.Should().Be(5);
        result.MaxHeight.Should().Be(9);
    }
}

