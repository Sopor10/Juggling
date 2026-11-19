extern alias JifDetails;

using System.Text.Json;

namespace Siteswaps.Generator.Components.SiteswapLab;

internal static class SiteswapLabJifMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string MapPassing(
        IReadOnlyList<int> timeZones,
        IReadOnlyList<IReadOnlyList<int>> heights,
        IReadOnlyList<IReadOnlyList<int>> targets,
        string propType
    ) => JsonSerializer.Serialize(
        JifDetails::Siteswap.Details.Jif.JifMapper.MapPassing(timeZones, heights, targets, propType),
        JsonOptions
    );
}
