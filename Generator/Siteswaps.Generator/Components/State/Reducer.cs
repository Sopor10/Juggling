using System.Collections.Immutable;
using Fluxor;
using MoreLinq.Extensions;
using Siteswaps.Generator.Components.Internal.EasyFilter;

namespace Siteswaps.Generator.Components.State;

public static class Reducer
{
    [ReducerMethod]
    public static SiteswapGeneratorState ReduceSingleSiteswapsGeneratedChangedAction(
        SiteswapGeneratorState state,
        SiteswapGeneratedAction action
    ) => state with { Siteswaps = state.Siteswaps.AddRange(action.Siteswaps) };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIsGeneratingAction(
        SiteswapGeneratorState state,
        GenerateSiteswapsAction action
    ) => state with { Siteswaps = [], CancellationTokenSource = action.CancellationTokenSource };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementPeriodChangedAction(
        SiteswapGeneratorState state,
        PeriodChangedAction action
    ) =>
        state with
        {
            State = state.State with
            {
                Period = action.Value,
                Filter = state
                    .State.Filter.Where(x => x is not NewPatternFilterInformation)
                    .ToImmutableList(),
            },
        };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMinThrowChangedAction(
        SiteswapGeneratorState state,
        MinThrowChangedAction action
    )
    {
        return state with { State = state.State with { MinThrow = action.Value } };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMaxThrowChangedAction(
        SiteswapGeneratorState state,
        MaxThrowChangedAction action
    )
    {
        return state with { State = state.State with { MaxThrow = action.Value } };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementNumberOfJugglersChangedAction(
        SiteswapGeneratorState state,
        NumberOfJugglersChangedAction action
    )
    {
        return state with { State = state.State with { NumberOfJugglers = action.Value } };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementExactNumberChangedAction(
        SiteswapGeneratorState state,
        ExactNumberChangedAction action
    )
    {
        return state with
        {
            State = state.State with
            {
                Objects = state.State.Objects switch
                {
                    Between _ => new ExactNumber { Number = action.Value },
                    ExactNumber _ => new ExactNumber { Number = action.Value },
                    _ => throw new ArgumentOutOfRangeException(),
                },
            },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMinNumberOfObjectsChangedAction(
        SiteswapGeneratorState state,
        MinNumberChangedAction action
    )
    {
        return state with
        {
            State = state.State with
            {
                Objects = state.State.Objects switch
                {
                    Between between => between with { MinNumber = action.Value },
                    ExactNumber _ => new Between { MinNumber = action.Value },
                    _ => throw new ArgumentOutOfRangeException(),
                },
            },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementMaxNumberOfObjectsChangedAction(
        SiteswapGeneratorState state,
        MaxNumberChangedAction action
    )
    {
        return state with
        {
            State = state.State with
            {
                Objects = state.State.Objects switch
                {
                    Between between => between with { MaxNumber = action.Value },
                    ExactNumber _ => new Between { MaxNumber = action.Value },
                    _ => throw new ArgumentOutOfRangeException(),
                },
            },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceIncrementExactNumberOrRangeOfBallsSwitchedAction(
        SiteswapGeneratorState state,
        ExactNumberOrRangeOfBallsSwitchedAction action
    )
    {
        return state with
        {
            State = state.State with
            {
                Objects = action.Value switch
                {
                    true => new ExactNumber(),
                    false => new Between(),
                },
            },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceNewFilterCreatedActionAction(
        SiteswapGeneratorState state,
        NewFilterCreatedAction action
    )
    {
        return state with
        {
            State = state.State with { Filter = state.State.Filter.Add(action.Value) },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceFilterChangedActionAction(
        SiteswapGeneratorState state,
        ChangedFilterAction action
    )
    {
        return state with
        {
            State = state.State with
            {
                Filter = state
                    .State.Filter.RemoveAt(action.FilterNumber)
                    .Insert(action.FilterNumber, action.NewPatternFilterInformation),
            },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceNewFilterCreatedAction(
        SiteswapGeneratorState state,
        RemoveFilterNumber action
    )
    {
        return state with
        {
            State = state.State with { Filter = state.State.Filter.RemoveAt(action.Value) },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceSetState(
        SiteswapGeneratorState state,
        SetState action
    )
    {
        return state with { State = action.State };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceThrowsChangedAction(
        SiteswapGeneratorState state,
        ThrowsChangedAction action
    )
    {
        return state with
        {
            State = state.State with
            {
                Throws = action.Throws.ToImmutableList(),
                Filter = state
                    .State.Filter.Where(x =>
                        !(
                            x is EasyNumberFilter.NumberFilter numberFilter
                            && (
                                action
                                    .Throws.ToImmutableList()
                                    .AddRange(Throw.AllWildCards)
                                    .Contains(numberFilter.Throw)
                                is false
                            )
                        )
                    )
                    .Where(x =>
                        !(
                            x is NewPatternFilterInformation newPatternFilterInformation
                            && (
                                newPatternFilterInformation.Pattern.Any(x =>
                                    action.Throws.Contains(x) is false
                                )
                            )
                        )
                    )
                    .ToImmutableList(),
            },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceCreateFilterFromThrowList(
        SiteswapGeneratorState state,
        CreateFilterFromThrowList action
    )
    {
        return state with
        {
            State = state.State with
            {
                CreateFilterFromThrowList = action.Value,
                Objects = (action.Value, state.State.CreateFilterFromThrowList) switch
                {
                    (true, false) => new Between() { MinNumber = 6, MaxNumber = 6 },
                    (false, true) => new ExactNumber(),
                    _ => state.State.Objects,
                },
            },
        };
    }

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceLoadedSettings(
        SiteswapGeneratorState state,
        SettingsLoadedAction action
    )
    {
        return state with { State = state.State with { Settings = action.Settings } };
    }
}
