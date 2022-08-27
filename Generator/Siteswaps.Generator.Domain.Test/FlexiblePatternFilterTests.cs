using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.Domain.Test;

public class FlexiblePatternFilterTests
{
    [Test]
    public void Local_Notation_Works_For_2_People()
    {
        var pattern = new List<List<int>>
        {
            new(){ 9},
            new(){ 5},
            new(){-1},
            new(){-1},
            new(){-1},
        };

        var numberOfJuggler = 2;
        var siteswapGeneratorInput = new SiteswapGeneratorInput()
        {
            Period = 5,
        };
        
        var isGlobalPattern = false;
        var sut = new FlexiblePatternFilter(pattern, numberOfJuggler, siteswapGeneratorInput, isGlobalPattern);

        sut.CanFulfill(new PartialSiteswap(new sbyte[] { 9,7,5,3,1 }, 4)).Should().BeTrue();
    }

    [Test]
    public void Local_Notation_Works_For_3_People()
    {
        var pattern = new List<List<int>>
        {
            new(){ 9},
            new(){ 3},
            new(){-1},
            new(){-1},
            new(){-1},
        };

        var numberOfJuggler = 3;
        var siteswapGeneratorInput = new SiteswapGeneratorInput()
        {
            Period = 5,
        };
        var isGlobalPattern = false;
        var sut = new FlexiblePatternFilter(pattern, numberOfJuggler, siteswapGeneratorInput, isGlobalPattern);

        sut.CanFulfill(new PartialSiteswap(new sbyte[] { 9,7,5,3,1 }, 4)).Should().BeTrue();
    }

    
    [Test]
    public void METHOD_1()
    {
        var pattern = new List<List<int>>
        {
            new(){ -3},
            new(){ -2},
            new(){-1},
            new(){-1},
            new(){-1},
        };

        var numberOfJuggler = 3;
        var siteswapGeneratorInput = new SiteswapGeneratorInput()
        {
            Period = 5,
        };
        var isGlobalPattern = true;
        var sut = new FlexiblePatternFilter(pattern, numberOfJuggler, siteswapGeneratorInput, isGlobalPattern);

        sut.CanFulfill(new PartialSiteswap(new sbyte[] { 9,7,5,3,1 }, 4)).Should().BeTrue();
    }
    
    
    
    [Test]
    public void METHOD_4()
    {
        var pattern = new List<List<int>>
        {
            new(){ -3},
            new(){ -2},
            new(){4,5},
            new(){3},
            new(){1,2},
        };

        var numberOfJuggler = 3;
        var siteswapGeneratorInput = new SiteswapGeneratorInput()
        {
            Period = 5,
        };
        var isGlobalPattern = true;
        var sut = new FlexiblePatternFilter(pattern, numberOfJuggler, siteswapGeneratorInput, isGlobalPattern);

        sut.CanFulfill(new PartialSiteswap(new sbyte[] { 9,7,5,3,1 }, 4)).Should().BeTrue();
    }
    
    [Test]
    public void METHOD_2()
    {
        var pattern = new List<List<int>>
        {
            new(){ 5 },
            new(){ 3 },
            new(){-1},
            new(){-1},
            new(){-1},
        };

        var numberOfJuggler = 3;
        var siteswapGeneratorInput = new SiteswapGeneratorInput()
        {
            Period = 5,
        };
        var isGlobalPattern = true;
        var sut = new FlexiblePatternFilter(pattern, numberOfJuggler, siteswapGeneratorInput, isGlobalPattern);

        sut.CanFulfill(new PartialSiteswap(new sbyte[] { 9,7,5,3,1 }, 4)).Should().BeTrue();
    }
    
    [Test]
    public void METHOD_3()
    {
        var pattern = new List<List<int>>
        {
            new(){ 4, 5},
            new(){ 6},
            new(){-1},
            new(){-1},
            new(){-1},
        };

        var numberOfJuggler = 3;
        var siteswapGeneratorInput = new SiteswapGeneratorInput()
        {
            Period = 5,
        };
        var isGlobalPattern = true;
        var sut = new FlexiblePatternFilter(pattern, numberOfJuggler, siteswapGeneratorInput, isGlobalPattern);

        sut.CanFulfill(new PartialSiteswap(new sbyte[] { 9,7,5,3,1 }, 4)).Should().BeFalse();
    }
}