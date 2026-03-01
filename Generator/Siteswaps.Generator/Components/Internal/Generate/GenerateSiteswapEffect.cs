using Fluxor;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Components.Internal.Generate;

public class GenerateSiteswapEffect(INavigation navigation)
    : Effect<GenerateButton.GenerateSiteswapsAction>
{
    public override async Task HandleAsync(
        GenerateButton.GenerateSiteswapsAction action,
        IDispatcher dispatcher
    )
    {
        navigation.NavigateTo("/result");
        await Task.Delay(1);

        await CreateSiteswaps(action, dispatcher);

        dispatcher.Dispatch(new FinishedGeneratingSiteswaps());
        await Task.Delay(1);
        Console.WriteLine("Finished");
    }

    private async Task CreateSiteswaps(
        GenerateButton.GenerateSiteswapsAction action,
        IDispatcher dispatcher
    )
    {
        if (action.CancellationTokenSource.IsCancellationRequested)
            throw new InvalidOperationException("This is probably an old cancellation token");

        var results = new List<Siteswap>();
        foreach (var generator in CreateSiteswapGeneratorInputs(action))
        await foreach (var s in generator.GenerateAsync(action.CancellationTokenSource.Token))
        {
            if (action.CancellationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Cancelled siteswap generation");
                return;
            }

            if (results.Count < 10)
            {
                results.Add(s);
            }
            else
            {
                dispatcher.Dispatch(new SiteswapGeneratedAction(results.ToList()));
                results.Clear();
            }

            await Task.Delay(1);
        }

        dispatcher.Dispatch(new SiteswapGeneratedAction(results.ToList()));
    }

    private static List<SiteswapGenerator> CreateSiteswapGeneratorInputs(
        GenerateButton.GenerateSiteswapsAction action
    )
    {
        if (
            action.State.MinThrow is null
            || action.State.MaxThrow is null
            || action.State.NumberOfJugglers is null
        )
            return new();

        var range = Enumerable.Range(
            action.State.Clubs.MinNumber,
            action.State.Clubs.MaxNumber - action.State.Clubs.MinNumber + 1
        );

        var result = new List<SiteswapGenerator>();
        foreach (var number in range)
        {
            var siteswapGeneratorInput = new SiteswapGeneratorInput
            {
                Period = action.State.Period.Value,
                MaxHeight = action.State.CreateFilterFromThrowList
                    ? action
                        .State.Throws.MaxBy(x => x.Height)
                        ?.GetHeightForJugglers(
                            action.State.NumberOfJugglers.Value,
                            action.State.Settings.ShowThrowNames is false
                        )
                        .Max()
                    ?? throw new InvalidOperationException()
                    : action.State.MaxThrow.Value,
                MinHeight = action.State.CreateFilterFromThrowList
                    ? action
                        .State.Throws.MinBy(x => x.Height)
                        ?.GetHeightForJugglers(
                            action.State.NumberOfJugglers.Value,
                            action.State.Settings.ShowThrowNames is false
                        )
                        .Min()
                    ?? throw new InvalidOperationException()
                    : action.State.MinThrow.Value,
                NumberOfObjects = number,
            };

            var liste = new List<ISiteswapFilter>();

            var filter = action.State.FilterTree.Root?.Visit(
                new FilterBuilderVisitor(siteswapGeneratorInput, action)
            );

            if (filter is not null)
            {
                liste.Add(filter);
            }
            if (action.State.CreateFilterFromThrowList)
                for (
                    var i = siteswapGeneratorInput.MinHeight;
                    i <= siteswapGeneratorInput.MaxHeight;
                    i++
                )
                {
                    if (
                        action
                            .State.Throws.SelectMany(x =>
                                x.GetHeightForJugglers(
                                    action.State.NumberOfJugglers.Value,
                                    action.State.Settings.ShowThrowNames is false
                                )
                            )
                            .Contains(i)
                    )
                        continue;

                    liste.Add(new ExactlyXXXTimesFilter([i], 0));
                }

            result.Add(new SiteswapGenerator(new AndFilter(liste), siteswapGeneratorInput));
        }

        return result;
    }
}

internal class FilterBuilderVisitor(
    SiteswapGeneratorInput siteswapGeneratorInput,
    GenerateButton.GenerateSiteswapsAction action
) : IFilterVisitor<ISiteswapFilter>
{
    public ISiteswapFilter Visit(AndNode node)
    {
        return new AndFilter(node.Children.Select(y => y.Visit(this)));
    }

    public ISiteswapFilter Visit(OrNode node)
    {
        return new OrFilter(node.Children.Select(y => y.Visit(this)));
    }

    public ISiteswapFilter Visit(FilterLeaf node)
    {
        if (action.State.NumberOfJugglers is null)
        {
            return new NoFilter();
        }

        return ToFilter(
            node.Filter,
            action.State.NumberOfJugglers.Value,
            action.State.Settings.ShowThrowNames is false
        );
    }

    private ISiteswapFilter ToFilter(
        IFilterInformation filterInformation,
        int numberOfJugglers,
        bool showName
    )
    {
        var builder = new FilterBuilder(siteswapGeneratorInput);
        return filterInformation switch
        {
            NewPatternFilterInformation newPatternFilterInformation => BuildPatternFilter(
                newPatternFilterInformation,
                numberOfJugglers,
                builder,
                showName
            ),
            EasyNumberFilter.NumberFilter numberFilter => numberFilter.Type switch
            {
                EasyNumberFilter.NumberFilterType.Exactly => builder
                    .ExactOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, showName),
                        numberFilter.Amount
                    )
                    .Build(),
                EasyNumberFilter.NumberFilterType.AtLeast => builder
                    .MinimumOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, showName),
                        numberFilter.Amount
                    )
                    .Build(),
                EasyNumberFilter.NumberFilterType.Maximum => builder
                    .MaximumOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, showName),
                        numberFilter.Amount
                    )
                    .Build(),
                _ => throw new ArgumentOutOfRangeException(),
            },
            EasyStateFilter.StateFilter stateFilter => builder
                .WithState(new Siteswaps.Generator.Core.Generator.Filter.State(stateFilter.Items))
                .Build(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private ISiteswapFilter BuildPatternFilter(
        NewPatternFilterInformation newPatternFilterInformation,
        int numberOfJugglers,
        IFilterBuilder builder,
        bool showName
    )
    {
        var patterns = new List<List<int>>();
        foreach (var t in newPatternFilterInformation.Pattern)
        {
            var heights = t.Height switch
            {
                -1 => new List<int> { -1 },
                -2 => new List<int> { -2 },
                -3 => new List<int> { -3 },

                _ => t.GetHeightForJugglers(numberOfJugglers, showName).ToList(),
            };
            patterns.Add(heights);
        }

        ISiteswapFilter filter = new NoFilter();
        if (newPatternFilterInformation.PatternRotation.Value < 0)
        {
            filter = builder
                .FlexiblePattern(
                    patterns,
                    numberOfJugglers,
                    newPatternFilterInformation.PatternRotation == PatternRotation.Global
                )
                .Build();
        }
        else
        {
            filter = new RotationAwareFlexiblePatternFilter(
                patterns,
                numberOfJugglers,
                siteswapGeneratorInput,
                newPatternFilterInformation.PatternRotation.Value
            );
        }

        var buildPatternFilter = newPatternFilterInformation.IsIncludePattern
            ? filter
            : new NotFilter(filter);

        if (newPatternFilterInformation.IsValidLocally)
        {
            return builder
                .And(
                    buildPatternFilter,
                    new LocallyValidFilter(
                        numberOfJugglers,
                        newPatternFilterInformation.PatternRotation.Value
                    )
                )
                .Build();
        }

        return buildPatternFilter;
    }
}

public record SiteswapGeneratedAction(params IEnumerable<Siteswap> Siteswaps);

public record FinishedGeneratingSiteswaps();

public static class Reducer
{
    [ReducerMethod]
    public static SiteswapGeneratorState ReduceFinishedGeneratingSiteswapsAction(
        SiteswapGeneratorState state,
        FinishedGeneratingSiteswaps action
    ) => state with { IsFinished = true };

    [ReducerMethod]
    public static SiteswapGeneratorState ReduceSingleSiteswapsGeneratedChangedAction(
        SiteswapGeneratorState state,
        SiteswapGeneratedAction action
    )
    {
        return state with { Siteswaps = state.Siteswaps.AddRange(action.Siteswaps) };
    }
}
