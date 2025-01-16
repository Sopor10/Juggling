using Fluxor;
using Radzen;

namespace Siteswaps.Generator.Components.State;

public class CloseDialogAfterAddingFilterEffect(DialogService dialogService)
    : Effect<NewFilterCreatedAction>
{
    public override Task HandleAsync(NewFilterCreatedAction action, IDispatcher dispatcher)
    {
        dialogService.Close();
        return Task.CompletedTask;
    }
}
