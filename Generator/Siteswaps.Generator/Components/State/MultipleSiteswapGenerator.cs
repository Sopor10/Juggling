namespace Siteswaps.Generator.Components.State;

using Generator;
using Generator.Filter;

public class MultipleSiteswapGenerator : ISiteswapGenerator
{
    public MultipleSiteswapGenerator(GeneratorState state)
    {
        this.State = state;
    }

    private GeneratorState State { get; }
    public async IAsyncEnumerable<Siteswap> GenerateAsync()
    {
        foreach (var (siteswapGeneratorInput, factory) in CreateSiteswapGeneratorInputs(this.State))
        {
            await foreach (var siteswap in factory
                               .Create(siteswapGeneratorInput)
                               .GenerateAsync())
            {
                yield return siteswap;
            }
        }
    }

    private static List<(SiteswapGeneratorInput siteswapGeneratorInput, SiteswapGeneratorFactory factory)>
        CreateSiteswapGeneratorInputs(GeneratorState state)
    {
    
        if (state.Period is null ||
            state.MinThrow is null ||
            state.MaxThrow is null ||
            state.NumberOfJugglers is null)
            return new List<(SiteswapGeneratorInput siteswapGeneratorInput, SiteswapGeneratorFactory factory)>();

        var range = state.Objects switch
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
                Period = state.Period.Value,
                MaxHeight = state.CreateFilterFromThrowList
                    ? state.Throws.MaxBy(x => x.Height).GetHeightForJugglers(state.NumberOfJugglers.Value)
                        .Max()
                    : state.MaxThrow.Value,
                MinHeight = state.CreateFilterFromThrowList
                    ? state.Throws.MinBy(x => x.Height).GetHeightForJugglers(state.NumberOfJugglers.Value)
                        .Min()
                    : state.MinThrow.Value,
                NumberOfObjects = number,
            };

            Func<IFilterBuilder, IFilterBuilder> filterConfig = builder => state.Filter
                .Aggregate(builder,
                    (current, filterInformation) => ToFilter(current, filterInformation,
                        state.NumberOfJugglers.Value));

            var siteswapGeneratorFactory = new SiteswapGeneratorFactory()
                .ConfigureFilter(filterConfig);

            var liste = new List<Func<IFilterBuilder, IFilterBuilder>>();
            if (state.CreateFilterFromThrowList)
                for (var i = siteswapGeneratorInput.MinHeight; i <= siteswapGeneratorInput.MaxHeight; i++)
                {
                    if (state.Throws.SelectMany(x => x.GetHeightForJugglers(state.NumberOfJugglers.Value))
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
            case FilterType.Interface:
            {
                if (filterInformation is InterfaceFilterInformation interfaceFilterInformation)
                {
                    return builder.Interface(interfaceFilterInformation.Pattern.Select(x => x.Height), numberOfJugglers);
                }

                break;
            }
        }

        throw new ArgumentOutOfRangeException();
    }

    private static IFilterBuilder BuildPatternFilter(NewPatternFilterInformation newPatternFilterInformation,
        int numberOfJugglers, IFilterBuilder builder)
    {
        var pattern = Pattern.FromThrows(newPatternFilterInformation.Pattern, numberOfJugglers);

        return builder.FlexiblePattern(pattern, numberOfJugglers, newPatternFilterInformation.IsGlobalPattern);
    }
}
