using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Test.Filter;

public partial class FilterTestSuite
{
    [Test]
    [TestCase(5, 5, 5)]
    [TestCase(4, 1, 1)]
    public void Standard_Filter_Are_Fullfilled(params int[] input)
    {
        var sut = FilterBuilder.WithDefault().Build();

        sut.CanFulfill(new PartialSiteswap(input.Select(x => (int)x).ToArray())).Should().BeFalse();
    }

    [Test]
    [TestCase(5, 3, -1)]
    [TestCase(8, 0, -1)]
    [TestCase(1, 1, -1)]
    [TestCase(3, 0, -1)]
    [TestCase(5, 3, 1)]
    [TestCase(9, 0, 0)]
    public void Standard_Filter_Are_Detect_An_Error(params int[] input)
    {
        var sut = FilterBuilder.WithDefault().Build();

        sut.CanFulfill(new PartialSiteswap(input.Select(x => (int)x).ToArray())).Should().BeTrue();
    }
}
