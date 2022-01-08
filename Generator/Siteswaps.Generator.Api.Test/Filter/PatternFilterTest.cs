using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Siteswaps.Generator.Api.Test.Filter;

public abstract partial class FilterTestSuite
{
    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;
    
    [Test]
    [TestCase(new[]{5,5,DontCare})]
    [TestCase(new[]{8,4,2,6,3,DontCare})]
    [TestCase(new[]{8,53,21,1,3,DontCare})]
    [TestCase(new[]{8,3,DontCare})]
    [TestCase(new[]{8,3,DontCare,DontCare,DontCare,DontCare})]
    public void Partly_Filled_Siteswaps_Do_Not_Get_Checked(int[] input)
    {
        ConfigureSiteswapGeneratorInput(new SiteswapGeneratorInput(3, 3, 0, 10));
        var sut = FilterBuilder.Pattern(Enumerable.Range(Random.Shared.Next(), input.Length), 2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeTrue();
    }
        
    [Test]
    [TestCase(new[]{5,4,2})]
    [TestCase(new[]{8,0,5})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered(int[] input)
    {
        ConfigureSiteswapGeneratorInput(new SiteswapGeneratorInput(3, 3, 0, 5));
        var sut = FilterBuilder.Pattern(new []{DontCare,DontCare,5}, 2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new[]{4,4,1})]
    [TestCase(new[]{8,0,1})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_2(int[] input)
    {
        ConfigureSiteswapGeneratorInput(new SiteswapGeneratorInput(3, 3, 0, 8));
        var sut = FilterBuilder.Pattern(new []{DontCare,DontCare,5}, 2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeFalse();
    }
        
    [Test]
    [TestCase(new[]{4,4,1}, new []{4,4,1})]
    [TestCase(new[]{4,4,1}, new []{4,1,4})]
    [TestCase(new[]{4,4,1}, new []{1,4,4})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_3(int[] input, int[] filter)
    {
        ConfigureSiteswapGeneratorInput(new SiteswapGeneratorInput(3, 3, 0, 8));
        var sut = FilterBuilder.Pattern(filter, 2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new[]{5,5,1}, new []{Pass,Pass,DontCare})]
    [TestCase(new[]{5,5,1}, new []{Pass,DontCare,DontCare})]
    [TestCase(new[]{7,5,0}, new []{Pass,Pass,0})]
    public void Passes_Will_Be_Filtered_Correctly(int[] input, int[] filter)
    {
        ConfigureSiteswapGeneratorInput(new SiteswapGeneratorInput(3, 3, 0, 8));
        var sut = FilterBuilder.Pattern(filter, 2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new[]{10,8,3}, new []{Pass, Pass, Self})]
    [TestCase(new[]{10,2,9}, new []{Pass, Pass, Self})]
    [TestCase(new[]{9,4,8}, new []{Pass, Pass, Self})]
    public void Passes_Will_Be_Filtered_Correctly_False(int[] input, int[] filter)
    {
        ConfigureSiteswapGeneratorInput(new SiteswapGeneratorInput(3, 3, 0, 10));
        var sut = FilterBuilder.Pattern(filter, 2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeFalse();
    }
    
    [Test]
    [TestCase(new[]{4,4,1}, new []{Self,Self,1})]
    [TestCase(new[]{4,4,1}, new []{Self,DontCare,DontCare})]
    [TestCase(new[]{4,4,1}, new []{DontCare,DontCare,Self})]
    public void Selfs_Will_Be_Filtered_Correctly(int[] input, int[] filter)
    {
        ConfigureSiteswapGeneratorInput(new SiteswapGeneratorInput(3, 3, 0, 8));
        var sut = FilterBuilder.Pattern(filter, 2).Build();

        sut.CanFulfill(AsPartialSiteswap(input)).Should().BeTrue();
    }
}
