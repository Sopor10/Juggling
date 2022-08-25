using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Generator.Api.Test.Filter;

public abstract partial class FilterTestSuite
{
    [Test]
    [TestCase(new sbyte[]{8,4,-1})]
    [TestCase(new sbyte[]{5,4,4})]
    [TestCase(new sbyte[]{5,5,5})]
    public void Exactly_Two_Fives_In_Siteswap_Is_False(sbyte[] input)
    {
        var sut = FilterBuilder.ExactOccurence(5,2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeFalse();
    }
        
    [Test]
    [TestCase(new sbyte[]{6,5,5,-1})]
    [TestCase(new sbyte[]{8,7,5,5,4,3,7,-1})]
    [TestCase(new sbyte[]{8,7,5,7,5,6})]
    [TestCase(new sbyte[]{5,5,4})]
    [TestCase(new sbyte[]{8,5,-1})]
    [TestCase(new sbyte[]{8,5,-1,-1})]
    public void Exactly_Two_Fives_In_Siteswap_Is_True(sbyte[] input)
    {
        var sut = FilterBuilder.ExactOccurence(5,2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeTrue();
    }
}