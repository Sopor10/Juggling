using Fluxor;
using Siteswaps.Generator.Components;

namespace Siteswaps.Generator.Components.Internal.EasyFilter;

public class CloseDialogAfterChangingFilterEffect(DialogTracker dialogTracker)
    : Effect<Filter.ChangedFilterAction>
{
    public override Task HandleAsync(Filter.ChangedFilterAction action, IDispatcher dispatcher)
    {
        dialogTracker.Close();
        return Task.CompletedTask;
    }
}
