using Fluxor;
using Radzen;

namespace Siteswaps.Generator.Components.Internal.EasyFilter;

public class CloseDialogAfterChangingFilterEffect(DialogService dialogService)
    : Effect<Filter.ChangedFilterAction>
{
    public override Task HandleAsync(Filter.ChangedFilterAction action, IDispatcher dispatcher)
    {
        dialogService.Close();
        return Task.CompletedTask;
    }
}
