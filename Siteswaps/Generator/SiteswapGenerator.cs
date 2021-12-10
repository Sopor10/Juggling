using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Generator;

public class SiteswapGenerator : ISiteswapGenerator
{
    public HashsetStack<PartialSiteswap> Stack { get; } = new();
        
    public Task<IEnumerable<Siteswap>> GenerateAsync(SiteswapGeneratorInput input)
    {
        return Task.Run(() =>
        {
            var tmp = GeneratePartialSiteswaps(input)
                .Select(x => x.TryCreateSiteswap())
                .WhereNotNull()
                .Distinct()
                .ToList()
                .AsEnumerable();
            Stack.Reset();
            return tmp;
        });

    }

    private IEnumerable<PartialSiteswap> GeneratePartialSiteswaps(SiteswapGeneratorInput input)
    {
        var siteswapFilter = input.Filter.Combine(new FilterFactory().Standard());

        for (var i = 0; i <= input.MaxHeight; i++)
        {
            var partialSiteswap = PartialSiteswap.Standard(input.Period, i);
            if (!siteswapFilter.CanFulfill(partialSiteswap, input)) continue;
            Stack.Push(partialSiteswap);
        }
            
        while (Stack.TryPop(out var partialSiteswap))
        {
            if (partialSiteswap.IsFilled())
            {
                yield return partialSiteswap;
                continue;
            }

            foreach (var siteswap in GenerateNext(partialSiteswap, input))
            {
                if (!siteswapFilter.CanFulfill(siteswap, input)) continue;

                Stack.Push(siteswap);
            }
        }
    }

    private IEnumerable<PartialSiteswap> GenerateNext(PartialSiteswap current, SiteswapGeneratorInput input)
    {
        var nextPosition = CreateNextFilledPosition(current, input);
        if (nextPosition is not null)
        {
            foreach (var siteswap in ThisPositionWithLower(nextPosition, input))
            {
                yield return siteswap;
            }
            yield return nextPosition;
        }
    }

    private IEnumerable<PartialSiteswap> ThisPositionWithLower(PartialSiteswap current, SiteswapGeneratorInput input)
    {
        for (var i =  input.MinHeight; i < current.ValueAtCurrentIndex() ; i++)
        {
            yield return current.WithLastFilledPosition(i);
        }
    }

    private PartialSiteswap? CreateNextFilledPosition(PartialSiteswap current, SiteswapGeneratorInput input)
    {
        var currentIndex = current.LastFilledPosition;
        if (currentIndex < 0 || currentIndex >= current.Period() - 1)
        {
            return null;
        }

        return current.SetPosition(currentIndex + 1, GetNextMax(current, input));
    }

    private int GetNextMax(PartialSiteswap current, SiteswapGeneratorInput input)
    {
        return new[]
        {
            current.MaxForNextFree(),
            input.MaxHeight
        }.Min();
    }
}