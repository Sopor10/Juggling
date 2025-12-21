using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class CalculateTransitionsToolTests
{
    [Test]
    public void CalculateTransitions_With_Valid_Siteswaps_Returns_Transitions()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();
        var fromSiteswap = "3";
        var toSiteswap = "3";
        var maxLength = 1;

        // Act
        var result = tool.CalculateTransitions(fromSiteswap, toSiteswap, maxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().NotBeEmpty();
        result.Data.Should().OnlyContain(t => t.IsValid);
    }

    [Test]
    public void CalculateTransitions_With_Same_Siteswap_Returns_Empty_Transition()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();
        var siteswap = "531";
        var maxLength = 1;

        // Act
        var result = tool.CalculateTransitions(siteswap, siteswap, maxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().ContainSingle();
        result.Data[0].FromSiteswap.Should().Be(siteswap);
        result.Data[0].ToSiteswap.Should().Be(siteswap);
        result.Data[0].Length.Should().Be(0);
        result.Data[0].Throws.Should().BeEmpty();
        result.Data[0].IsValid.Should().BeTrue();
    }

    [Test]
    public void CalculateTransitions_With_3_To_51_Returns_Valid_Transitions()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();
        var fromSiteswap = "3";
        var toSiteswap = "51";
        var maxLength = 2;

        // Act
        var result = tool.CalculateTransitions(fromSiteswap, toSiteswap, maxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().NotBeEmpty();
        result.Data.Should().OnlyContain(t => t.IsValid);
        result.Data.Should().OnlyContain(t => t.FromSiteswap == fromSiteswap);
        result.Data.Should().OnlyContain(t => t.ToSiteswap == toSiteswap);
        result.Data.Should().OnlyContain(t => t.Length > 0);
    }

    [Test]
    public void CalculateTransitions_With_Empty_FromSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var result = tool.CalculateTransitions(string.Empty, "531", 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Source siteswap cannot be null or empty");
    }

    [Test]
    public void CalculateTransitions_With_Null_FromSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var result = tool.CalculateTransitions(null!, "531", 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Source siteswap cannot be null or empty");
    }

    [Test]
    public void CalculateTransitions_With_Empty_ToSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var result = tool.CalculateTransitions("531", string.Empty, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Target siteswap cannot be null or empty");
    }

    [Test]
    public void CalculateTransitions_With_Null_ToSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var result = tool.CalculateTransitions("531", null!, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Target siteswap cannot be null or empty");
    }

    [Test]
    public void CalculateTransitions_With_Invalid_FromSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var result = tool.CalculateTransitions("43", "531", 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Invalid source siteswap: 43");
    }

    [Test]
    public void CalculateTransitions_With_Invalid_ToSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var result = tool.CalculateTransitions("531", "43", 1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Invalid target siteswap: 43");
    }

    [Test]
    public void CalculateTransitions_With_Different_Number_Of_Objects_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var result = tool.CalculateTransitions("3", "4444", 2);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("same number of objects");
    }

    [Test]
    public void CalculateTransitions_With_Negative_MaxLength_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var result = tool.CalculateTransitions("531", "441", -1);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Maximum transition length must be non-negative");
    }

    [Test]
    public void CalculateTransitions_With_MaxHeight_Respects_Limit()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();
        var fromSiteswap = "531";
        var toSiteswap = "441";
        var maxLength = 2;
        var maxHeight = 4;

        // Act
        var result = tool.CalculateTransitions(fromSiteswap, toSiteswap, maxLength, maxHeight);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().OnlyContain(t => t.Throws.All(th => th.Value <= maxHeight));
    }

    [Test]
    public void CalculateTransitions_Returns_Valid_ThrowInfo()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();
        var fromSiteswap = "3";
        var toSiteswap = "51";
        var maxLength = 2;

        // Act
        var result = tool.CalculateTransitions(fromSiteswap, toSiteswap, maxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var transitionsWithThrows = result.Data!.Where(t => t.Throws.Any()).ToList();
        transitionsWithThrows.Should().NotBeEmpty();
        transitionsWithThrows
            .Should()
            .OnlyContain(t =>
                t.Throws.All(th =>
                    th.Value > 0
                    && !string.IsNullOrWhiteSpace(th.StartingState)
                    && !string.IsNullOrWhiteSpace(th.EndingState)
                )
            );
    }

    [Test]
    public void CalculateTransitions_Returns_PrettyPrint_For_Each_Transition()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();
        var fromSiteswap = "531";
        var toSiteswap = "441";
        var maxLength = 2;

        // Act
        var result = tool.CalculateTransitions(fromSiteswap, toSiteswap, maxLength);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().OnlyContain(t => !string.IsNullOrWhiteSpace(t.PrettyPrint));
    }
}
