using BlazorApplicationInsights;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Siteswaps.Generator;
using VisNetwork.Blazor;
using Webassembly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder
    .Services
    .InstallGenerator();
builder.Services.AddBlazorApplicationInsights();
builder
    .Services.AddScoped<DialogService>();
builder.Services.AddVisNetwork();

await builder.Build().RunAsync();