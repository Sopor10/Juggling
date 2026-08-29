using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Localization;
using Siteswaps.Generator.Components;
using Webassembly.Components.DesignTests;

namespace Siteswaps.Design.TestHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents();
        builder.Services.AddLocalization();
        // ResultsView injects ILocalStorageService; unused during static SSR (no OnAfterRender).
        builder.Services.AddBlazoredLocalStorage();

        var supportedCultures = AppCultures.Supported.Select(c => new CultureInfo(c)).ToList();
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(AppCultures.Default);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.RequestCultureProviders =
            [
                new AcceptLanguageHeaderRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new QueryStringRequestCultureProvider(),
            ];
        });

        var app = builder.Build();

        app.UseRequestLocalization();
        app.UseAntiforgery();
        app.MapStaticAssets();
        app.MapRazorComponents<Components.App>()
            .AddAdditionalAssemblies(typeof(DesignTestComponentAttribute).Assembly);

        app.Run();
    }
}
