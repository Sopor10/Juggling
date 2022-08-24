namespace Siteswaps.Generator.Api.Filter;

public interface IPartialSiteswap
{
    sbyte[] Items { get; }
    sbyte LastFilledPosition { get;}
    sbyte PartialSum { get; }
    bool IsFilled();
}