using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Test.Filter;

public class ExactNumberOfPassesFilterTests : FilterTestSuiteBase
{

    [Test]
    [TestCase(new sbyte[] { 4, 2, 3 }, 2)]
    [TestCase(new sbyte[] { 4, 4, 1 }, 2)]
    [TestCase(new sbyte[] { 6, 0, 3 }, 2)]
    [TestCase(new sbyte[] { 5, 0, 4 }, 4)]
    [TestCase(new sbyte[] { 4, 4, 1 }, 4)]
    public void There_Is_One_Pass(sbyte[] siteswap, int numberOfJugglers)
    {
        var sut = FilterBuilder.ExactNumberOfPasses(1, numberOfJugglers).Build();

        sut.CanFulfill(new PartialSiteswap(siteswap)).Should().BeTrue();
    }

    [Test]
    [TestCase(new sbyte[] { 6, 0, 3 }, 3)]
    [TestCase(new sbyte[] { 9, 0, 0 }, 3)]
    public void There_Is_No_Pass(sbyte[] input, int numberOfJugglers)
    {
        var sut = FilterBuilder.ExactNumberOfPasses(1, numberOfJugglers).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeFalse();
    }
}
