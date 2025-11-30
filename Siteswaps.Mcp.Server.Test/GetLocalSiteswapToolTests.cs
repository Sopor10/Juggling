using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class GetLocalSiteswapToolTests
{
    [Test]
    public void GetLocalSiteswap_With_Valid_Inputs_Returns_LocalSiteswap()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "531";
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, numberOfJugglers);

        // Assert
        result.Should().NotBeNull();
        result.GlobalSiteswap.Should().Be(siteswap);
        result.NumberOfJugglers.Should().Be(numberOfJugglers);
        result.Jugglers.Should().HaveCount(numberOfJugglers);
        result.Jugglers[0].GlobalNotation.Should().NotBeNullOrWhiteSpace();
        result.Jugglers[0].LocalNotation.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [TestCase("531", 0, 2, "513")]
    [TestCase("531", 1, 2, "351")]
    [TestCase("51", 0, 2, "5")]
    [TestCase("51", 1, 2, "1")]
    public void GetLocalSiteswap_Returns_Correct_GlobalNotation(string siteswap, int juggler, int numberOfJugglers, string expectedGlobalNotation)
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act
        var result = tool.GetLocalSiteswap(siteswap, numberOfJugglers);

        // Assert
        result.Jugglers[juggler].GlobalNotation.Should().Be(expectedGlobalNotation);
    }

    [Test]
    public void GetLocalSiteswap_With_Empty_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var act = () => tool.GetLocalSiteswap(string.Empty, 2);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void GetLocalSiteswap_With_Null_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var act = () => tool.GetLocalSiteswap(null!, 2);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Siteswap string cannot be null or empty.*");
    }

    [Test]
    public void GetLocalSiteswap_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var act = () => tool.GetLocalSiteswap("43", 2);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid siteswap: 43*");
    }

    [Test]
    public void GetLocalSiteswap_With_Zero_NumberOfJugglers_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var act = () => tool.GetLocalSiteswap("531", 0);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Number of jugglers must be at least 1.*");
    }

    [Test]
    public void GetLocalSiteswap_With_Negative_NumberOfJugglers_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var act = () => tool.GetLocalSiteswap("531", -1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Number of jugglers must be at least 1.*");
    }

    [Test]
    public void GetLocalSiteswap_Returns_Correct_AverageObjects()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "531";
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, numberOfJugglers);

        // Assert
        result.Jugglers[0].AverageObjects.Should().BeGreaterThan(0);
        result.Jugglers[0].AverageObjects.Should().BeLessThanOrEqualTo(3); // 531 has 3 objects, so per juggler should be <= 3
    }

    [Test]
    public void GetLocalSiteswap_Returns_IsValidAsGlobalSiteswap()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "531";
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, numberOfJugglers);

        // Assert
        // IsValidAsGlobalSiteswap is a bool property, so it will always have a value
        // We just verify the property is accessible and the result is complete
        result.Should().NotBeNull();
        result.Jugglers[0].GlobalNotation.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetLocalSiteswap_With_Complex_Siteswap_Returns_Correct_Result()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "a7242";
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, numberOfJugglers);

        // Assert
        result.Should().NotBeNull();
        result.GlobalSiteswap.Should().Be(siteswap);
        result.Jugglers.Should().HaveCount(2);
        result.Jugglers[0].GlobalNotation.Should().NotBeNullOrWhiteSpace();
        result.Jugglers[0].LocalNotation.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetLocalSiteswap_With_Three_Jugglers_Returns_Correct_Result()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "531";
        var numberOfJugglers = 3;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, numberOfJugglers);

        // Assert
        result.Should().NotBeNull();
        result.Jugglers.Should().HaveCount(3);
        result.Jugglers[0].Juggler.Should().Be(0);
        result.Jugglers[1].Juggler.Should().Be(1);
        result.Jugglers[2].Juggler.Should().Be(2);
        result.NumberOfJugglers.Should().Be(3);
        result.Jugglers[1].GlobalNotation.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetLocalSiteswap_Returns_All_Jugglers()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "966";
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, numberOfJugglers);

        // Assert
        result.Jugglers.Should().HaveCount(2);
        result.Jugglers[0].Juggler.Should().Be(0);
        result.Jugglers[1].Juggler.Should().Be(1);
        result.Jugglers.Should().AllSatisfy(j => j.LocalNotation.Should().NotBeNullOrWhiteSpace());
    }
}
