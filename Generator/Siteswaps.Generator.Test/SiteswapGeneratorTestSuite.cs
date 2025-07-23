using System.Collections;
using Siteswaps.Generator.Generator;
using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Api.Test;

public class SiteswapGeneratorTestSuite
{
    static SiteswapGeneratorTestSuite()
    {
        VerifierSettings.DisableRequireUniquePrefix();
    }

    [TestCaseSource(typeof(GenerateInputs))]
    public async Task Verify_SiteswapGenerator_Against_Older_Version(SiteswapGeneratorInput input)
    {
        var siteswaps = await new SiteswapGenerator(new NoFilter(), input)
            .GenerateAsync(new CancellationTokenSource().Token)
            .ToListAsync();

        await Verify(siteswaps.Select(x => x.ToString()).ToList())
            .UseTypeName(nameof(SiteswapGeneratorTestSuite))
            .UseMethodName(nameof(Verify_SiteswapGenerator_Against_Older_Version))
            .UseTextForParameters(GenerateInputs.ToName(input));
    }

    [Test]
    public async Task PatternFilterTest()
    {
        SiteswapGeneratorInput input = new SiteswapGeneratorInput(10, 6, 2, 10)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1000),
        };
        var sut = new SiteswapGenerator(
            new FilterBuilder(input).Pattern([2, -1, 6, -1, 5, -1, -1, -1, -1, -1], 2).Build(),
            input
        );
        var siteswaps = await sut.GenerateAsync(new CancellationTokenSource().Token).ToListAsync();
        await Verify(siteswaps.Select(x => x.ToString()).ToList());
    }

    [Test]
    public async Task WrongThings()
    {
        SiteswapGeneratorInput input = new SiteswapGeneratorInput(14, 7, 2, 9)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1000),
        };
        var sut = new SiteswapGenerator(
            new FilterBuilder(input)
                .FlexiblePattern(
                    [
                        [9],
                        [6],
                        [9],
                        [6],
                        [9],
                        [8],
                        [2],
                    ],
                    2,
                    false
                )
                .ExactOccurence(3, 0)
                .ExactOccurence(1, 0)
                .ExactOccurence(0, 0)
                .Build(),
            input
        );
        var siteswaps = await sut.GenerateAsync(new CancellationTokenSource().Token).ToListAsync();
        // await Verify(siteswaps.Select(x => x.ToString()).ToList());
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
            StopCriteria = new(TimeSpan.FromSeconds(60), 100000),
        };
        return new TestCaseData(input).SetName(ToName(input));
    }

    public IEnumerator GetEnumerator()
    {
        yield return Next(3, 13, 0, 8);
        yield return Next(3, 10, 0, 5);
        yield return Next(3, 5, 0, 3);
        yield return Next(5, 10, 2, 7);
        yield return Next(5, 10, 2, 6);
        yield return Next(5, 5, 0, 3);
        yield return Next(7, 13, 2, 8);
        yield return Next(4, 7, 5, 6);
        // yield return Next(12, 20, 2, 8);
    }
}
