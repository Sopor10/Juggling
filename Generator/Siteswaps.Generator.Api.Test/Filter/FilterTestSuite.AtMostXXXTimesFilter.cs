﻿using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Generator.Api.Test.Filter;

public abstract partial class FilterTestSuite
{
    [Test]
    [TestCase(new[]{5,5,-1})]
    [TestCase(new[]{8,4,-1})]
    [TestCase(new[]{8,4,3})]
    public void At_Most_Two_Fives_In_Siteswap_Is_True(int[] input)
    {
        var sut =FilterBuilder.MaximumOccurence(5,2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeTrue();
    }
        
    [Test]
    [TestCase(new[]{6,5,5,5,-1})]
    [TestCase(new[]{8,7,5,5,5,4,3,7,-1})]
    [TestCase(new[]{8,7,5,7,5,6,5})]
    public void At_Most_Two_Fives_In_Siteswap_Is_False(int[] input)
    {
        var sut = FilterBuilder.MaximumOccurence(5,2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeFalse();
    }
}