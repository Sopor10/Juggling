using System.Collections.Immutable;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain;

public record SiteswapGeneratorFactory(IFilterBuilderFactory FilterBuilderFactory) : ISiteswapGeneratorFactory
{
    private ImmutableList<Func<IFilterBuilder, IFilterBuilder>> Config { get; init; } = ImmutableList<Func<IFilterBuilder, IFilterBuilder>>.Empty;
    private SiteswapGeneratorInput Input { get; init; } = new();
    public ISiteswapGeneratorFactory ConfigureFilter(Func<IFilterBuilder, IFilterBuilder>? builder) => this with { Config = builder is null ? Config : Config.Add(builder) };
    public ISiteswapGeneratorFactory WithInput(SiteswapGeneratorInput input) => this with {Input = input};
    
    
    public ISiteswapGenerator Create()
    {
        var builder = FilterBuilderFactory.Create(Input);
        foreach (var func in Config)
        {
            builder = func(builder);
        }

        return new SiteswapGenerator(builder.WithDefault().Build(), Input);
    }
}