using System.Diagnostics;

namespace Siteswaps.Design.Tests.Infrastructure;

internal static class Timing
{
    public static bool Enabled { get; } =
        string.Equals(
            Environment.GetEnvironmentVariable("DESIGN_TEST_TIMING"),
            "1",
            StringComparison.Ordinal
        );

    public static void Log(Stopwatch? sw, string phase)
    {
        if (sw is null)
        {
            return;
        }

        Console.Error.WriteLine($"[design-timing] {phase}={sw.ElapsedMilliseconds}ms");
    }
}
