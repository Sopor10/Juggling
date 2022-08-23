using System.Diagnostics;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain;

public class SiteswapGenerator : ISiteswapGenerator
{
    public SiteswapGenerator(ISiteswapFilter filter, SiteswapGeneratorInput input)
    {
        Filter = filter;
        Input = input;
        PartialSiteswap = PartialSiteswap.Standard((sbyte)Input.Period, (sbyte)Input.MaxHeight);
        Stopwatch = new Stopwatch();

    }

    private Stopwatch Stopwatch { get; set; }

    private HashSet<ISiteswap> Siteswaps { get; } = new();
    private ISiteswapFilter Filter { get; }
    private SiteswapGeneratorInput Input { get; }
    private PartialSiteswap PartialSiteswap { get; }

    public async Task<IEnumerable<ISiteswap>> GenerateAsync()
    {
        Stopwatch.Start();

        await Task.Run(() => BackTrack(0));

        return Siteswaps;
    }

    private void BackTrack(int uniqueMaxIndex)
    {
        var min = Input.MinHeight;
        var max = PartialSiteswap.Items[uniqueMaxIndex] != -1? PartialSiteswap.Items[uniqueMaxIndex]:PartialSiteswap.Items[uniqueMaxIndex - 1];

        for (var i = max; i >= min; i--)
        {
            if (ShouldStop()) return;
            
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
                    Siteswaps.Add(Siteswap.CreateFromCorrect(PartialSiteswap.Items));
                }
                PartialSiteswap.ResetCurrentPosition();
                continue;
            }

            PartialSiteswap.MoveForward(max);
            BackTrack(i == max ? uniqueMaxIndex + 1 : 0);
            PartialSiteswap.MoveBack();
        }
    }

    private bool ShouldStop()
    {
        // return false;
        return Stopwatch.Elapsed > Input.StopCriteria.TimeOut ||
               Siteswaps.Count > Input.StopCriteria.MaxNumberOfResults;
    }
}