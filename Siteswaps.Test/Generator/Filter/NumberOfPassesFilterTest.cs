using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator.Filter;

public class NumberOfPassesFilterTest
{
    [Test]
    [TestCase(new[] { 4, 2, 3 }, 2)]
    [TestCase(new[] { 4, 4, 1 }, 2)]
    [TestCase(new[] { 6, 0, 3 }, 2)]
    [TestCase(new[] { 5, 0, 4 }, 4)]
    [TestCase(new[] { 4, 4, 1 }, 4)]
    public void There_Is_One_Pass(int[] siteswap, int numberOfJugglers)
    {
        var sut = new FilterFactory().ExactNumberOfPassesFilter(1, numberOfJugglers);
        var result = sut.CanFulfill(new PartialSiteswap(siteswap), new SiteswapGeneratorInput(3, 3, 0, 10));

        result.Should().BeTrue();
    }

    [Test]
    [TestCase(new[] { 6, 0, 3 }, 3)]
    [TestCase(new[] { 9, 0, 0 }, 3)]
    public void There_Is_No_Pass(int[] input, int numberOfJugglers)
    {
        var sut = new FilterFactory().ExactNumberOfPassesFilter(1, numberOfJugglers);
        var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 5));

        result.Should().BeFalse();
    }
}