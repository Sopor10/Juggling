using Fluxor;
using Radzen;

namespace Siteswaps.Generator.Components.Internal.EasyFilter;

public class CloseDialogAfterAddingFilterEffect(DialogService dialogService)
    : Effect<Filter.NewFilterCreatedAction>
{
    public override Task HandleAsync(Filter.NewFilterCreatedAction action, IDispatcher dispatcher)
    {
        dialogService.Close();
        return Task.CompletedTask;
    }
}
