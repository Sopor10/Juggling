using System.Diagnostics;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator;

internal class SiteswapGenerator : ISiteswapGenerator
{
    public SiteswapGenerator(ISiteswapFilter siteswapFilter, SiteswapGeneratorInput input)
    {
        SiteswapFilter = siteswapFilter;
        Input = input;
    }

    private ISiteswapFilter SiteswapFilter { get; set; }
    public SiteswapGeneratorInput Input { get; }
    private HashsetStack<PartialSiteswap> Stack { get; } = new();

    public Task<IEnumerable<ISiteswap>> GenerateAsync()
    {
        return Task.Run(() =>
        {
            var tmp = GeneratePartialSiteswaps()
                .Distinct()
                .Select(x => x as ISiteswap)
                .ToList()
                .AsEnumerable();
            Stack.Reset();
            return tmp;
        });
    }

    private IEnumerable<Siteswap> GeneratePartialSiteswaps()
    {
        var count = 0;
        var sw = new Stopwatch();
        sw.Start();

        for (var i = 0; i <= Input.MaxHeight; i++)
        {
            var partialSiteswap = PartialSiteswap.Standard(Input.Period, i);
            if (!SiteswapFilter.CanFulfill(partialSiteswap)) continue;
            Stack.Push(partialSiteswap);
        }

        while (Stack.TryPop(out var partialSiteswap))
        {
            if (sw.Elapsed > Input.StopCriteria.TimeOut || count > Input.StopCriteria.MaxNumberOfResults) yield break;

            if (partialSiteswap.IsFilled() && Siteswap.TryCreate(partialSiteswap.Items, out var s))
            {
                count++;
                yield return s;
                continue;
            }

            foreach (var siteswap in GenerateNext(partialSiteswap, Input))
            {
                if (!SiteswapFilter.CanFulfill(siteswap)) continue;

                Stack.Push(siteswap);
            }
        }
    }

    private IEnumerable<PartialSiteswap> GenerateNext(PartialSiteswap current, SiteswapGeneratorInput input)
    {
        var nextPosition = CreateNextFilledPosition(current, input);
        if (nextPosition is not null)
        {
            foreach (var siteswap in ThisPositionWithLower(nextPosition, input)) yield return siteswap;
            yield return nextPosition;
        }
    }

    private IEnumerable<PartialSiteswap> ThisPositionWithLower(PartialSiteswap current, SiteswapGeneratorInput input)
    {
        for (var i = input.MinHeight; i < current.ValueAtCurrentIndex(); i++)
            yield return current.WithLastFilledPosition(i);
    }

    private static PartialSiteswap? CreateNextFilledPosition(PartialSiteswap current, SiteswapGeneratorInput input)
    {
        var currentIndex = current.LastFilledPosition;
        if (currentIndex < 0 || currentIndex >= input.Period - 1) return null;

        return current.SetPosition(currentIndex + 1, GetNextMax(current, input));
    }

    private static int GetNextMax(PartialSiteswap current, SiteswapGeneratorInput input)
    {
        return new[]
        {
            current.MaxForNextFree(),
            input.MaxHeight
        }.Min();
    }
}