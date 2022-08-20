using System.Diagnostics;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.NewGenerator;

public class SiteswapGenerator : ISiteswapGenerator
{
    public SiteswapGenerator(ISiteswapFilter filter, SiteswapGeneratorInput input)
    {
        Filter = filter;
        Input = input;
        PartialSiteswap = PartialSiteswap.Standard(Input.Period, Input.MaxHeight);
    }

    private Stopwatch Stopwatch { get; set; }

    private HashSet<ISiteswap> Siteswaps { get; } = new();
    private ISiteswapFilter Filter { get; }
    private SiteswapGeneratorInput Input { get; }
    private PartialSiteswap PartialSiteswap { get; }

    public async Task<IEnumerable<ISiteswap>> GenerateAsync()
    {
        Stopwatch = new Stopwatch();
        Stopwatch.Start();

        await Task.Run(() => BackTrack(0));

        return Siteswaps;
    }

    private void BackTrack(int uniqueMaxIndex)
    {
        var min = Input.MinHeight;
        var max = PartialSiteswap.Items[uniqueMaxIndex];

        for (var i = max; i >= min; i--)
        {
            if (ShouldStop()) return;
            if (PartialSiteswap.FillCurrentPosition(i) is false)
            {
                continue;
            }

            if (Filter.CanFulfill(PartialSiteswap) is false)
            {
                PartialSiteswap.FillCurrentPosition(-1);
                continue;
            }

            if (PartialSiteswap.IsFilled())
            {
                Siteswaps.Add(Siteswap.CreateFromCorrect(PartialSiteswap.Items));
                PartialSiteswap.FillCurrentPosition(-1);
                continue;
            }

            PartialSiteswap.MoveForward(max);
            BackTrack(i == max ? uniqueMaxIndex + 1 : 0);
            PartialSiteswap.MoveBack();
        }
    }

    private bool ShouldStop()
    {
        return Stopwatch.Elapsed > Input.StopCriteria.TimeOut ||
               Siteswaps.Count > Input.StopCriteria.MaxNumberOfResults;
    }
}