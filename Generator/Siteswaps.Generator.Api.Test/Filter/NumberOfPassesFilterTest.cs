using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Api.Test.Filter;

public abstract class NumberOfPassesFilterTest : FilterTestBase
{
    protected abstract ISiteswapFilter CreateSut(int numberOfJugglers);

    [Test]
    [TestCase(new[] { 4, 2, 3 }, 2, ExpectedResult = true)]
    [TestCase(new[] { 4, 4, 1 }, 2, ExpectedResult = true)]
    [TestCase(new[] { 6, 0, 3 }, 2, ExpectedResult = true)]
    [TestCase(new[] { 5, 0, 4 }, 4, ExpectedResult = true)]
    [TestCase(new[] { 4, 4, 1 }, 4, ExpectedResult = true)]
    [TestCase(new[] { 6, 0, 3 }, 3, ExpectedResult = false)]
    [TestCase(new[] { 9, 0, 0 }, 3, ExpectedResult = false)]
    public bool Are_There_Passes(int[] input, int numberOfJugglers) => 
        CreateSut(numberOfJugglers).CanFulfill(ToPartialSiteswap(input));
}

public abstract class FilterTestBase
{
    protected abstract ISiteswapFilter CreateSut();

    protected abstract IPartialSiteswap ToPartialSiteswap(int[] values);
}