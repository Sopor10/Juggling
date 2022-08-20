using System.Collections.ObjectModel;

namespace Siteswaps.Generator.Api.Filter;

public interface IPartialSiteswap
{
    ReadOnlyCollection<sbyte> Items { get; }
    sbyte LastFilledPosition { get;}
    bool IsFilled();
}