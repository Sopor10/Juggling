using FluentAssertions;
using NUnit.Framework;
using Siteswap.Details;

namespace Siteswaps.Test;

[TestFixture]
public class PassingThrowNamesTest
{
    [TestCase(2, 2, "Zip")]
    [TestCase(2, 4, "Hold")]
    [TestCase(2, 6, "Self")]
    [TestCase(2, 7, "Single")]
    [TestCase(2, 9, "Double")]
    [TestCase(3, 3, "Zip")]
    [TestCase(3, 6, "Hold")]
    [TestCase(3, 9, "Self")]
    [TestCase(3, 10, "Single")]
    [TestCase(3, 11, "Single")]
    [TestCase(3, 12, "Heff")]
    public void Format_Returns_Named_Throw_For_Scaled_Height(
        int jugglers,
        int height,
        string expected
    )
    {
        PassingThrowNames.Format(height, jugglers).Should().Be(expected);
    }

    [Test]
    public void Format_Falls_Back_To_Siteswap_Digit_When_Unknown()
    {
        PassingThrowNames.Format(1, 2).Should().Be("1");
        PassingThrowNames.Format(15, 2).Should().Be("f");
    }

    [TestCase(2, 2, new[] { 2 })]
    [TestCase(3, 2, new[] { 3 })]
    [TestCase(3, 7, new[] { 10, 11 })]
    [TestCase(5, 5, new[] { 11, 12, 13, 14 })]
    public void HeightsFor_Matches_Known_Scaling(int jugglers, int baseHeight, int[] expected)
    {
        PassingThrowNames.HeightsFor(baseHeight, jugglers).Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Format_Is_Inverse_Of_HeightsFor_For_Catalog()
    {
        foreach (var jugglers in new[] { 2, 3, 4, 5 })
        {
            foreach (var baseHeight in new[] { 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 })
            {
                foreach (var height in PassingThrowNames.HeightsFor(baseHeight, jugglers))
                {
                    var name = PassingThrowNames.Format(height, jugglers);
                    var roundTrip = PassingThrowNames.HeightsFor(
                        CatalogBaseHeight(name),
                        jugglers
                    );
                    roundTrip.Should().Contain(height);
                }
            }
        }
    }

    private static int CatalogBaseHeight(string display) =>
        display switch
        {
            "0" => 0,
            "Zip" => 2,
            "3" => 3,
            "Hold" => 4,
            "Zap" => 5,
            "Self" => 6,
            "Single" => 7,
            "Heff" => 8,
            "Double" => 9,
            "Triple S" => 10,
            "Triple" => 11,
            "Quad" => 12,
            "Quad Pass" => 13,
            _ => throw new ArgumentOutOfRangeException(nameof(display), display, null),
        };
}
