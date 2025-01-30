using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Siteswaps.Generator;
using VisNetwork.Blazor;
using Webassembly;

namespace Webassembly;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.InstallGenerator();
        builder.Services.AddRadzenComponents();
        builder.Services.AddVisNetwork();

        await builder.Build().RunAsync();
    }
}
