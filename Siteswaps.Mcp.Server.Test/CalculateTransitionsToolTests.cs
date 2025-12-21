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
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(t => t.IsValid);
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
        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result[0].FromSiteswap.Should().Be(siteswap);
        result[0].ToSiteswap.Should().Be(siteswap);
        result[0].Length.Should().Be(0);
        result[0].Throws.Should().BeEmpty();
        result[0].IsValid.Should().BeTrue();
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
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(t => t.IsValid);
        result.Should().OnlyContain(t => t.FromSiteswap == fromSiteswap);
        result.Should().OnlyContain(t => t.ToSiteswap == toSiteswap);
        result.Should().OnlyContain(t => t.Length > 0);
    }

    [Test]
    public void CalculateTransitions_With_Empty_FromSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var act = () => tool.CalculateTransitions(string.Empty, "531", 1);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Source siteswap cannot be null or empty.*");
    }

    [Test]
    public void CalculateTransitions_With_Null_FromSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var act = () => tool.CalculateTransitions(null!, "531", 1);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Source siteswap cannot be null or empty.*");
    }

    [Test]
    public void CalculateTransitions_With_Empty_ToSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var act = () => tool.CalculateTransitions("531", string.Empty, 1);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Target siteswap cannot be null or empty.*");
    }

    [Test]
    public void CalculateTransitions_With_Null_ToSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var act = () => tool.CalculateTransitions("531", null!, 1);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Target siteswap cannot be null or empty.*");
    }

    [Test]
    public void CalculateTransitions_With_Invalid_FromSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var act = () => tool.CalculateTransitions("43", "531", 1);
        act.Should().Throw<ArgumentException>().WithMessage("Invalid source siteswap: 43*");
    }

    [Test]
    public void CalculateTransitions_With_Invalid_ToSiteswap_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var act = () => tool.CalculateTransitions("531", "43", 1);
        act.Should().Throw<ArgumentException>().WithMessage("Invalid target siteswap: 43*");
    }

    [Test]
    public void CalculateTransitions_With_Different_Number_Of_Objects_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var act = () => tool.CalculateTransitions("3", "4444", 2);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Source and target must use the same number of objects*");
    }

    [Test]
    public void CalculateTransitions_With_Negative_MaxLength_Throws_ArgumentException()
    {
        // Arrange
        var tool = new CalculateTransitionsTool();

        // Act & Assert
        var act = () => tool.CalculateTransitions("531", "441", -1);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Maximum transition length must be non-negative.*");
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
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.Throws.All(th => th.Value <= maxHeight));
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
        result.Should().NotBeNull();
        var transitionsWithThrows = result.Where(t => t.Throws.Any()).ToList();
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
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => !string.IsNullOrWhiteSpace(t.PrettyPrint));
    }
}
