using System.Globalization;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.Feeding;

/// <summary>
/// Local / Global / Name throw formatting for Feeding, matching the details-page
/// <c>JugglerBeatRows</c> / <c>DetailViewModel.ThrowDisplayMode</c> behaviour
/// (same rules as <c>Siteswap.Details.PassingThrowNames</c>).
/// </summary>
public static class FeedingThrowDisplay
{
    public enum Mode
    {
        Local,
        Global,
        Name,
    }

    public readonly record struct Chip(string Display, PassOrSelf Kind, int Height, int LocalBeat);

    private sealed record NamedThrow(string Display, int BaseHeight);

    /// <summary>Same catalog as Siteswap.Details.PassingThrowNames.</summary>
    private static readonly NamedThrow[] Catalog =
    [
        new("Zip", 2),
        new("Hold", 4),
        new("Zap", 5),
        new("Self", 6),
        new("Single", 7),
        new("Heff", 8),
        new("Double", 9),
        new("Triple S", 10),
        new("Triple", 11),
        new("Quad", 12),
        new("Quad Pass", 13),
    ];

    public static Mode ParseMode(string? value) =>
        Enum.TryParse<Mode>(value, ignoreCase: true, out var mode) ? mode : Mode.Global;

    public static IReadOnlyList<Chip> ChipsFor(
        Siteswap siteswap,
        int juggler,
        int numberOfJugglers,
        Mode mode
    )
    {
        var localPeriod = siteswap.Period.GetLocalPeriod(numberOfJugglers).Value;
        var chips = new Chip[localPeriod];
        for (var i = 0; i < localPeriod; i++)
        {
            var height = siteswap.Items[(juggler + i * numberOfJugglers) % siteswap.Items.Length];
            chips[i] = new Chip(
                Format(height, numberOfJugglers, mode),
                Kind(height, numberOfJugglers),
                height,
                i
            );
        }

        return chips;
    }

    public static string Format(int height, int numberOfJugglers, Mode mode) =>
        mode switch
        {
            Mode.Local => ToLocalDisplay(height, numberOfJugglers),
            Mode.Name => FormatName(height, numberOfJugglers),
            _ => FormatGlobal(height),
        };

    public static PassOrSelf Kind(int height, int numberOfJugglers) =>
        height % Math.Max(1, numberOfJugglers) == 0 ? PassOrSelf.Self : PassOrSelf.Pass;

    public static string FormatAverage(double average) =>
        average.ToString("0.##", CultureInfo.InvariantCulture);

    private static string FormatGlobal(int height) =>
        height < 10 ? $"{height}" : Convert.ToChar(height + 87).ToString();

    private static string ToLocalDisplay(int height, int numberOfJugglers)
    {
        var jugglers = Math.Max(1, numberOfJugglers);
        return (height * 1.0 / jugglers).ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatName(int height, int numberOfJugglers)
    {
        var jugglers = Math.Max(1, numberOfJugglers);
        foreach (var named in Catalog)
        {
            if (HeightsFor(named.BaseHeight, jugglers).Contains(height))
            {
                return named.Display;
            }
        }

        return ToLocalDisplay(height, jugglers);
    }

    private static HashSet<int> HeightsFor(int baseHeight, int jugglers)
    {
        var result = new HashSet<int>();
        if (baseHeight % 2 == 1)
        {
            var min = baseHeight - 1;
            var max = baseHeight + 1;
            for (var i = min * jugglers + 1; i < max * jugglers; i++)
            {
                var item = i / 2;
                if (item % jugglers != 0)
                {
                    result.Add(item);
                }
            }
        }
        else
        {
            result.Add(baseHeight * jugglers / 2);
        }

        return result;
    }
}
