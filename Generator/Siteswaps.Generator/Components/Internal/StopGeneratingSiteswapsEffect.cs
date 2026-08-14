using Fluxor;
using Siteswaps.Generator.Components.Internal.Generate;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.Internal;

public record NavigatedAwayFromSiteswapResultListAction;

public class StopGeneratingSiteswapsEffect(IState<SiteswapGeneratorState> state)
    : Effect<NavigatedAwayFromSiteswapResultListAction>
{
    public override async Task HandleAsync(
        NavigatedAwayFromSiteswapResultListAction action,
        IDispatcher dispatcher
    )
    {
        // Cancelling the shared CancellationTokenSource is the only way to stop the background generation effect.
        if (state.Value.CancellationTokenSource is null)
        {
            return;
        }
        await state.Value.CancellationTokenSource.CancelAsync();
        dispatcher.Dispatch(new FinishedGeneratingSiteswaps());
    }
}
