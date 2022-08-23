using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyTests;
using static VerifyNUnit.Verifier;

namespace Siteswaps.Generator.Api.Test;

public abstract class SiteswapGeneratorTestSuite
{
    protected abstract ISiteswapGenerator CreateTestObject(SiteswapGeneratorInput input);

    static SiteswapGeneratorTestSuite()
    {
        VerifierSettings.DisableRequireUniquePrefix();
    }
   
    [TestCaseSource(typeof(GenerateInputs))]
    public async Task Verify_SiteswapGenerator_Against_Older_Version(SiteswapGeneratorInput input)
    {
        var siteswaps = await CreateTestObject(input).GenerateAsync();
        
        await Verify(siteswaps.Select(x => x.ToString()).ToList())
            .UseTypeName(nameof(SiteswapGeneratorTestSuite))
            .UseTextForParameters(GenerateInputs.ToName(input));
    }
}

class GenerateInputs : IEnumerable
{
    public static string ToName(SiteswapGeneratorInput input) =>
        $"Input({input.Period},{input.MaxHeight},{input.MinHeight},{input.NumberOfObjects})";
    
    private TestCaseData Next(int period, int maxHeight, int minHeight, int numberOfObjects)
    {
        var input = new SiteswapGeneratorInput
        {
            Period = period,
            MaxHeight = maxHeight,
            MinHeight = minHeight,
            NumberOfObjects = numberOfObjects,
            StopCriteria = new(TimeSpan.FromSeconds(60), 100000)
        };
        return new TestCaseData(input).SetName(ToName(input));
    }
    
    public IEnumerator GetEnumerator()
    {
        yield return Next(3, 13, 0, 8);
        yield return Next(3, 10, 0, 5);
        yield return Next(3,  5, 0, 3);
        yield return Next(5, 10, 2, 7);
        yield return Next(5, 10, 2, 6);
        yield return Next(5,  5, 0, 3);
        yield return Next(7, 13, 2, 8);
        yield return Next(12, 20, 2, 8);
    }
}