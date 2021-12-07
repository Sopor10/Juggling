using Fluxor;

namespace Siteswaps.Components.Generator.State;

[FeatureState]
public class SiteswapGeneratorState 
{
    public SiteswapGeneratorState()
    {
        State = new();
    }

    public SiteswapGeneratorState(GeneratorState state)
    {
        State = state;
    }
    
    public GeneratorState State { get; }
    
}

public record GenerateSiteswapsAction;
public record PeriodChangedAction(int Value);
public record ExactNumberChangedAction(int Value);
public record MinNumberChangedAction(int Value);
public record MaxNumberChangedAction(int Value);
public record NumberOfJugglersChangedAction(int Value);
public record MinThrowChangedAction(int Value);
public record MaxThrowChangedAction(int Value);

public static class Reducers
{
    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementCounterAction(SiteswapGeneratorState state,
        PeriodChangedAction action) =>
        new(state.State with { Period = action.Value});
}