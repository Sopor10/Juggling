using System;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Test.Generator.Filter;

public class PatternFilterTest
{
    [Test]
    [TestCase(new[]{5,5,-1})]
    [TestCase(new[]{8,4,2,6,3,-1})]
    [TestCase(new[]{8,53,21,1,3,-1})]
    [TestCase(new[]{8,3,-1})]
    [TestCase(new[]{8,3,-1,-1,-1,-1})]
    public void Partly_Filled_Siteswaps_Do_Not_Get_Checked(int[] input)
    {
        var sut = new FilterFactory().PatternFilter(Enumerable.Range(Random.Shared.Next(), input.Length));
        var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 10));

        result.Should().BeTrue();
    }
        
    [Test]
    [TestCase(new[]{5,4,2})]
    [TestCase(new[]{8,0,5})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered(int[] input)
    {
        var sut = new FilterFactory().PatternFilter(new []{-1,-1,5}.ToImmutableArray());
        var result = sut.CanFulfill(new PartialSiteswap(input), new SiteswapGeneratorInput(3, 3, 0, 5));

        result.Should().BeTrue();
    }
}