using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Siteswaps.Generator;
using Webassembly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder
    .Services
    .InstallGenerator();

builder
    .Services.AddScoped<DialogService>();

await builder.Build().RunAsync();