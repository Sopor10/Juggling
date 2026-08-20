using System.Globalization;
using System.Text.RegularExpressions;

namespace Siteswaps.E2ETests.Design;

/// <summary>Helpers for comparing CSS colors from Playwright computed styles against Passing Zone brand hexes.</summary>
public static partial class DesignColor
{
    public const string BrandPurple800 = "#33225d";
    public const string BrandPurple700 = "#3c286d";
    public const string BrandPurple600 = "#472779";
    public const string BrandPurple500 = "#552f8c";
    public const string BrandPurple950 = "#241a3d";
    public const string BrandOrange = "#f9a500";
    public const string BrandCyan = "#00b3ff";
    public const string BrandLavenderBg = "#f5f3fb";
    public const string BrandPurple100 = "#e8e1f7";
    public const string LegacyMaterialPurple = "#8e44ad";

    public static (byte R, byte G, byte B) FromHex(string hex)
    {
        var value = hex.Trim().TrimStart('#');
        if (value.Length == 3)
        {
            value = string.Concat(value[0], value[0], value[1], value[1], value[2], value[2]);
        }

        return (
            byte.Parse(value[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            byte.Parse(value[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            byte.Parse(value[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture)
        );
    }

    public static bool EqualsHex(string cssColor, string expectedHex, int tolerance = 2)
    {
        if (!TryParseCssRgb(cssColor, out var actual))
        {
            return string.Equals(
                NormalizeHex(cssColor),
                NormalizeHex(expectedHex),
                StringComparison.OrdinalIgnoreCase
            );
        }

        var expected = FromHex(expectedHex);
        return Math.Abs(actual.R - expected.R) <= tolerance
            && Math.Abs(actual.G - expected.G) <= tolerance
            && Math.Abs(actual.B - expected.B) <= tolerance;
    }

    public static bool CssContainsHex(string cssValue, string expectedHex, int tolerance = 2)
    {
        if (string.IsNullOrWhiteSpace(cssValue))
        {
            return false;
        }

        if (
            cssValue.Contains(NormalizeHex(expectedHex), StringComparison.OrdinalIgnoreCase)
            || cssValue.Contains(expectedHex.TrimStart('#'), StringComparison.OrdinalIgnoreCase)
        )
        {
            return true;
        }

        var expected = FromHex(expectedHex);
        foreach (Match match in RgbaRegex().Matches(cssValue))
        {
            var r = byte.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var g = byte.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            var b = byte.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
            if (
                Math.Abs(r - expected.R) <= tolerance
                && Math.Abs(g - expected.G) <= tolerance
                && Math.Abs(b - expected.B) <= tolerance
            )
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryParseCssRgb(string cssColor, out (byte R, byte G, byte B) rgb)
    {
        rgb = default;
        if (!TryParseCssRgba(cssColor, out var rgba))
        {
            return false;
        }

        rgb = (rgba.R, rgba.G, rgba.B);
        return true;
    }

    public static bool TryParseCssRgba(string cssColor, out (byte R, byte G, byte B, double A) rgba)
    {
        rgba = default;
        if (string.IsNullOrWhiteSpace(cssColor))
        {
            return false;
        }

        var trimmed = cssColor.Trim();
        if (trimmed.StartsWith('#'))
        {
            var rgb = FromHex(trimmed);
            rgba = (rgb.R, rgb.G, rgb.B, 1);
            return true;
        }

        var match = RgbaRegex().Match(trimmed);
        if (!match.Success)
        {
            return false;
        }

        var alpha = match.Groups[4].Success
            ? double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture)
            : 1d;
        rgba = (
            byte.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            byte.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
            byte.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
            alpha
        );
        return true;
    }

    public static double ParseCssPx(string cssLength)
    {
        if (string.IsNullOrWhiteSpace(cssLength))
        {
            return 0;
        }

        var match = PxRegex().Match(cssLength.Trim());
        return match.Success
            ? double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : 0;
    }

    public static string NormalizeHex(string hex) => hex.Trim().TrimStart('#').ToLowerInvariant();

    [GeneratedRegex(
        @"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)(?:\s*,\s*([\d.]+))?",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex RgbaRegex();

    [GeneratedRegex(@"^(-?[\d.]+)px$", RegexOptions.IgnoreCase)]
    private static partial Regex PxRegex();
}
