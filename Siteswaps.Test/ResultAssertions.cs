using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Siteswap.Details;

namespace Siteswaps.Test;

public class ResultAssertions<T>(Result<T> instance)
    : ReferenceTypeAssertions<Result<T>, ResultAssertions<T>>(
        instance,
        AssertionChain.GetOrCreate()
    )
{
    protected override string Identifier => "result";

    public virtual ResultBeAssertions<T> Be() => new(Subject);
}
