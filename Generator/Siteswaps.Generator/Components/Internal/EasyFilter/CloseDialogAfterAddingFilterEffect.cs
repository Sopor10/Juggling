using Fluxor;
using Siteswaps.Generator.Components;

namespace Siteswaps.Generator.Components.Internal.EasyFilter;

public class CloseDialogAfterAddingFilterEffect(DialogTracker dialogTracker)
    : Effect<Filter.NewFilterCreatedAction>
{
    public override Task HandleAsync(Filter.NewFilterCreatedAction action, IDispatcher dispatcher)
    {
        dialogTracker.Close();
        return Task.CompletedTask;
    }
}
