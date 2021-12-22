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
        var range = action.State.Objects switch
        {
            Between between => Enumerable.Range(between.MinNumber, between.MaxNumber - between.MinNumber),
            ExactNumber exactNumber => new[] { exactNumber.Number },
            _ => throw new ArgumentOutOfRangeException()
        };
        var siteswaps = new List<ISiteswap>();

        foreach (var number in range)
        {
            var siteswapGeneratorInput = new SiteswapGeneratorInput
            {
                Period = action.State.Period,
                MaxHeight = action.State.MaxThrow,
                MinHeight = action.State.MinThrow,
                NumberOfObjects = number,
            };

            FilterBuilder = FilterBuilder.WithInput(siteswapGeneratorInput);
            foreach (var filterInformation in action.State.Filter)
            {
                FilterBuilder = ToFilter(filterInformation);
            }

            siteswaps.AddRange(await SiteswapGeneratorFactory.Create(FilterBuilder.Build()).GenerateAsync(siteswapGeneratorInput));
        }
        dispatcher.Dispatch(new SiteswapsGeneratetAction(siteswaps));
    }


    private IFilterBuilder ToFilter(IFilterInformation filterInformation)
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
        }
        throw new ArgumentOutOfRangeException();
    }
}