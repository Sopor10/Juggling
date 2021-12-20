using System.Collections.Immutable;
using Fluxor;

namespace Siteswaps.Generator.Components.State;

public static class Reducer
{
    [ReducerMethod]
    public static SiteswapGeneratorState ReduceSiteswapsGeneratedChangedAction(SiteswapGeneratorState state,
        SiteswapsGeneratetAction action)
    {
        return state with { Siteswaps = action.Siteswaps, State = state.State with { IsGenerating = false } };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIsGeneratingAction(SiteswapGeneratorState state,
        GenerateSiteswapsAction _)
    {
        return state with { State = state.State with { IsGenerating = true } };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementPeriodChangedAction(SiteswapGeneratorState state,
        PeriodChangedAction action)
    {
        return state with
        {
            State = state.State with { Period = action.Value },
            NewFilter = (state.NewFilter is PatternFilterInformation patternFilterInformation) ? new PatternFilterInformation(Enumerable.Repeat(-1, action.Value).ToImmutableArray()): state.NewFilter,
            
        };
    }


    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMinThrowChangedAction(SiteswapGeneratorState state,
        MinThrowChangedAction action)
    {
        return state with { State = state.State with { MinThrow = action.Value } };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMaxThrowChangedAction(SiteswapGeneratorState state,
        MaxThrowChangedAction action)
    {
        return state with { State = state.State with { MaxThrow = action.Value } };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementNumberOfJugglersChangedAction(SiteswapGeneratorState state,
        NumberOfJugglersChangedAction action)
    {
        return state with { State = state.State with { NumberOfJugglers = action.Value } };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementExactNumberChangedAction(SiteswapGeneratorState state,
        ExactNumberChangedAction action)
    {
        return state with
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
    }


    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMinNumberOfObjectsChangedAction(SiteswapGeneratorState state,
        MinNumberChangedAction action)
    {
        return state with
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
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMaxNumberOfObjectsChangedAction(
        SiteswapGeneratorState state,
        MaxNumberChangedAction action)
    {
        return state with
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
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementExactNumberOrRangeOfBallsSwitchedAction(
        SiteswapGeneratorState state,
        ExactNumberOrRangeOfBallsSwitchedAction action)
    {
        return state with
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
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceNewFilterCreatedActionAction(
        SiteswapGeneratorState state,
        NewFilterCreatedAction action)
    {
        return state with
        {
            State = state.State with
            {
                Filter = state.State.Filter.Add(action.Value)
            }
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceNewFilterCreatedAction(
        SiteswapGeneratorState state,
        RemoveFilterNumber action)
    {
        return state with
        {
            State = state.State with
            {
                Filter = state.State.Filter.RemoveAt(action.Value)
            }
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceFilterTypeSelectionChangedAction(
        SiteswapGeneratorState state,
        FilterTypeSelectionChangedAction action)
    {
        return state with
        {
            NewFilter = action.FilterType switch
            {
                FilterType.Number => new NumberFilterInformation(),
                FilterType.Pattern => new PatternFilterInformation(Enumerable.Repeat(-1, state.State.Period).ToImmutableArray()),
                _ => throw new ArgumentOutOfRangeException()
            }
        };
    }
}