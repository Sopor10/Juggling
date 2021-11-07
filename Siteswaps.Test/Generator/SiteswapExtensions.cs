using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;

namespace Siteswaps.Test.Generator
{
    public static class SiteswapExtensions
    {
        public static bool Is(this Siteswap siteswap, params int[] values)
        {
            return siteswap.Items.EnumerateValues(1).SequenceEqual(values);
        }    
        
        public static AndWhichConstraint<GenericCollectionAssertions<Siteswap>, Siteswap> Contain(this GenericCollectionAssertions<Siteswap> siteswapList, params int[] expected)
        {
            return siteswapList.Contain(x =>x.Is(expected));
        }
    }
}