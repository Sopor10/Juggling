using System.Collections.Immutable;

namespace Siteswaps.Generator.Api.Filter;

public interface IPartialSiteswap
{
    ImmutableList<int> Items { get; }
    int LastFilledPosition { get;}
    bool IsFilled();
}