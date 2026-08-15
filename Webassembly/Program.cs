using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Siteswaps.Generator;
using Siteswaps.Generator.Components;
using VisNetwork.Blazor;

namespace Webassembly;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.InstallGenerator();
        builder.Services.AddLocalization();
        builder.Services.AddVisNetwork();

        var host = builder.Build();

        var js = host.Services.GetRequiredService<IJSRuntime>();
        var cultureName = AppCultures.Normalize(
            await js.InvokeAsync<string>("blazorCulture.getPreferred")
        );
        var culture = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        await js.InvokeVoidAsync("blazorCulture.setDocumentLang", cultureName);

        await host.RunAsync();
    }
}
