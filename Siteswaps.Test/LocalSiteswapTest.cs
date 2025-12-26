using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Siteswap.Details;

namespace Siteswaps.Test;

public class LocalSiteswapTest
{
    [Test]
    public void FromLocals_With_Incomplete_Jugglers_Should_Return_Error()
    {
        var global = new Siteswap.Details.Siteswap(5, 3, 1);
        var local0 = new LocalSiteswap(global, 0, 3);
        var local1 = new LocalSiteswap(global, 1, 3);

        var result = LocalSiteswap.FromLocals([local0, local1]);

        result.Should().BeError("Expected 3 local siteswaps, but got 2.");
    }

    [Test]
    public void FromLocals_With_Duplicate_Jugglers_Should_Return_Error()
    {
        var global = new Siteswap.Details.Siteswap(5, 3, 1);
        var local0 = new LocalSiteswap(global, 0, 3);
        var local1 = new LocalSiteswap(global, 1, 3);
        var local2 = new LocalSiteswap(global, 1, 3); // Duplicate index 1

        var result = LocalSiteswap.FromLocals([local0, local1, local2]);

        result.Should().BeError("Juggler indices are not unique or incomplete.");
    }

    [Test]
    public void FromLocals_With_Unordered_Input_Should_Work()
    {
        var global = new Siteswap.Details.Siteswap(5, 3, 1);
        var local0 = new LocalSiteswap(global, 0, 3);
        var local1 = new LocalSiteswap(global, 1, 3);
        var local2 = new LocalSiteswap(global, 2, 3);

        var result = LocalSiteswap.FromLocals([local2, local0, local1]);

        result.Should().BeSiteswap(global);
    }

    [Test]
    public void FromLocals_Should_Reconstruct_Siteswap()
    {
        var global = new Siteswap.Details.Siteswap(5, 3, 1); // 3 Jongleure: 5, 3, 1
        var local0 = new LocalSiteswap(global, 0, 3);
        var local1 = new LocalSiteswap(global, 1, 3);
        var local2 = new LocalSiteswap(global, 2, 3);

        var result = LocalSiteswap.FromLocals([local0, local1, local2]);

        result.Should().BeSiteswap(global);
    }

    [Test]
    public void Combining_Two_Period_5_Siteswaps_Results_In_Period_10()
    {
        var local0 = new LocalSiteswap(new("95894"), 0, 2);
        var local1 = new LocalSiteswap(new("92897"), 1, 2);

        var result = LocalSiteswap.FromLocals([local0, local1]);

        result.Should().BeSiteswap(new Siteswap.Details.Siteswap("9289495897"));
    }
}

public static class ResultExtensions
{
    public static ResultAssertions<T> Should<T>(this Result<T> instance)
    {
        return new ResultAssertions<T>(instance);
    }
}

public class ResultAssertions<T> : ReferenceTypeAssertions<Result<T>, ResultAssertions<T>>
{
    public ResultAssertions(Result<T> instance)
        : base(instance, AssertionChain.GetOrCreate()) { }

    protected override string Identifier => "result";

    public AndConstraint<ResultAssertions<T>> BeSiteswap(
        Siteswap.Details.Siteswap expected,
        string because = "",
        params object[] becauseArgs
    )
    {
        AssertionChain
            .GetOrCreate()
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject)
            .ForCondition(subject => subject is Result<T>.Success)
            .FailWith("Expected {context:result} to be Success, but found Failure.")
            .Then.Given(subject => ((Result<T>.Success)subject).Value)
            .ForCondition(value =>
                value is Siteswap.Details.Siteswap s && s.ToString() == expected.ToString()
            )
            .FailWith(
                "Expected {context:result} to be {0}, but found {1}.",
                _ => expected.ToString(),
                value => value?.ToString() ?? "null"
            );

        return new AndConstraint<ResultAssertions<T>>(this);
    }

    public AndConstraint<ResultAssertions<T>> BeError(
        string expectedError,
        string because = "",
        params object[] becauseArgs
    )
    {
        AssertionChain
            .GetOrCreate()
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject)
            .ForCondition(subject => subject is Result<T>.Failure)
            .FailWith("Expected {context:result} to be Failure, but found Success.")
            .Then.Given(subject => ((Result<T>.Failure)subject).Error)
            .ForCondition(error => error == expectedError)
            .FailWith(
                "Expected {context:result} to have error {0}, but found {1}.",
                _ => expectedError,
                error => error
            );

        return new AndConstraint<ResultAssertions<T>>(this);
    }
}
