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
            Stack.Push(PartialSiteswap.Standard(input.Period, input.MaxHeight));
            while (Stack.TryPop(out var partialSiteswap))
            {
                if (!input.Filter.CanFulfill(partialSiteswap, input)) continue;
                
                if (partialSiteswap.IsFilled())
                {
                    yield return partialSiteswap;
                    continue;
                }

                foreach (var siteswap in GenerateNext(partialSiteswap, input))
                {
                    Stack.Push(siteswap);
                }
            }
        }

        private IEnumerable<PartialSiteswap> GenerateNext(PartialSiteswap current, SiteswapGeneratorInput input)
        {
            foreach (var siteswap in ThisPositionWithLower(current, input))
            {
                yield return siteswap;
            }
            
            var nextPosition = CreateNextFilledPosition(current, input);
            if (nextPosition is not null)
            {
                yield return nextPosition;
            }
        }

        private IEnumerable<PartialSiteswap> ThisPositionWithLower(PartialSiteswap current, SiteswapGeneratorInput input)
        {
            for (var i =  input.MinHeight; i <= new[] { current.Max(), input.MaxHeight}.Min() ; i++)
            {
                yield return current.SetPosition(current.CurrentIndex(), i);
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