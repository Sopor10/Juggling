using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;
using Siteswaps.Generator.Components;
using Siteswaps.Generator.Domain;
using Siteswaps.Generator.Domain.Filter;

namespace Siteswaps.Generator.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static void InstallGenerator(this IServiceCollection services)
    {
        services.AddTransient<ISiteswapGeneratorFactory, SiteswapGeneratorFactory>()
            .AddTransient<IFilterBuilderFactory, FilterBuilderFactory>()
            .AddFluxor(options => options.ScanAssemblies(typeof(Components.AssemblyInfo).Assembly));
    }
}
