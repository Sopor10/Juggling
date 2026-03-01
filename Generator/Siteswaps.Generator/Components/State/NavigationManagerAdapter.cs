using Microsoft.AspNetCore.Components;

namespace Siteswaps.Generator.Components.State;

public class NavigationManagerAdapter(NavigationManager navigationManager) : INavigation
{
    public void NavigateTo(string uri)
    {
        navigationManager.NavigateTo(uri);
    }
}
