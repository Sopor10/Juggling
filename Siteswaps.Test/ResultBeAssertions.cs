using FluentAssertions;
using FluentAssertions.Execution;
using Siteswap.Details;

namespace Siteswaps.Test;

public class ResultBeAssertions<T>(Result<T> subject)
{
    protected Result<T> Subject { get; } = subject;

    public AndConstraint<ResultBeAssertions<T>> Error(
        string expectedError,
        string because = "",
        params object[] becauseArgs
    )
    {
        AssertionChain
            .GetOrCreate()
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject)
            .ForCondition(s => s is Result<T>.Failure)
            .FailWith("Expected {context:result} to be Failure, but found Success.")
            .Then.Given(s => ((Result<T>.Failure)s).Error)
            .ForCondition(error => error == expectedError)
            .FailWith(
                "Expected {context:result} to have error {0}, but found {1}.",
                _ => expectedError,
                error => error
            );

        return new AndConstraint<ResultBeAssertions<T>>(this);
    }
}
