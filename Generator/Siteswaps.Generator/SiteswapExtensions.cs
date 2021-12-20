using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator;

public static class SiteswapExtensions
{
    public static bool TryCreateSiteswap(this PartialSiteswap partialSiteswap, [NotNullWhen(true)] out Siteswap? siteswap)
    {
        siteswap = null;
        return Siteswap.TryCreate(partialSiteswap.Items.ToList(), out siteswap);
    }   
        
    public static Siteswap? TryCreateSiteswap(this PartialSiteswap partialSiteswap)
    {
        Siteswap.TryCreate(partialSiteswap.Items.ToList(), out var siteswap);
        return siteswap;
    }
}