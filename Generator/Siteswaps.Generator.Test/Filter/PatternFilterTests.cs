using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Test.Filter;

public class PatternFilterTests : FilterTestSuiteBase
{
    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;
    
    [Test]
    [TestCase(new sbyte[]{5,5,DontCare})]
    [TestCase(new sbyte[]{8,DontCare,DontCare})]
    [TestCase(new sbyte[]{8,3,DontCare})]
    public void Partly_Filled_Siteswaps_Do_Not_Get_Checked(sbyte[] input)
    {
        Input = new SiteswapGeneratorInput(3, 3, 0, 10);
        var sut = FilterBuilder.Pattern(Enumerable.Range(Random.Shared.Next(), input.Length), 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
        
    [Test]
    [TestCase(new sbyte[]{5,4,2})]
    [TestCase(new sbyte[]{8,0,5})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered(sbyte[] input)
    {
        Input = new SiteswapGeneratorInput(3, 3, 0, 5);
        var sut = FilterBuilder.Pattern(new []{DontCare,DontCare,5}, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new sbyte[]{4,4,1})]
    [TestCase(new sbyte[]{8,0,1})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_2(sbyte[] input)
    {
        Input = new SiteswapGeneratorInput(3, 3, 0, 8);
        var sut = FilterBuilder.Pattern(new []{DontCare,DontCare,5}, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeFalse();
    }
        
    [Test]
    [TestCase(new sbyte[]{4,4,1}, new []{4,4,1})]
    [TestCase(new sbyte[]{4,4,1}, new []{4,1,4})]
    [TestCase(new sbyte[]{4,4,1}, new []{1,4,4})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_3(sbyte[] input, int[] filter)
    {
        Input = new SiteswapGeneratorInput(3, 3, 0, 8);
        var sut = FilterBuilder.Pattern(filter, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new sbyte[]{10,8,5,5,8,6}, new []{6,8,5})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_4(sbyte[] input, int[] filter)
    {
        Input = new SiteswapGeneratorInput(6, 7, 0, 10);
        var sut = FilterBuilder.FlexiblePattern(Pattern.FromThrows(filter, 2), 2, false).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new sbyte[]{5,5,1}, new []{Pass,Pass,DontCare})]
    [TestCase(new sbyte[]{5,5,1}, new []{Pass,DontCare,DontCare})]
    [TestCase(new sbyte[]{7,5,0}, new []{Pass,Pass,0})]
    public void Passes_Will_Be_Filtered_Correctly(sbyte[] input, int[] filter)
    {
        Input = new SiteswapGeneratorInput(3, 3, 0, 8);
        var sut = FilterBuilder.Pattern(filter, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
    
    [Test]
    [TestCase(new sbyte[]{10,8,3}, new []{Pass, Pass, Self})]
    [TestCase(new sbyte[]{10,2,9}, new []{Pass, Pass, Self})]
    [TestCase(new sbyte[]{9,4,8}, new []{Pass, Pass, Self})]
    public void Passes_Will_Be_Filtered_Correctly_False(sbyte[] input, int[] filter)
    {
        Input = new SiteswapGeneratorInput(3, 3, 0, 10);
        var sut = FilterBuilder.Pattern(filter, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeFalse();
    }
    
    [Test]
    [TestCase(new sbyte[]{4,4,1}, new []{Self,Self,1})]
    [TestCase(new sbyte[]{4,4,1}, new []{Self,DontCare,DontCare})]
    [TestCase(new sbyte[]{4,4,1}, new []{DontCare,DontCare,Self})]
    public void Selfs_Will_Be_Filtered_Correctly(sbyte[] input, int[] filter)
    {
        Input = new SiteswapGeneratorInput(3, 3, 0, 8);
        var sut = FilterBuilder.Pattern(filter, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
}
