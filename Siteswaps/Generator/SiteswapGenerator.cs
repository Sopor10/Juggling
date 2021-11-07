using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Siteswaps.Generator
{

    public class HashsetStack<T>
    {
        private HashSet<T> HashSet { get; } = new ();
        private Stack<T> Stack { get; } = new();

        public void Push(T item)
        {
            if (HashSet.Contains(item))
            {
                return;
            }
            Stack.Push(item);
            HashSet.Add(item);
        }

        public bool TryPop([MaybeNullWhen(false)] out T o)
        {
            return Stack.TryPop(out o);
        }
    }
    public class SiteswapGenerator : ISiteswapGenerator
    {
        private HashsetStack<PartialSiteswap> Stack = new();
        public IEnumerable<Siteswap> Generate(SiteswapGeneratorInput input)
        {
            return GeneratePartialSiteswaps(input)
                .Select(x => x.TryCreateSiteswap())
                .WhereNotNull();
        }

        private IEnumerable<PartialSiteswap> GeneratePartialSiteswaps(SiteswapGeneratorInput input)
        {
            for (var i = 0; i <= input.MaxHeight; i++)
            {
                var partialSiteswap = PartialSiteswap.Standard(input.Period, i);
                if (!input.Filter.CanFulfill(partialSiteswap, input)) continue;
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
                    if (!input.Filter.CanFulfill(siteswap, input)) continue;

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
            var currentIndex = current.CurrentIndex();
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
}