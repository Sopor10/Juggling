using System;
using Fluxor;

namespace Siteswaps.Components.Generator.State;

public static class Reducer
{
    [ReducerMethod]
    public static SiteswapGeneratorState ReduceSiteswapsGeneratedChangedAction(SiteswapGeneratorState state,
        SiteswapsGeneratetAction action) =>
        state with { Siteswaps = action.Siteswaps, State = state.State with{ IsGenerating = false}};

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIsGeneratingAction(SiteswapGeneratorState state,
        GenerateSiteswapsAction _) =>
        state with { State = state.State with{ IsGenerating = true}};

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementPeriodChangedAction(SiteswapGeneratorState state,
        PeriodChangedAction action) =>
        state with { State = state.State with { Period = action.Value } };


    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMinThrowChangedAction(SiteswapGeneratorState state,
        MinThrowChangedAction action) =>
        state with { State = state.State with { MinThrow = action.Value } };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMaxThrowChangedAction(SiteswapGeneratorState state,
        MaxThrowChangedAction action) =>
        state with { State = state.State with { MaxThrow = action.Value } };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementNumberOfJugglersChangedAction(SiteswapGeneratorState state,
        NumberOfJugglersChangedAction action) =>
        state with { State = state.State with { NumberOfJugglers = action.Value } };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementExactNumberChangedAction(SiteswapGeneratorState state,
        ExactNumberChangedAction action) =>
        state with
        {
            State = state.State with
            {
                Objects = state.State.Objects switch
                {
                    Between _ => new ExactNumber { Number = action.Value },
                    ExactNumber _ => new ExactNumber { Number = action.Value },
                    _ => throw new ArgumentOutOfRangeException()
                }
            }
        };


    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMinNumberOfObjectsChangedAction(SiteswapGeneratorState state,
        MinNumberChangedAction action) =>
        state with
        {
            State = state.State with
            {
                Objects = state.State.Objects switch
                {
                    Between between => between with { MinNumber = action.Value },
                    ExactNumber _ => new Between { MinNumber = action.Value },
                    _ => throw new ArgumentOutOfRangeException()
                }
            }
        };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMaxNumberOfObjectsChangedAction(
        SiteswapGeneratorState state,
        MaxNumberChangedAction action) =>
        state with
        {
            State = state.State with
            {
                Objects = state.State.Objects switch
                {
                    Between between => between with { MaxNumber = action.Value },
                    ExactNumber _ => new Between { MaxNumber = action.Value },
                    _ => throw new ArgumentOutOfRangeException()
                }
            }
        };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementExactNumberOrRangeOfBallsSwitchedAction(
        SiteswapGeneratorState state,
        ExactNumberOrRangeOfBallsSwitchedAction action) =>
        state with
        {
            State = state.State with
            {
                Objects = action.Value switch
                {
                    true => new ExactNumber(),
                    false => new Between()
                }
            }
        };
    
    [ReducerMethod]
    public static SiteswapGeneratorState ReduceNewFilterCreatedActionAction(
        SiteswapGeneratorState state,
        NewFilterCreatedAction action) =>
        state with
        {
            State = state.State with
            {
                Filter = state.State.Filter.Add(action.Value)
            }
        };
    
    [ReducerMethod]
    public static SiteswapGeneratorState ReduceNewFilterCreatedAction(
        SiteswapGeneratorState state,
        RemoveFilterNumber action) =>
        state with
        {
            State = state.State with
            {
                Filter = state.State.Filter.RemoveAt(action.Value)
            }
        };
    
    [ReducerMethod]
    public static SiteswapGeneratorState ReduceFilterTypeSelectionChangedAction(
        SiteswapGeneratorState state,
        FilterTypeSelectionChangedAction action) =>
        state with
        {
            NewFilter = state.KnownFilters.GetDefault(action.FilterType)
        };
    
}
