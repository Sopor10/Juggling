using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

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

    [Test]
    [TestCase(10, 10, 10, 0, 4, 2)]
    [TestCase(9, 7, 2)]
    [TestCase(9, 7, 2, 9, 7, 2)]
    [Ignore("not ready yet")]
    public void LocallyValidFilterWorksTrue(params int[] input)
    {
        var sut = new LocallyValidFilter(2, 0);

        sut.CanFulfill(new PartialSiteswap(input.Select(x => (int)x).ToArray())).Should().BeTrue();
    }

    [Test]
    [TestCase(10, 10, 10, 0, 3, 3)]
    [TestCase(10, 10, 7, 5, 3, 1)]
    public void LocallyValidFilterWorksFalse(params int[] input)
    {
        var sut = new LocallyValidFilter(2, 0);

        sut.CanFulfill(new PartialSiteswap(input.Select(x => (int)x).ToArray())).Should().BeFalse();
    }

    [Test]
    [TestCase(10, 10, 10, 0, 3, 3)]
    [TestCase(10, 10, 7, 5, 3, 1)]
    public void LocallyValidFilterWorksFalseJugglerB(params int[] input)
    {
        var sut = new LocallyValidFilter(2, 1);

        sut.CanFulfill(new PartialSiteswap(input.Select(x => (int)x).ToArray())).Should().BeFalse();
    }
}
