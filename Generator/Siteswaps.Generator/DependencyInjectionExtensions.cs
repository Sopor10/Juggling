using Blazored.LocalStorage;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Siteswaps.Generator.Components.State;
#if DEBUG
using Fluxor.Blazor.Web.ReduxDevTools;
#endif

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
        services.AddScoped<INavigation, NavigationManagerAdapter>();
        services.AddBlazoredLocalStorage();
    }
}
