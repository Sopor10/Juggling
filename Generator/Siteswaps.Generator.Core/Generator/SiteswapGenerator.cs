using System.Runtime.CompilerServices;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;

namespace Siteswaps.Generator.Core.Generator;

public class SiteswapGenerator
{
    public SiteswapGenerator(SiteswapGeneratorInput input)
        : this(new NoFilter(), input) { }

    public SiteswapGenerator(ISiteswapFilter filter, SiteswapGeneratorInput input)
    {
        Filter = new AndFilter(filter, new RightAmountOfBallsFilter(input));
        Input = input;
        PartialSiteswap = PartialSiteswap.Standard(Input.Period, Input.MaxHeight);
    }

    private ISiteswapFilter Filter { get; }
    private SiteswapGeneratorInput Input { get; }
    private PartialSiteswap PartialSiteswap { get; }

    public IEnumerable<Siteswap> Generate(CancellationToken token = default)
    {
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        cancellationTokenSource.CancelAfter(Input.StopCriteria.TimeOut);

        var results = new List<Siteswap>();
        BackTrack(0, cancellationTokenSource.Token, results);
        return results;
    }

    public async IAsyncEnumerable<Siteswap> GenerateAsync(
        [EnumeratorCancellation] CancellationToken token
    )
    {
        foreach (var siteswap in Generate(token))
        {
            yield return siteswap;
        }
    }

    private void BackTrack(int uniqueMaxIndex, CancellationToken token, List<Siteswap> results)
    {
        if (token.IsCancellationRequested || results.Count >= Input.StopCriteria.MaxNumberOfResults)
            return;

        var min = Input.MinHeight;
        var uniqueMax =
            PartialSiteswap.Items[uniqueMaxIndex] != -1
                ? PartialSiteswap.Items[uniqueMaxIndex]
                : PartialSiteswap.Items[uniqueMaxIndex - 1];
        var max = uniqueMax;

        PartialSiteswap.ResetCurrentPosition();

        var targetSum = Input.NumberOfObjects * Input.Period;
        var partialSumBeforeCurrent = PartialSiteswap.PartialSum;
        var remainingAfterCurrent = Input.Period - PartialSiteswap.LastFilledPosition - 1;

        if (remainingAfterCurrent > 0)
        {
            var maxSumRest = GetMaxSumToGenerate(PartialSiteswap.LastFilledPosition + 1);
            var minSumRest = GetMinSumToGenerate(PartialSiteswap.LastFilledPosition + 1);

            var tightMin = targetSum - partialSumBeforeCurrent - maxSumRest;
            if (tightMin > min) min = tightMin;

            if (minSumRest < int.MaxValue)
            {
                var tightMax = targetSum - partialSumBeforeCurrent - minSumRest;
                if (tightMax < max) max = tightMax;
            }
        }

        if (min > max)
            return;

        for (var i = max; i >= min; i--)
        {
            if (results.Count >= Input.StopCriteria.MaxNumberOfResults)
                return;

            if (PartialSiteswap.FillCurrentPosition(i) is false)
                continue;

            bool canFulfill;
            if (Filter.IsRotationAware)
            {
                canFulfill = false;
                for (int j = 0; j < PartialSiteswap.LastFilledPosition + 1; j++)
                {
                    PartialSiteswap.RotationIndex = j;
                    if (Filter.CanFulfill(PartialSiteswap))
                    {
                        canFulfill = true;
                        break;
                    }
                }
                PartialSiteswap.RotationIndex = 0;
            }
            else
            {
                canFulfill = Filter.CanFulfill(PartialSiteswap);
            }
            if (canFulfill is false)
            {
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }

            if (PartialSiteswap.IsFilled())
            {
                if (PartialSiteswap.Items[^1] != uniqueMax)
                {
                    results.Add(Siteswap.CreateFromCorrect(PartialSiteswap.Items.AsSpan().ToArray()));
                }
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }

            PartialSiteswap.MoveForward();
            BackTrack(i == uniqueMax ? uniqueMaxIndex + 1 : 0, token, results);
            PartialSiteswap.MoveBack();
        }
    }

    private int GetMaxSumToGenerate(int fromIndex)
    {
        int maxSum = 0;
        int interfaceIndex = Input.Period + Input.MaxHeight - 2;
        for (int i = Input.Period - 1; i >= fromIndex; i--)
        {
            while (PartialSiteswap.Interface[interfaceIndex] != -1)
            {
                interfaceIndex--;
                if (interfaceIndex < i)
                    return 0;
            }
            maxSum += (interfaceIndex - i);
            interfaceIndex--;
        }
        return maxSum;
    }

    private int GetMinSumToGenerate(int fromIndex)
    {
        int minSum = 0;
        int interfaceIndex = fromIndex + Input.MinHeight;
        for (int i = fromIndex; i < Input.Period; i++)
        {
            while (PartialSiteswap.Interface[interfaceIndex] != -1)
            {
                interfaceIndex++;
                if ((interfaceIndex - i) > Input.MaxHeight)
                    return int.MaxValue;
            }
            minSum += (interfaceIndex - i);
            interfaceIndex++;
        }
        return minSum;
    }
}
