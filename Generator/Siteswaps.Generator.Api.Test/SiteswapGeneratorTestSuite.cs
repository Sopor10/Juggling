using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using static VerifyNUnit.Verifier;

namespace Siteswaps.Generator.Api.Test;

public abstract class SiteswapGeneratorTestSuite
{
    protected abstract ISiteswapGenerator CreateTestObject(SiteswapGeneratorInput input);

    [Test]
    [TestCase(3, 13, 0, 8)]
    [TestCase(3, 10, 0, 5)]
    [TestCase(3,  5, 0, 3)]
    [TestCase(5, 10, 2, 7)]
    [TestCase(5, 10, 2, 6)]
    [TestCase(5,  5, 0, 3)]
    [TestCase(7, 13, 2, 8)]
    public async Task Verify_SiteswapGenerator_Against_Older_Version(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        var input = new SiteswapGeneratorInput
        {
            Period = period,
            MaxHeight = maxHeight,
            MinHeight = minHeight,
            NumberOfObjects = numberOfObjects,
            StopCriteria = new(TimeSpan.FromSeconds(15), 10000)
        };

        var siteswaps = await CreateTestObject(input).GenerateAsync();
        
        await Verify(siteswaps.Select(x => x.ToString()).ToList())
            .UseTypeName(nameof(SiteswapGeneratorTestSuite));
    }
}
