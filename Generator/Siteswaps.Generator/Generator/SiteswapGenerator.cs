using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Generator;

public class SiteswapGenerator
{
    public SiteswapGenerator(ISiteswapFilter filter, SiteswapGeneratorInput input)
    {
        Filter = filter;
        Input = input;
    }

    private bool CountExceedsLimit { get; set; }
    private int Count { get; set; }
    private ISiteswapFilter Filter { get; }
    private SiteswapGeneratorInput Input { get; set; }
    private PartialSiteswap PartialSiteswap { get; set; }

    public IAsyncEnumerable<Siteswap> GenerateAsync()
    {
        PartialSiteswap = PartialSiteswap.Standard((sbyte)Input.Period, (sbyte)Input.MaxHeight);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(Input.StopCriteria.TimeOut);
        Token = cancellationTokenSource.Token;

        return BackTrack(0);
    }

    private CancellationToken Token { get; set; }

    private async IAsyncEnumerable<Siteswap> BackTrack(int uniqueMaxIndex)
    {
        var min = Input.MinHeight;
        var max = PartialSiteswap.Items[uniqueMaxIndex] != -1? PartialSiteswap.Items[uniqueMaxIndex]:PartialSiteswap.Items[uniqueMaxIndex - 1];

        for (var i = max; i >= min; i--)
        {
            if (ShouldStop()) yield break;
            
            if (PartialSiteswap.FillCurrentPosition(i) is false)
            {
                continue;
            }
             
            if ((PartialSiteswap.PartialSum + (Input.Period - PartialSiteswap.LastFilledPosition) * min)/ Input.Period > Input.NumberOfObjects)
            {
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }
            
            if ((PartialSiteswap.PartialSum + (Input.Period - PartialSiteswap.LastFilledPosition) * Input.MaxHeight)/ Input.Period < Input.NumberOfObjects)
            {
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }
            
            if (Filter.CanFulfill(PartialSiteswap) is false)
            {
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }

            if (PartialSiteswap.IsFilled())
            {
                if (PartialSiteswap.Items[^1] != max)
                {
                    yield return Siteswap.CreateFromCorrect(PartialSiteswap.Items);
                    Count++;
                    if (Count > Input.StopCriteria.MaxNumberOfResults)
                    {
                        CountExceedsLimit = true;
                    }
                }
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }

            PartialSiteswap.MoveForward(max);
            await foreach (var siteswap in BackTrack(i == max ? uniqueMaxIndex + 1 : 0).WithCancellation(Token))
            {
                yield return siteswap;
            }
            PartialSiteswap.MoveBack();
        }
    }

    private bool ShouldStop()
    {
        return CountExceedsLimit || Token.IsCancellationRequested;
    }
}