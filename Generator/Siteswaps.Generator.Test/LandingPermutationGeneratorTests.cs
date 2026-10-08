using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Test;

public class LandingPermutationGeneratorTests
{
    [TestCase(3, 5, 0, 3)]
    [TestCase(4, 7, 1, 6)]
    [TestCase(5, 6, 2, 7)]
    public void Unfiltered_Output_Matches_Independent_Canonical_Landing_Enumeration(
        int period,
        int maxHeight,
        int minHeight,
        int numberOfObjects
    )
    {
        var input = new SiteswapGeneratorInput(period, numberOfObjects, minHeight, maxHeight)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(10), 100_000),
        };

        var actual = new SiteswapGenerator(new NoFilter(), input)
            .Generate()
            .Select(items => Format(items.Items))
            .ToArray();
        var expected = Enumerate(period, minHeight, maxHeight, numberOfObjects)
            .Where(IsGeneratorCanonical)
            .Select(items => Format(items))
            .ToArray();

        actual.Should().BeEquivalentTo(expected);
        actual.Should().HaveSameCount(expected);
    }

    [Test]
    public void Landing_Permutation_Path_Is_Bounded_By_Materialized_Output_Size()
    {
        var largeInput = new SiteswapGeneratorInput(7, 8, 2, 13)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(30), 100_000),
        };
        var highDimensionalInput = new SiteswapGeneratorInput(30, 30, 0, 40)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(6), 14_000_000),
        };

        new SiteswapGenerator(new NoFilter(), largeInput)
            .UsesLandingPermutationGenerator.Should()
            .BeTrue();
        new SiteswapGenerator(new NoFilter(), highDimensionalInput)
            .UsesLandingPermutationGenerator.Should()
            .BeFalse();
    }

    [Test]
    public void Established_Backtracker_Path_Honors_Cancellation()
    {
        var input = new SiteswapGeneratorInput(30, 30, 0, 40)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(6), 14_000_000),
        };
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        new SiteswapGenerator(new NoFilter(), input)
            .Generate(cancellation.Token)
            .Should()
            .BeEmpty();
    }

    [Test]
    public void HighDimensional_Unfiltered_Backtracker_Respects_Maximum_Result_Count()
    {
        var input = new SiteswapGeneratorInput(30, 30, 0, 40)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(30), 350_000),
        };

        var actual = new SiteswapGenerator(new NoFilter(), input).Generate().ToArray();

        actual.Should().HaveCount(350_000);
        actual
            .Should()
            .OnlyContain(siteswap => siteswap.Items.All(height => height >= 0 && height <= 40));
    }

    [Test]
    public void HighDimensional_Unfiltered_Generation_Respects_Maximum_Result_Count()
    {
        var input = new SiteswapGeneratorInput(30, 30, 0, 40)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(6), 250),
        };

        var actual = new SiteswapGenerator(new NoFilter(), input).Generate().ToArray();

        actual.Should().HaveCount(250);
        actual
            .Should()
            .OnlyContain(siteswap => siteswap.Items.All(height => height >= 0 && height <= 40));
    }

    [Test]
    public void Unfiltered_Generation_Honors_Already_Canceled_Token()
    {
        var input = new SiteswapGeneratorInput(7, 8, 2, 13)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(30), 100_000),
        };
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var actual = new SiteswapGenerator(new NoFilter(), input).Generate(cancellation.Token);

        actual.Should().BeEmpty();
    }

    [Test]
    public void Unfiltered_Generation_Supports_Periods_Beyond_Landing_Mask_Width()
    {
        var input = new SiteswapGeneratorInput(65, 1, 0, 2)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(30), 1),
        };

        var actual = new SiteswapGenerator(new NoFilter(), input).Generate().ToArray();

        actual.Should().HaveCount(1);
        actual[0].Items.Should().OnlyContain(height => height >= 0 && height <= 2);
        actual[0].IsValid().Should().BeTrue();
    }

    [Test]
    public void Unfiltered_Generation_Rejects_An_Inverted_Height_Bound()
    {
        var input = new SiteswapGeneratorInput(3, 3, 5, 2)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(30), 100),
        };

        new SiteswapGenerator(new NoFilter(), input).Generate().Should().BeEmpty();
    }

    private static IEnumerable<int[]> Enumerate(
        int period,
        int minHeight,
        int maxHeight,
        int numberOfObjects
    )
    {
        var items = new int[period];
        var occupied = new bool[period];
        return EnumerateAt(0, 0);

        IEnumerable<int[]> EnumerateAt(int position, int sum)
        {
            if (position == period)
            {
                if (sum == numberOfObjects * period)
                    yield return (int[])items.Clone();
                yield break;
            }

            for (var height = minHeight; height <= maxHeight; height++)
            {
                var landing = (position + height) % period;
                if (occupied[landing])
                    continue;

                occupied[landing] = true;
                items[position] = height;
                foreach (var result in EnumerateAt(position + 1, sum + height))
                    yield return result;
                occupied[landing] = false;
            }
        }
    }

    private static bool IsGeneratorCanonical(int[] items)
    {
        var uniqueMaxIndex = 0;
        for (var position = 1; position < items.Length; position++)
        {
            var uniqueMax = uniqueMaxIndex < position ? items[uniqueMaxIndex] : items[position - 1];
            if (items[position] > uniqueMax)
                return false;
            if (position == items.Length - 1)
                return items[position] != uniqueMax;
            if (items[position] == uniqueMax)
                uniqueMaxIndex++;
        }

        return true;
    }

    private static string Format(int[] items) => string.Join(',', items);
}
