using Microsoft.Playwright;

namespace Siteswaps.E2ETests;

/// <summary>
/// E2E assertions are written against the German UI. Force <c>de</c> so CI runners
/// (typically <c>en</c> browser locales) do not flake on localized copy/aria-labels.
/// </summary>
internal static class E2ECulture
{
    public const string Locale = "de-DE";

    public const string BlazorCulture = "de";

    private const string InitScript = "window.localStorage.setItem('BlazorCulture', 'de');";

    public static BrowserNewContextOptions NewContextOptions() => new() { Locale = Locale };

    public static async Task InstallAsync(IBrowserContext? context)
    {
        if (context is null)
        {
            return;
        }

        await context.AddInitScriptAsync(InitScript);
    }
}
