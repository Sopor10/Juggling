using Fluxor;
using Radzen;

namespace Siteswaps.Generator.Components.State;

public class CloseDialogAfterChangingFilterEffect(DialogService dialogService)
    : Effect<ChangedFilterAction>
{
    public override Task HandleAsync(ChangedFilterAction action, IDispatcher dispatcher)
    {
        dialogService.Close();
        return Task.CompletedTask;
    }
}
