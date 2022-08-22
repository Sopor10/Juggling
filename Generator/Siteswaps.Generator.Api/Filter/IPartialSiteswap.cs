using System.Collections.ObjectModel;

namespace Siteswaps.Generator.Api.Filter;

public interface IPartialSiteswap
{
    sbyte[] Items { get; }
    sbyte LastFilledPosition { get;}
    sbyte PartialSum { get; set; }
    bool IsFilled();
}