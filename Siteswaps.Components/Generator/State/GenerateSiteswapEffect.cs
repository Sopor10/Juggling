using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Siteswaps.Generator;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Components.Generator.State;

public class GenerateSiteswapEffect : Effect<GenerateSiteswapsAction>
{
    public GenerateSiteswapEffect(ISiteswapGenerator siteswapGenerator, IFilterFactory filterFactory)
    {
        SiteswapGenerator = siteswapGenerator;
        FilterFactory = filterFactory;
    }

    public ISiteswapGenerator SiteswapGenerator { get; }
    public IFilterFactory FilterFactory { get; }

    public override async Task HandleAsync(GenerateSiteswapsAction action, IDispatcher dispatcher)
    {
        var range = action.State.Objects switch
        {
            Between between => Enumerable.Range(between.MinNumber, between.MaxNumber - between.MinNumber),
            ExactNumber exactNumber => new[] { exactNumber.Number },
            _ => throw new ArgumentOutOfRangeException()
        };
        var siteswaps = new List<Siteswap>();

        foreach (var number in range)
            siteswaps.AddRange(await SiteswapGenerator.GenerateAsync(new SiteswapGeneratorInput
            {
                Period = action.State.Period,
                MaxHeight = action.State.MaxThrow,
                MinHeight = action.State.MinThrow,
                NumberOfObjects = number,
                Filter = ToFilter(action.State.Filter)
            }));

        dispatcher.Dispatch(new SiteswapsGeneratetAction(siteswaps));
    }

    private ISiteswapFilter ToFilter(ImmutableList<IFilterInformation> stateFilter) => FilterFactory.Combine(stateFilter.Select(ToFilter));

    private ISiteswapFilter ToFilter(IFilterInformation filterInformation)
    {
        var filter = FilterFactory.NoFilter();
        switch (filterInformation.FilterType)
        {
            case FilterType.Number:
                if (filterInformation is NumberFilterInformation numberFilterInformation)
                {
                    if (numberFilterInformation.Amount is null || numberFilterInformation.Height is null)
                        return FilterFactory.NoFilter();

                    filter = numberFilterInformation.Type switch
                    {
                        NumberFilterType.Exactly => FilterFactory.ExactOccurenceFilter(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        NumberFilterType.AtLeast => FilterFactory.MinimumOccurenceFilter(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        NumberFilterType.Maximum => FilterFactory.MaximumOccurenceFilter(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return filter;
    }
}