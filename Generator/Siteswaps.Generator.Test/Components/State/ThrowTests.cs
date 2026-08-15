using System.Collections;
using FluentAssertions;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Test.Components.State;

[TestFixture]
public class ThrowTests
{
    [TestCaseSource(typeof(GenerateInputs))]
    public void Should_Calculate_Height_Correctly(Throw @throw, int jugglers, int[] heights)
    {
        @throw.GetHeightForJugglers(jugglers, false).Should().BeEquivalentTo(heights);
    }

    [TestCase(3, 3, "Zip")]
    [TestCase(3, 9, "Self")]
    [TestCase(3, 10, "Single")]
    [TestCase(2, 9, "Double")]
    public void GetDisplayNameForHeight_Uses_Juggler_Scaled_Names(
        int jugglers,
        int height,
        string expected
    )
    {
        Throw.GetDisplayNameForHeight(height, jugglers).Should().Be(expected);
    }

    /// <summary>
    /// Property: for every juggler count and named throw,
    /// throw → GetHeightForJugglers → GetDisplayNameForHeight must yield the same throw.
    /// </summary>
    [Test]
    public void Named_Throw_To_Height_To_Named_Throw_Is_Identity_For_All_Juggler_Counts()
    {
        var namedThrows = new[]
        {
            Throw.EmptyHand,
            Throw.Zip,
            Throw.Three,
            Throw.Hold,
            Throw.Zap,
            Throw.Self,
            Throw.SinglePass,
            Throw.Heff,
            Throw.DoublePass,
            Throw.TripleSelf,
            Throw.TriplePass,
            Throw.Quad,
            Throw.QuadPass,
        };

        foreach (var jugglers in Enumerable.Range(1, 8))
        {
            foreach (var named in namedThrows)
            {
                var heights = named.GetHeightForJugglers(jugglers, useLiteralValue: false).ToList();
                if (heights.Count == 0)
                {
                    // Pass-style (odd) throws have no scaled heights for 1 juggler.
                    continue;
                }

                foreach (var height in heights)
                {
                    Throw
                        .GetDisplayNameForHeight(height, jugglers)
                        .Should()
                        .Be(
                            named.DisplayValue,
                            because: "throw {0} → height {1} → name must round-trip for {2} jugglers",
                            named.DisplayValue,
                            height,
                            jugglers
                        );
                }
            }
        }
    }
}

public class GenerateInputs : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new TestCaseData(Throw.SinglePass, 2, new[] { 7 });
        yield return new TestCaseData(Throw.DoublePass, 2, new[] { 9 });
        yield return new TestCaseData(Throw.SinglePass, 3, new[] { 10, 11 });
        yield return new TestCaseData(Throw.Zip, 2, new[] { 2 });
        yield return new TestCaseData(Throw.Zip, 3, new[] { 3 });
        yield return new TestCaseData(Throw.Zap, 5, new[] { 11, 12, 13, 14 });
    }
}
