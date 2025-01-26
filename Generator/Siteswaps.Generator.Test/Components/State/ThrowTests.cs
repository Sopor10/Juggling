using System.Collections;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
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
