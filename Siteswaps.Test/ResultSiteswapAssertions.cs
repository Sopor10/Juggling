using Siteswap.Details;

namespace Siteswaps.Test;

public class ResultSiteswapAssertions(Result<Siteswap.Details.Siteswap> instance)
    : ResultAssertions<Siteswap.Details.Siteswap>(instance)
{
    public override ResultSiteswapBeAssertions Be() => new(Subject);
}
