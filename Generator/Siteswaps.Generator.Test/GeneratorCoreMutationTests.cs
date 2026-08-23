using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Test;

internal sealed class RecordingFilter(bool result, bool isRotationAware = false) : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value) => result;

    public bool IsRotationAware => isRotationAware;
}
