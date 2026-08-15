using FluentAssertions;
using NUnit.Framework;
using Siteswap.Details;

namespace Siteswaps.Test;

[TestFixture]
public class PassingThrowNamesTest
{
    private static readonly (string Name, int BaseHeight)[] Catalog =
    [
        ("0", 0),
        ("Zip", 2),
        ("3", 3),
        ("Hold", 4),
        ("Zap", 5),
        ("Self", 6),
        ("Single", 7),
        ("Heff", 8),
        ("Double", 9),
        ("Triple S", 10),
        ("Triple", 11),
        ("Quad", 12),
        ("Quad Pass", 13),
    ];

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

    /// <summary>
    /// Property: for every juggler count and catalog throw,
    /// throw → HeightsFor → Format must yield the same throw name.
    /// (Odd/pass bases with 1 juggler yield no heights — skipped.)
    /// </summary>
    [Test]
    public void Named_Throw_To_Height_To_Named_Throw_Is_Identity_For_All_Juggler_Counts()
    {
        foreach (var jugglers in Enumerable.Range(1, 8))
        {
            foreach (var (name, baseHeight) in Catalog)
            {
                var heights = PassingThrowNames.HeightsFor(baseHeight, jugglers);
                if (heights.Count == 0)
                {
                    // Odd/pass bases yield no heights for 1 juggler (every int is a "self").
                    continue;
                }

                foreach (var height in heights)
                {
                    PassingThrowNames
                        .Format(height, jugglers)
                        .Should()
                        .Be(
                            name,
                            because: "throw {0} → height {1} → Format must round-trip for {2} jugglers",
                            name,
                            height,
                            jugglers
                        );
                }
            }
        }
    }
}
