namespace Siteswaps.Generator.Core.Generator.Filter;

public interface ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value);

    public int Order => 0;

    public bool IsRotationAware => false;

    public bool CanFulfillAnyRotation(PartialSiteswap value)
    {
        if (!IsRotationAware)
        {
            return CanFulfill(value);
        }

        var originalRotation = value.RotationIndex;
        for (var rotation = 0; rotation < value.Length; rotation++)
        {
            value.RotationIndex = rotation;
            if (CanFulfill(value))
            {
                value.RotationIndex = originalRotation;
                return true;
            }
        }

        value.RotationIndex = originalRotation;
        return false;
    }
}
