using Blazored.LocalStorage;
using Fluxor;

namespace Siteswaps.Generator.Components.State;

public class LoadSettingsEffect(ILocalStorageService localStorageService) : Effect<LoadSettings>
{
    public override async Task HandleAsync(LoadSettings action, IDispatcher dispatcher)
    {
        var settings = await localStorageService.GetItemAsync<Settings.SettingsDto>("settings");

        dispatcher.Dispatch(new SettingsLoadedAction(settings ?? new()));
    }
}
