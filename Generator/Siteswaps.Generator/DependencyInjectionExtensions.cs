using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.Extensions.DependencyInjection;

namespace Siteswaps.Generator;

public static class DependencyInjectionExtensions
{
    public static void InstallGenerator(this IServiceCollection services)
    {
        services.AddFluxor(options =>
        {
            options.ScanAssemblies(typeof(AssemblyInfo).Assembly);
#if DEBUG
            options.UseReduxDevTools();
#endif
        });
    }
}
