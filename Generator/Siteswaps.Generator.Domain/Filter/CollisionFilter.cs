﻿using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class CollisionFilter : ISiteswapFilter
{
    public bool CanFulfill(IPartialSiteswap value)
    {
        var bools = new bool [ value.Items.Count ];

        var currentIndex = value.LastFilledPosition;
        for (var i = 0; i <= currentIndex; i++)
        {
            var count = (i + value.Items[i]) % value.Items.Count;
            bools[count] = true;
        }

        return bools.Count(x => x) == currentIndex + 1;
    }
}