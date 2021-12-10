using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Siteswaps.Generator;

namespace Siteswaps.Components.Generator.State;

[FeatureState]
public record SiteswapGeneratorState
{
    public IReadOnlyCollection<Siteswap> Siteswaps { get; init; }
    public bool IsGenerating => State.IsGenerating;

    public SiteswapGeneratorState()
    {
        State = new GeneratorState();
        Siteswaps = new List<Siteswap>();
    }

    public SiteswapGeneratorState(GeneratorState state, IReadOnlyCollection<Siteswap> siteswaps)
    {
        Siteswaps = siteswaps;
        State = state;
    }

    public GeneratorState State { get; init; }
}

public record GenerateSiteswapsAction(GeneratorState State);

public record PeriodChangedAction(int Value);

public record ExactNumberChangedAction(int Value);

public record MinNumberChangedAction(int Value);

public record MaxNumberChangedAction(int Value);

public record NumberOfJugglersChangedAction(int Value);

public record MinThrowChangedAction(int Value);

public record MaxThrowChangedAction(int Value);

public record ExactNumberOrRangeOfBallsSwitchedAction(bool Value);

public record SiteswapsGeneratetAction(IReadOnlyCollection<Siteswap> Siteswaps);

public class GenerateSiteswapEffect : Effect<GenerateSiteswapsAction>
{
    public GenerateSiteswapEffect(ISiteswapGenerator siteswapGenerator)
    {
        SiteswapGenerator = siteswapGenerator;
    }

    public ISiteswapGenerator SiteswapGenerator { get; }

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
        {
            siteswaps.AddRange(await SiteswapGenerator.GenerateAsync(new SiteswapGeneratorInput
            {
                Period = action.State.Period,
                MaxHeight = action.State.MaxThrow,
                MinHeight = action.State.MinThrow,
                NumberOfObjects = number
            }));
        }
        
        dispatcher.Dispatch(new SiteswapsGeneratetAction(siteswaps));
    }
}