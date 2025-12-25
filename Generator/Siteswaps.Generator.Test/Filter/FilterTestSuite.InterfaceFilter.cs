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
        // ps[0] = 9 -> Interface[0 + 9 % 3] = Interface[0] = 9
        // ps[1] = 7 -> Interface[1 + 7 % 3] = Interface[2] = 7
        // ps[2] = 5 -> Interface[2 + 5 % 3] = Interface[1] = 5
        // Interface: [9, 5, 7]

        var pattern = new List<List<int>>
        {
            new() { 9 },
            new() { -1 }, // Wildcard
            new() { 7 },
        };
        var sut = new InterfaceFilter(pattern);
        sut.CanFulfill(ps).Should().BeTrue();
    }

    [Test]
    public void InterfaceFilter_With_Pass_Wildcard_Matches()
    {
        var ps = new PartialSiteswap([8, 6, 2, -1, -1]);
        // Interface: [8, 6, 2, -1, -1] - wait, the constructor of PartialSiteswap fills it
        // PartialSiteswap constructor calls setter for each item.
        // items: [8, 6, 2, -1, -1]
        // i=0, v=8 -> Interface[0+8%5] = Interface[3] = 8
        // i=1, v=6 -> Interface[1+6%5] = Interface[2] = 6
        // i=2, v=2 -> Interface[2+2%5] = Interface[4] = 2
        // i=3, v=-1 -> skips
        // i=4, v=-1 -> skips
        // Interface should be: [-1, -1, 6, 8, 2]

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
