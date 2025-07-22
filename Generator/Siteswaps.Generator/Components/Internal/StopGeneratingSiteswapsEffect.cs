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
        // I don't know of any other way to stop execution of another effect, that is running in the background.
        // we don't need to dispatch any other actions, as this is a cancellation token source, that is also stored in the state.
        // state should be immutable and this obviosly is not, but I think it is an acceptable tradeoff for now.
        if (state.Value.CancellationTokenSource is null)
        {
            return;
        }
        await state.Value.CancellationTokenSource.CancelAsync();
        dispatcher.Dispatch(new FinishedGeneratingSiteswaps());
    }
}
