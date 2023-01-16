using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Siteswaps.Generator.Generator;
using Siteswaps.Generator.Generator.Filter;
using VerifyTests;
using static VerifyNUnit.Verifier;

namespace Siteswaps.Generator.Api.Test;

public class SiteswapGeneratorTestSuite
{
    private SiteswapGenerator CreateTestObject(SiteswapGeneratorInput input,
        Func<IFilterBuilder, IFilterBuilder>? builder = null) =>
        new SiteswapGeneratorFactory()
            .ConfigureFilter(builder)
            .Create(input);

    static SiteswapGeneratorTestSuite()
    {
        VerifierSettings.DisableRequireUniquePrefix();
    }

    [TestCaseSource(typeof(GenerateInputs))]
    public async Task Verify_SiteswapGenerator_Against_Older_Version(SiteswapGeneratorInput input)
    {
        var siteswaps = await CreateTestObject(input).GenerateAsync().ToListAsync();

        await Verify(siteswaps.Select(x => x.ToString()).ToList())
            .UseTypeName(nameof(SiteswapGeneratorTestSuite))
            .UseMethodName(nameof(SiteswapGeneratorTestSuite.Verify_SiteswapGenerator_Against_Older_Version))
            .UseTextForParameters(GenerateInputs.ToName(input));
    }

    [Test]
    public async Task PatternFilterTest()
    {
        var sut = CreateTestObject(
            new SiteswapGeneratorInput(10, 6, 2, 10)
            {
                StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1000)
            },
            x => x.Pattern(new[] { 2, -1, 6, -1, 5, -1, -1, -1, -1, -1 }, 2));
        var siteswaps = await sut.GenerateAsync().ToListAsync();
        await Verify(siteswaps.Select(x => x.ToString()).ToList());
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
        yield return Next(4, 7, 5, 6);
        // yield return Next(12, 20, 2, 8);
    }
}