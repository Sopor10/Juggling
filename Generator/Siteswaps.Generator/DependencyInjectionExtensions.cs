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
#if DEBUG && FLUXOR_REDUX_DEVTOOLS
            // Optional: build with -p:DefineConstants=FLUXOR_REDUX_DEVTOOLS for Chrome/Edge + extension.
            // Cursor's embedded Electron browser has no Redux DevTools extension and Fluxor's
            // middleware uses eval — leaving it enabled keeps the app stuck on the boot splash.
            options.UseReduxDevTools();
#endif
        });
        services.AddScoped<INavigation, NavigationManagerAdapter>();

        services.AddBlazoredLocalStorage();
    }
}
