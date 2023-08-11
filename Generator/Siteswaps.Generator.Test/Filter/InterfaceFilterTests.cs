namespace Siteswaps.Generator.Test.Filter;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Generator;
using NUnit.Framework;

public class InterfaceFilterTests : FilterTestSuiteBase
{
    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;
    
    [Test]
    [TestCase(new sbyte[]{4,4,1}, new []{1,4,4})]
    [TestCase(new sbyte[]{4,4,1}, new []{1,4,Self})]
    [TestCase(new sbyte[]{5,3,1}, new []{1,3,5})]
    [TestCase(new sbyte[]{5,3,1}, new []{1,3,Pass})]
    [TestCase(new sbyte[]{5,3,1}, new []{1,3,DontCare})]
    public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_2(sbyte[] input, IEnumerable<int> @interface)
    {
        Input = new SiteswapGeneratorInput(3, 3, 0, 8);
        var sut = FilterBuilder.Interface(@interface, 2).Build();

        sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    }
        
    // [Test]
    // [TestCase(new sbyte[]{10,8,5,5,8,6}, new []{6,8,5})]
    // public void Fully_Filled_PartialSiteswap_Can_Be_Filtered_4(sbyte[] input, int[] filter)
    // {
    //     Input = new SiteswapGeneratorInput(6, 7, 0, 10);
    //     var sut = FilterBuilder.Interface(filter.Select(x => new List<int>(){x}).ToList(), 2, false).Build();
    //
    //     sut.CanFulfill(new PartialSiteswap(input)).Should().BeTrue();
    // }
    //

}
