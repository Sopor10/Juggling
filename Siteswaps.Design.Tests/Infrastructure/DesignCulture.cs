using Microsoft.Playwright;

namespace Siteswaps.Design.Tests.Infrastructure;

/// <summary>
/// Design snapshots use fixed German UI culture (same convention as E2E).
/// </summary>
internal static class DesignCulture
{
    public const string Locale = "de-DE";

    private const string InitScript = "window.localStorage.setItem('BlazorCulture', 'de');";

    public static BrowserNewContextOptions NewContextOptions() =>
        new()
        {
            Locale = Locale,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
        };

    public static Task InstallAsync(IBrowserContext context) =>
        context.AddInitScriptAsync(InitScript);
}
