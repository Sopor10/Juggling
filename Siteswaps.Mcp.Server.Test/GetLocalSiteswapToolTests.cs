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
        var juggler = 0;
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, juggler, numberOfJugglers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.GlobalSiteswap.Should().Be(siteswap);
        result.Data.Juggler.Should().Be(juggler);
        result.Data.NumberOfJugglers.Should().Be(numberOfJugglers);
        result.Data.GlobalNotation.Should().NotBeNullOrWhiteSpace();
        result.Data.LocalNotation.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [TestCase("531", 0, 2, "513")]
    [TestCase("531", 1, 2, "351")]
    [TestCase("51", 0, 2, "5")]
    [TestCase("51", 1, 2, "1")]
    public void GetLocalSiteswap_Returns_Correct_GlobalNotation(
        string siteswap,
        int juggler,
        int numberOfJugglers,
        string expectedGlobalNotation
    )
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act
        var result = tool.GetLocalSiteswap(siteswap, juggler, numberOfJugglers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.GlobalNotation.Should().Be(expectedGlobalNotation);
    }

    [Test]
    public void GetLocalSiteswap_With_Empty_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var result = tool.GetLocalSiteswap(string.Empty, 0, 2);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be null or empty");
    }

    [Test]
    public void GetLocalSiteswap_With_Null_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var result = tool.GetLocalSiteswap(null!, 0, 2);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be null or empty");
    }

    [Test]
    public void GetLocalSiteswap_With_Invalid_Siteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var result = tool.GetLocalSiteswap("43", 0, 2);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Invalid siteswap: 43");
    }

    [Test]
    public void GetLocalSiteswap_With_Negative_Juggler_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var result = tool.GetLocalSiteswap("531", -1, 2);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Juggler index must be non-negative");
    }

    [Test]
    public void GetLocalSiteswap_With_Zero_NumberOfJugglers_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var result = tool.GetLocalSiteswap("531", 0, 0);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Number of jugglers must be at least 1");
    }

    [Test]
    public void GetLocalSiteswap_With_Negative_NumberOfJugglers_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var result = tool.GetLocalSiteswap("531", 0, -1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Number of jugglers must be at least 1");
    }

    [Test]
    public void GetLocalSiteswap_With_Juggler_Index_Too_High_Throws_ArgumentException()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();

        // Act & Assert
        var result = tool.GetLocalSiteswap("531", 2, 2);
        result.IsSuccess.Should().BeFalse();
        result
            .Error!.Message.Should()
            .Contain("Juggler index (2) must be less than number of jugglers (2)");
    }

    [Test]
    public void GetLocalSiteswap_Returns_Correct_AverageObjectsPerJuggler()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "531";
        var juggler = 0;
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, juggler, numberOfJugglers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.AverageObjectsPerJuggler.Should().BeGreaterThan(0);
        result.Data.AverageObjectsPerJuggler.Should().BeLessThanOrEqualTo(3); // 531 has 3 objects, so per juggler should be <= 3
    }

    [Test]
    public void GetLocalSiteswap_Returns_IsValidAsGlobalSiteswap()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "531";
        var juggler = 0;
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, juggler, numberOfJugglers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.GlobalNotation.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetLocalSiteswap_With_Complex_Siteswap_Returns_Correct_Result()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "a7242";
        var juggler = 0;
        var numberOfJugglers = 2;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, juggler, numberOfJugglers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.GlobalSiteswap.Should().Be(siteswap);
        result.Data.GlobalNotation.Should().NotBeNullOrWhiteSpace();
        result.Data.LocalNotation.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetLocalSiteswap_With_Three_Jugglers_Returns_Correct_Result()
    {
        // Arrange
        var tool = new GetLocalSiteswapTool();
        var siteswap = "531";
        var juggler = 1;
        var numberOfJugglers = 3;

        // Act
        var result = tool.GetLocalSiteswap(siteswap, juggler, numberOfJugglers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Juggler.Should().Be(1);
        result.Data.NumberOfJugglers.Should().Be(3);
        result.Data.GlobalNotation.Should().NotBeNullOrWhiteSpace();
    }
}
