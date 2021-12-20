using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
using Siteswaps.Generator.Api;

namespace Siteswaps.Generator.Test;

public static class SiteswapExtensions
{
    public static bool Is(this ISiteswap siteswap, params int[] values)
    {
        return siteswap.Items.SequenceEqual(values);
    }    
        
    public static AndWhichConstraint<GenericCollectionAssertions<ISiteswap>, ISiteswap> Contain(this GenericCollectionAssertions<ISiteswap> siteswapList, params int[] expected)
    {
        return siteswapList.Contain(x =>x.Is(expected));
    }
}