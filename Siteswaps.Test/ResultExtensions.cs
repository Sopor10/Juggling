using Siteswap.Details;

namespace Siteswaps.Test;

public static class ResultExtensions
{
    public static ResultAssertions<T> Should<T>(this Result<T> instance)
    {
        return new ResultAssertions<T>(instance);
    }

    public static ResultSiteswapAssertions Should(this Result<Siteswap.Details.Siteswap> instance)
    {
        return new ResultSiteswapAssertions(instance);
    }
}
