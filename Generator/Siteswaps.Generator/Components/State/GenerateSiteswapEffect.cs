using Fluxor;

namespace Siteswaps.Generator.Components.State;

using Microsoft.AspNetCore.Components;

public class GenerateSiteswapEffect : Effect<GenerateSiteswapsAction>
{
    public GenerateSiteswapEffect(NavigationManager navigationManager)
    {
        NavigationManager = navigationManager;
    }

    private NavigationManager NavigationManager { get; }

    public override async Task HandleAsync(GenerateSiteswapsAction action, IDispatcher dispatcher)
    {
        var siteswaps = await new MultipleSiteswapGenerator(action.State).GenerateAsync().ToListAsync();

        dispatcher.Dispatch(new SiteswapsGeneratedAction(siteswaps));
        NavigationManager.NavigateTo("/result");
    }
}
