using Fluxor;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Components.State;

public class GenerateSiteswapsFromIntuitiveUiEffect : Effect<SetStateFromIntuitiveUiAndGenerateSiteswaps>
{
    public override async Task HandleAsync(SetStateFromIntuitiveUiAndGenerateSiteswaps action, IDispatcher dispatcher)
    {
        var state = new GeneratorState()
        {
            Objects = new Between
            {
                MaxNumber = action.Clubs.Last(),
                MinNumber = action.Clubs.First()
            },
            Period = action.Period,
            MaxThrow = action.Throws.Select(x => x.Height).Max(),
            MinThrow = action.Throws.Select(x => x.Height).Min(),
            NumberOfJugglers = action.NumberOfJugglers
        };
        dispatcher.Dispatch(new SetState(state));
        dispatcher.Dispatch(new GenerateSiteswapsAction(state));
    }
}



public class GenerateSiteswapEffect : Effect<GenerateSiteswapsAction>
{
    public GenerateSiteswapEffect(ISiteswapGeneratorFactory siteswapGeneratorFactory)
    {
        SiteswapGeneratorFactory = siteswapGeneratorFactory;
    }

    private ISiteswapGeneratorFactory SiteswapGeneratorFactory { get; }

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

            siteswaps.AddRange(await SiteswapGeneratorFactory
                .WithInput(siteswapGeneratorInput)
                .ConfigureFilter(builder => action.State.Filter
                    .Aggregate(builder, (current, filterInformation) => ToFilter(current, filterInformation, action.State.NumberOfJugglers.Value)))
                .Create()
                .GenerateAsync()
                .ToListAsync());
        }
        
        dispatcher.Dispatch(new SiteswapsGeneratedAction(siteswaps));
    }


    private IFilterBuilder ToFilter(IFilterBuilder builder, IFilterInformation filterInformation, int numberOfJugglers)
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
                        NumberFilterType.Exactly => builder.ExactOccurence(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        NumberFilterType.AtLeast => builder.MinimumOccurence(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        NumberFilterType.Maximum => builder.MaximumOccurence(numberFilterInformation.Height.Value, numberFilterInformation.Amount.Value),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                break;
            case FilterType.Pattern:
                if (filterInformation is PatternFilterInformation patternFilterInformation)
                {
                    return builder.Pattern(patternFilterInformation.Pattern, numberOfJugglers);
                }
                break;
        }
        throw new ArgumentOutOfRangeException();
    }
}