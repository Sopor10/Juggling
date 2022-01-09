using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using static VerifyNUnit.Verifier;

namespace Siteswaps.Generator.Api.Test;

public abstract class SiteswapGeneratorTestSuite
{
    protected abstract ISiteswapGenerator CreateTestObject(SiteswapGeneratorInput input);

    [Test]
    [TestCase(new[] { 5, 3, 1 })]
    [TestCase(new[] { 4, 2, 3 })]
    public async Task Generator_Generates_Siteswap(int[] expected)
    {
        var input = Input(3, 5, 0, 3);

        var generator = CreateTestObject(input);


        var result = (await generator.GenerateAsync()).ToList();
        result.Should().Contain(expected);
    }

    private SiteswapGeneratorInput Input(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        return new SiteswapGeneratorInput
        {
            Period = period,
            MaxHeight = maxHeight,
            MinHeight = minHeight,
            NumberOfObjects = numberOfObjects,
            StopCriteria = new(TimeSpan.FromSeconds(15), 10000)
        };
    }

    [Test]
    [TestCase(5, 10, 2, 7)]
    [TestCase(5, 10, 2, 6)]
    [TestCase(7, 13, 2, 8)]
    [TestCase(3, 13, 0, 8)]
    [TestCase(3, 10, 0, 5)]
    [TestCase(5, 5, 0, 3)]
    public async Task Verify_SiteswapGenerator_Against_Older_Version(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        var input = Input(period, maxHeight, minHeight, numberOfObjects);
        var sut = CreateTestObject(input);

        var siteswaps = (await sut.GenerateAsync()).Select(x => x.ToString()).ToList();
        await Verify(siteswaps).UseTypeName(nameof(SiteswapGeneratorTestSuite));

    }
}
