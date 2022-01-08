using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Generator.Api.Test.Filter;

public abstract partial class FilterTestSuite
{

    [Test]
    [TestCase(new[] { 4, 2, 3 }, 2)]
    [TestCase(new[] { 4, 4, 1 }, 2)]
    [TestCase(new[] { 6, 0, 3 }, 2)]
    [TestCase(new[] { 5, 0, 4 }, 4)]
    [TestCase(new[] { 4, 4, 1 }, 4)]
    public void There_Is_One_Pass(int[] siteswap, int numberOfJugglers)
    {
        var sut = FilterBuilder.ExactNumberOfPasses(1, numberOfJugglers).Build();

        sut.CanFulfill(AsPartialSiteswap(siteswap)).Should().BeTrue();
    }

    [Test]
    [TestCase(new[] { 6, 0, 3 }, 3)]
    [TestCase(new[] { 9, 0, 0 }, 3)]
    public void There_Is_No_Pass(int[] input, int numberOfJugglers)
    {
        var sut = FilterBuilder.ExactNumberOfPasses(1, numberOfJugglers).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeFalse();
    }
}