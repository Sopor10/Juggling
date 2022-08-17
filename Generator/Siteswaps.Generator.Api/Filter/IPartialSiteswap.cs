using System.Collections.ObjectModel;

namespace Siteswaps.Generator.Api.Filter;

public interface IPartialSiteswap
{
    ReadOnlyCollection<int> Items { get; }
    int LastFilledPosition { get;}
    bool IsFilled();
}