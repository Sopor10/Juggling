using Fluxor;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Generator;
using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Components.State;

public class GenerateSiteswapEffect(INavigation navigation) : Effect<GenerateSiteswapsAction>
{
    public override async Task HandleAsync(GenerateSiteswapsAction action, IDispatcher dispatcher)
    {
        navigation.NavigateTo("/result");
        await Task.Delay(1);

        await CreateSiteswaps(action, dispatcher);
        Console.WriteLine("Finished");
    }

    private async Task CreateSiteswaps(GenerateSiteswapsAction action, IDispatcher dispatcher)
    {
        if (action.CancellationTokenSource.IsCancellationRequested)
        {
            throw new InvalidOperationException("This is probably an old cancellation token");
        }

        var results = new List<Siteswap>();
        foreach (var (siteswapGeneratorInput, factory) in CreateSiteswapGeneratorInputs(action))
        {
            await foreach (
                var s in factory
                    .Create(siteswapGeneratorInput)
                    .GenerateAsync(action.CancellationTokenSource.Token)
            )
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
        }
        dispatcher.Dispatch(new SiteswapGeneratedAction(results.ToList()));
    }

    private static List<(
        SiteswapGeneratorInput siteswapGeneratorInput,
        SiteswapGeneratorFactory factory
    )> CreateSiteswapGeneratorInputs(GenerateSiteswapsAction action)
    {
        if (
            action.State.MinThrow is null
            || action.State.MaxThrow is null
            || action.State.NumberOfJugglers is null
        )
        {
            return new List<(
                SiteswapGeneratorInput siteswapGeneratorInput,
                SiteswapGeneratorFactory factory
            )>();
        }

        var range = action.State.Objects switch
        {
            Between between => Enumerable.Range(
                between.MinNumber ?? throw new InvalidOperationException(),
                between.MaxNumber - between.MinNumber.Value + 1
                    ?? throw new InvalidOperationException()
            ),
            ExactNumber exactNumber => new[]
            {
                exactNumber.Number ?? throw new InvalidOperationException(),
            },
            _ => throw new ArgumentOutOfRangeException(),
        };

        var result =
            new List<(
                SiteswapGeneratorInput siteswapGeneratorInput,
                SiteswapGeneratorFactory factory
            )>();
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
                        .Max() ?? throw new InvalidOperationException()
                    : action.State.MaxThrow.Value,
                MinHeight = action.State.CreateFilterFromThrowList
                    ? action
                        .State.Throws.MinBy(x => x.Height)
                        ?.GetHeightForJugglers(
                            action.State.NumberOfJugglers.Value,
                            action.State.Settings.ShowThrowNames is false
                        )
                        .Min() ?? throw new InvalidOperationException()
                    : action.State.MinThrow.Value,
                NumberOfObjects = number,
            };

            Func<IFilterBuilder, IFilterBuilder> filterConfig = builder =>
                action.State.Filter.Aggregate(
                    builder,
                    (current, filterInformation) =>
                        ToFilter(
                            current,
                            filterInformation,
                            action.State.NumberOfJugglers.Value,
                            action.State.Settings.ShowThrowNames is false
                        )
                );

            var siteswapGeneratorFactory = new SiteswapGeneratorFactory().ConfigureFilter(
                filterConfig
            );

            var liste = new List<Func<IFilterBuilder, IFilterBuilder>>();
            if (action.State.CreateFilterFromThrowList)
            {
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
                    {
                        continue;
                    }

                    var n = i;
                    liste.Add(x => x.ExactOccurence(n, 0));
                }
            }

            foreach (var func in liste)
            {
                siteswapGeneratorFactory = siteswapGeneratorFactory.ConfigureFilter(func);
            }

            result.Add((siteswapGeneratorInput, siteswapGeneratorFactory));
        }

        return result;
    }

    private static IFilterBuilder ToFilter(
        IFilterBuilder builder,
        IFilterInformation filterInformation,
        int numberOfJugglers,
        bool showName
    )
    {
        switch (filterInformation)
        {
            case NewPatternFilterInformation newPatternFilterInformation:
                return BuildPatternFilter(
                    newPatternFilterInformation,
                    numberOfJugglers,
                    builder,
                    showName
                );
            case EasyNumberFilter.NumberFilter numberFilter:
                return numberFilter.Type switch
                {
                    EasyNumberFilter.NumberFilterType.Exactly => builder.ExactOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, showName),
                        numberFilter.Amount
                    ),
                    EasyNumberFilter.NumberFilterType.AtLeast => builder.MinimumOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, showName),
                        numberFilter.Amount
                    ),
                    EasyNumberFilter.NumberFilterType.Maximum => builder.MaximumOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, showName),
                        numberFilter.Amount
                    ),
                    _ => throw new ArgumentOutOfRangeException(),
                };
        }

        throw new ArgumentOutOfRangeException();
    }

    private static IFilterBuilder BuildPatternFilter(
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

        return builder.FlexiblePattern(
            patterns,
            numberOfJugglers,
            newPatternFilterInformation.IsGlobalPattern
        );
    }
}
