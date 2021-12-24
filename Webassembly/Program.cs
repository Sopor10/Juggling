using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Siteswaps.Generator;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;
using Siteswaps.Generator.Components;
using Siteswaps.Generator.Filter;
using Webassembly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddTransient<ISiteswapGeneratorFactory, SiteswapGeneratorFactory>()
    .AddTransient<IFilterBuilder, FilterBuilder>()
    .AddFluxor(options => options.ScanAssemblies(typeof(AssemblyInfo).Assembly));

await builder.Build().RunAsync();