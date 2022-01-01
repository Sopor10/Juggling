using System.Linq;
using FluentAssertions.Collections;

namespace Siteswaps.Generator.Api.Test;

public static class SiteswapExtensions
{
    private static bool Is(this ISiteswap siteswap, params int[] values) => siteswap.Items.SequenceEqual(values);

    public static void Contain(this GenericCollectionAssertions<ISiteswap> siteswapList, params int[] expected) => siteswapList.Contain(x =>x.Is(expected));
}