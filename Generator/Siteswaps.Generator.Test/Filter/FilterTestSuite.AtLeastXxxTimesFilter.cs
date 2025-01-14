using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Test.Filter;

public partial class FilterTestSuite
{
    [Test]
    [TestCase(new[] { 8, 4, -1 })]
    [TestCase(new[] { 5, 4, 4 })]
    public void At_Least_Two_Fives_In_Siteswap_Is_False(int[] input)
    {
        var sut = FilterBuilder.MinimumOccurence(5, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeFalse();
    }

    [Test]
    [TestCase(new[] { 6, 5, 5, 5, -1 })]
    [TestCase(new[] { 8, 7, 5, 5, 5, 4, 3, 7, -1 })]
    [TestCase(new[] { 8, 7, 5, 7, 5, 6, 5 })]
    [TestCase(new[] { 5, 5, 4 })]
    [TestCase(new[] { 8, 5, -1 })]
    public void At_Least_Two_Fives_In_Siteswap_Is_True(int[] input)
    {
        var sut = FilterBuilder.MinimumOccurence(5, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
}
