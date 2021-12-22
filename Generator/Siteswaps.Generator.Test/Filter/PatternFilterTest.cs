using System;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Generator.Test.Filter;

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
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 10)).PatternFilter(Enumerable.Range(Random.Shared.Next(), input.Length), 2);

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
        
    [Test]
    [TestCase(new[]{5,4,2})]
    [TestCase(new[]{8,0,5})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered(int[] input)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 5)).PatternFilter(new []{-1,-1,5}.ToImmutableArray(), 2);

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new[]{4,4,1})]
    [TestCase(new[]{8,0,1})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_2(int[] input)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 8)).PatternFilter(new []{-1,-1,5}.ToImmutableArray(), 2);

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeFalse();
    }
        
    [Test]
    [TestCase(new[]{4,4,1}, new []{4,4,1})]
    [TestCase(new[]{4,4,1}, new []{4,1,4})]
    [TestCase(new[]{4,4,1}, new []{1,4,4})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_3(int[] input, int[] filter)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 8)).PatternFilter(filter.ToImmutableArray(), 2);

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new[]{5,5,1}, new []{-2,-2,-1})]
    [TestCase(new[]{5,5,1}, new []{-2,-1,-1})]
    [TestCase(new[]{7,5,0}, new []{-2,-2,0})]
    public void Passes_Will_Be_Filtered_Correctly(int[] input, int[] filter)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 8)).PatternFilter(filter.ToImmutableArray(), 2);

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new[]{4,4,1}, new []{-3,-3,1})]
    [TestCase(new[]{4,4,1}, new []{-3,-1,-1})]
    [TestCase(new[]{4,4,1}, new []{-1,-1,-3})]
    public void Selfs_Will_Be_Filtered_Correctly(int[] input, int[] filter)
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3, 3, 0, 8)).PatternFilter(filter.ToImmutableArray(), 2);

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
}