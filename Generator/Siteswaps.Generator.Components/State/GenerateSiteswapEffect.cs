using Fluxor;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Components.State;

public class GenerateSiteswapEffect : Effect<GenerateSiteswapsAction>
{
    public GenerateSiteswapEffect(ISiteswapGeneratorFactory siteswapGeneratorFactory, IFilterBuilder filterBuilder)
    {
        SiteswapGeneratorFactory = siteswapGeneratorFactory;
        FilterBuilder = filterBuilder;
    }

    private ISiteswapGeneratorFactory SiteswapGeneratorFactory { get; }
    private IFilterBuilder FilterBuilder { get; set; }

    public override async Task HandleAsync(GenerateSiteswapsAction action, IDispatcher dispatcher)
    {
        if (action.State.Period is null ||
            action.State.MinThrow is null ||
            action.State.MaxThrow is null ||
            action.State.NumberOfJugglers is null)
        {
            return;
        }
        
        var range = action.State.Objects switch
        {
            Between between => Enumerable.Range(between.MinNumber.Value, between.MaxNumber.Value - between.MinNumber.Value),
            ExactNumber exactNumber => new [] { exactNumber.Number.Value },
            _ => throw new ArgumentOutOfRangeException()
        };
        var siteswaps = new List<ISiteswap>();

        foreach (var number in range)
        {
            var siteswapGeneratorInput = new SiteswapGeneratorInput
            {
                Period = action.State.Period.Value,
                MaxHeight = action.State.MaxThrow.Value,
                MinHeight = action.State.MinThrow.Value,
                NumberOfObjects = number,
            };

            FilterBuilder = FilterBuilder.WithInput(siteswapGeneratorInput);
            foreach (var filterInformation in action.State.Filter)
            {
                FilterBuilder = ToFilter(filterInformation, action.State.NumberOfJugglers.Value);
            }

            siteswaps.AddRange(await SiteswapGeneratorFactory.Create(FilterBuilder.Build()).GenerateAsync(siteswapGeneratorInput));
        }
        dispatcher.Dispatch(new SiteswapsGeneratetAction(siteswaps));
    }


    private IFilterBuilder ToFilter(IFilterInformation filterInformation, int numberOfJugglers)
    {
        switch (filterInformation.FilterType)
        {
            case FilterType.Number:
                if (filterInformation is NumberFilterInformation numberFilterInformation)
                {
                    if (numberFilterInformation.Amount is null || numberFilterInformation.Height is null)
                        return FilterBuilder.AddNoFilter();

                    return numberFilterInformation.Type switch
                    {
                        NumberFilterType.Exactly => FilterBuilder.AddExactOccurenceFilter(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        NumberFilterType.AtLeast => FilterBuilder.AddMinimumOccurenceFilter(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        NumberFilterType.Maximum => FilterBuilder.AddMaximumOccurenceFilter(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                break;
            case FilterType.Pattern:
                if (filterInformation is PatternFilterInformation patternFilterInformation)
                {
                    return FilterBuilder.AddPatternFilter(patternFilterInformation.Pattern, numberOfJugglers);
                }
                break;
        }
        throw new ArgumentOutOfRangeException();
    }
}