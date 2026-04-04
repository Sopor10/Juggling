using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Siteswaps.Generator;
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
        builder.Services.AddMudServices();
        builder.Services.AddVisNetwork();

        await builder.Build().RunAsync();
    }
}
