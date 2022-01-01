using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.Domain.Test.Filter;

public class PatternFilterHeuristicsTest
{
    [Test]
    public void METHOD()
    {
        var sut = new FilterFactory(new SiteswapGeneratorInput(3,3,0,10)).GeneratePatternFilterHeuristics(new []{5,-1,5},2);

        sut.CanFulfill(new PartialSiteswap(4, 4, -1)).Should().BeFalse();
    }
}