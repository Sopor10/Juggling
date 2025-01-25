using Blazored.LocalStorage;
using Fluxor;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components;

public record LoadSettings;



public record SettingsLoadedAction(Settings.SettingsDto Settings);

public class LoadSettingsEffect(ILocalStorageService localStorageService) : Effect<LoadSettings>
{
    public override async Task HandleAsync(LoadSettings action, IDispatcher dispatcher)
    {
        var settings = await localStorageService.GetItemAsync<Settings.SettingsDto>("settings");

        dispatcher.Dispatch(new SettingsLoadedAction(settings ?? new()));
    }
    
    
}

public static class Reducer
{

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceLoadedSettings(
        SiteswapGeneratorState state,
        SettingsLoadedAction action
    )
    {
        return state with { State = state.State with { Settings = action.Settings } };
    }
}
