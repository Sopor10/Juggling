using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Test.Filter;

public partial class FilterTestSuite
{
    [Test]
    public void InterfaceFilter_Matches_Correctly()
    {
        var ps = new PartialSiteswap([9, 7, 5]);
        var pattern2 = new List<List<int>>
        {
            new() { 9 },
            new() { 5 },
            new() { 7 },
        };
        var sut2 = new InterfaceFilter(pattern2);
        sut2.CanFulfill(ps).Should().BeTrue();
    }

    [Test]
    public void InterfaceFilter_With_Pass_Symbol_Matches()
    {
        var psValid = new PartialSiteswap([8, 6, 2, -1, -1]);
        var sutValid = new InterfaceFilter([
            [7],
            [-2],
            [6],
            [8],
            [-1],
        ]);
        sutValid.CanFulfill(psValid).Should().BeTrue();
    }

    [Test]
    public void Interface_Can_Not_Be_Fullfilled()
    {
        var psValid = new PartialSiteswap([8, 6, 2, -1, -1]);
        var sutValid = new InterfaceFilter([
            [7],
            [-1],
            [-1],
            [5],
            [2],
            [-1],
        ]);
        sutValid.CanFulfill(psValid).Should().BeFalse();
    }

    [Test]
    public void InterfaceFilter_With_Wildcard_Matches()
    {
        var ps = new PartialSiteswap([9, 7, 5]);
        var pattern = new List<List<int>>
        {
            new() { 9 },
            new() { -1 },
            new() { 7 },
        };
        var sut = new InterfaceFilter(pattern);
        sut.CanFulfill(ps).Should().BeTrue();
    }

    [Test]
    public void InterfaceFilter_With_Pass_Wildcard_Matches()
    {
        var ps = new PartialSiteswap([8, 6, 2, -1, -1]);
        var sut = new InterfaceFilter([
            [-1],
            [-1],
            [6],
            [8],
            [2],
        ]);
        sut.CanFulfill(ps).Should().BeTrue();
    }
}
