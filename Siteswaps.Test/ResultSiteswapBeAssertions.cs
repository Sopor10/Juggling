using FluentAssertions;
using FluentAssertions.Execution;
using Siteswap.Details;

namespace Siteswaps.Test;

public class ResultSiteswapBeAssertions(Result<Siteswap.Details.Siteswap> subject)
    : ResultBeAssertions<Siteswap.Details.Siteswap>(subject)
{
    public AndConstraint<ResultSiteswapBeAssertions> Siteswap(
        Siteswap.Details.Siteswap expected,
        string because = "",
        params object[] becauseArgs
    )
    {
        AssertionChain
            .GetOrCreate()
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject)
            .ForCondition(s => s is Result<Siteswap.Details.Siteswap>.Success)
            .FailWith("Expected {context:result} to be Success, but found Failure.")
            .Then.Given(s => ((Result<Siteswap.Details.Siteswap>.Success)s).Value)
            .ForCondition(value => value.ToString() == expected.ToString())
            .FailWith(
                "Expected {context:result} to be {0}, but found {1}.",
                _ => expected.ToString(),
                value => value?.ToString() ?? "null"
            );

        return new AndConstraint<ResultSiteswapBeAssertions>(this);
    }
}
