using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Generator;

public class SiteswapGenerator
{
    public SiteswapGenerator(ISiteswapFilter filter, SiteswapGeneratorInput input)
    {
        Filter = filter;
        Input = input;
        PartialSiteswap = PartialSiteswap.Standard((sbyte)Input.Period, (sbyte)Input.MaxHeight);

    }

    private ISiteswapFilter Filter { get; }
    private SiteswapGeneratorInput Input { get; }
    private PartialSiteswap PartialSiteswap { get; set; }

    public async IAsyncEnumerable<Siteswap> GenerateAsync()
    {

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(Input.StopCriteria.TimeOut);

        await foreach (var siteswap in GenerateInternalAsync().Take(Input.StopCriteria.MaxNumberOfResults).WithCancellation(cancellationTokenSource.Token))
        {
            yield return siteswap;
        }
    }
    
    private IAsyncEnumerable<Siteswap> GenerateInternalAsync()
    {

        return BackTrack(0);
    }

    private async IAsyncEnumerable<Siteswap> BackTrack(int uniqueMaxIndex)
    {
        var min = Input.MinHeight;
        var max = PartialSiteswap.Items[uniqueMaxIndex] != -1? PartialSiteswap.Items[uniqueMaxIndex]:PartialSiteswap.Items[uniqueMaxIndex - 1];

        for (var i = max; i >= min; i--)
        {
            if (PartialSiteswap.FillCurrentPosition(i) is false)
            {
                continue;
            }
             
            if ((PartialSiteswap.PartialSum + (Input.Period - PartialSiteswap.LastFilledPosition - 1) * min)/ Input.Period > Input.NumberOfObjects)
            {
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }
            
            if ((PartialSiteswap.PartialSum + (Input.Period - PartialSiteswap.LastFilledPosition - 1) * Input.MaxHeight)/ Input.Period < Input.NumberOfObjects)
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
                }
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }

            PartialSiteswap.MoveForward();
            await foreach (var siteswap in BackTrack(i == max ? uniqueMaxIndex + 1 : 0))
            {
                yield return siteswap;
            }
            PartialSiteswap.MoveBack();
        }
    }
}