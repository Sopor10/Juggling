﻿using Fluxor;
using Siteswaps.Generator.Generator;
using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Components.State;

using Microsoft.AspNetCore.Components;

public class GenerateSiteswapEffect : Effect<GenerateSiteswapsAction>
{
    public GenerateSiteswapEffect(NavigationManager navigationManager)
    {
        NavigationManager = navigationManager;
    }

    private NavigationManager NavigationManager { get; }

    public override async Task HandleAsync(GenerateSiteswapsAction action, IDispatcher dispatcher)
    {
        var siteswaps = await CreateSiteswaps(action);

        dispatcher.Dispatch(new SiteswapsGeneratedAction(siteswaps));
        NavigationManager.NavigateTo("/result");
    }

    public static async Task<List<Siteswap>> CreateSiteswaps(GenerateSiteswapsAction action)
    {
        var siteswaps = new List<Siteswap>();

        foreach (var (siteswapGeneratorInput, factory) in CreateSiteswapGeneratorInputs(action))
            siteswaps.AddRange(await factory
                .Create(siteswapGeneratorInput)
                .GenerateAsync()
                .ToListAsync());
        return siteswaps;
    }

    private static List<(SiteswapGeneratorInput siteswapGeneratorInput, SiteswapGeneratorFactory factory)>
        CreateSiteswapGeneratorInputs(GenerateSiteswapsAction action)
    {
        if (action.State.Period is null ||
            action.State.MinThrow is null ||
            action.State.MaxThrow is null ||
            action.State.NumberOfJugglers is null)
            return new List<(SiteswapGeneratorInput siteswapGeneratorInput, SiteswapGeneratorFactory factory)>();

        var range = action.State.Objects switch
        {
            Between between => Enumerable.Range(between.MinNumber.Value,
                between.MaxNumber.Value - between.MinNumber.Value + 1),
            ExactNumber exactNumber => new[] { exactNumber.Number.Value },
            _ => throw new ArgumentOutOfRangeException()
        };

        var result = new List<(SiteswapGeneratorInput siteswapGeneratorInput, SiteswapGeneratorFactory factory)>();
        foreach (var number in range)
        {
            var siteswapGeneratorInput = new SiteswapGeneratorInput
            {
                Period = action.State.Period.Value,
                MaxHeight = action.State.CreateFilterFromThrowList
                    ? action.State.Throws.MaxBy(x => x.Height).GetHeightForJugglers(action.State.NumberOfJugglers.Value)
                        .Max()
                    : action.State.MaxThrow.Value,
                MinHeight = action.State.CreateFilterFromThrowList
                    ? action.State.Throws.MinBy(x => x.Height).GetHeightForJugglers(action.State.NumberOfJugglers.Value)
                        .Min()
                    : action.State.MinThrow.Value,
                NumberOfObjects = number,
            };

            Func<IFilterBuilder, IFilterBuilder> filterConfig = builder => action.State.Filter
                .Aggregate(builder,
                    (current, filterInformation) => ToFilter(current, filterInformation,
                        action.State.NumberOfJugglers.Value));

            var siteswapGeneratorFactory = new SiteswapGeneratorFactory()
                .ConfigureFilter(filterConfig);

            var liste = new List<Func<IFilterBuilder, IFilterBuilder>>();
            if (action.State.CreateFilterFromThrowList)
                for (var i = siteswapGeneratorInput.MinHeight; i <= siteswapGeneratorInput.MaxHeight; i++)
                {
                    if (action.State.Throws.SelectMany(x => x.GetHeightForJugglers(action.State.NumberOfJugglers.Value))
                        .Contains(i)) continue;
                    var n = i;
                    liste.Add(x => x.ExactOccurence(n, 0));
                }

            foreach (var func in liste) siteswapGeneratorFactory = siteswapGeneratorFactory.ConfigureFilter(func);

            result.Add((siteswapGeneratorInput, siteswapGeneratorFactory));
        }

        return result;
    }

    private static IFilterBuilder ToFilter(IFilterBuilder builder, IFilterInformation filterInformation, int numberOfJugglers)
    {
        switch (filterInformation.FilterType)
        {
            case FilterType.Number:
                if (filterInformation is NumberFilterInformation numberFilterInformation)
                {
                    if (numberFilterInformation.Amount is null || numberFilterInformation.Height is null)
                        return builder.No();

                    return numberFilterInformation.Type switch
                    {
                        NumberFilterType.Exactly => builder.ExactOccurence(numberFilterInformation.Height.Value,
                            numberFilterInformation.Amount.Value),
                        NumberFilterType.AtLeast => builder.MinimumOccurence(numberFilterInformation.Height.Value,
                            numberFilterInformation.Amount.Value),
                        NumberFilterType.Maximum => builder.MaximumOccurence(numberFilterInformation.Height.Value,
                            numberFilterInformation.Amount.Value),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }

                break;
            case FilterType.Pattern:
                if (filterInformation is PatternFilterInformation patternFilterInformation)
                    return builder.Pattern(patternFilterInformation.Pattern, numberOfJugglers);
                break;
            case FilterType.NewPattern:
                if (filterInformation is NewPatternFilterInformation newPatternFilterInformation)
                    return BuildPatternFilter(newPatternFilterInformation, numberOfJugglers, builder);
                break;
        }

        throw new ArgumentOutOfRangeException();
    }

    private static IFilterBuilder BuildPatternFilter(NewPatternFilterInformation newPatternFilterInformation,
        int numberOfJugglers, IFilterBuilder builder)
    {
        var patterns = new List<List<int>>();
        foreach (var t in newPatternFilterInformation.Pattern)
        {
            var heights = t.Height switch
            {
                -1 => new List<int> { -1 },
                -2 => new List<int> { -2 },
                -3 => new List<int> { -3 },

                _ => t.GetHeightForJugglers(numberOfJugglers).ToList()
            };
            patterns.Add(heights);
        }

        return builder.FlexiblePattern(patterns, numberOfJugglers, newPatternFilterInformation.IsGlobalPattern);
    }
}