namespace Siteswap.Details;

/// <summary>Shared colors used to identify object orbits across siteswap views.</summary>
public static class OrbitColorPalette
{
    private static readonly string[] Values =
    [
        "#0072B2",
        "#D55E00",
        "#009E73",
        "#CC79A7",
        "#E69F00",
        "#56B4E9",
        "#7A5195",
        "#A05195",
    ];

    public static string For(int orbitIndex) => Values[Math.Abs(orbitIndex) % Values.Length];
}
