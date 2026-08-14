using System.Collections.Immutable;
using Siteswaps.Generator.Components.CardStackPage.Models;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Components.CardStackPage;

/// <summary>
/// Fluxor-free adaptation of the filter-translation logic in
/// Components/Internal/Generate/GenerateSiteswapEffect.cs, scoped to the
/// Card-Stack page's own flat AND/OR filter list instead of the free-form
/// FilterTree editor. The original file is untouched; this is an independent
/// copy tailored to this page's needs.
/// </summary>
public static class FilterTranslation
{
    /// <summary>
    /// Builds a <see cref="FilterTree"/> ("OR of AND-groups" / DNF) from the
    /// page's flat filter list + connectors, exactly mirroring computeGroups()
    /// in pz-demo.js: AND-runs become an AndNode, groups are joined by an
    /// OrNode, single filters stay a bare FilterLeaf.
    /// </summary>
    public static FilterTree BuildFilterTree(CardStackFormState state)
    {
        var groups = state.ComputeGroups();
        if (groups.Count == 0)
        {
            return new FilterTree(new AndNode());
        }

        var groupNodes = groups.Select(BuildGroupNode).ToImmutableList();
        FilterNode root = groupNodes.Count == 1 ? groupNodes[0] : new OrNode(groupNodes);
        return new FilterTree(root);
    }

    private static FilterNode BuildGroupNode(List<CardStackFilterItem> group)
    {
        var leaves = group
            .Select(f => (FilterNode)new FilterLeaf(ToFilterInformation(f)))
            .ToImmutableList();
        return leaves.Count == 1 ? leaves[0] : new AndNode(leaves);
    }

    private static IFilterInformation ToFilterInformation(CardStackFilterItem item) =>
        item.Kind switch
        {
            CardStackFilterKind.Number => new EasyNumberFilter.NumberFilter
            {
                Amount = item.NumberAmount,
                Type = item.NumberComparison switch
                {
                    CardStackNumberComparison.Exactly => EasyNumberFilter.NumberFilterType.Exactly,
                    CardStackNumberComparison.Maximum => EasyNumberFilter.NumberFilterType.Maximum,
                    CardStackNumberComparison.AtLeast => EasyNumberFilter.NumberFilterType.AtLeast,
                    _ => throw new ArgumentOutOfRangeException(nameof(item)),
                },
                Throw = ThrowForHeight(item.NumberThrowHeight),
            },
            CardStackFilterKind.Pattern => new NewPatternFilterInformation(
                item.PatternSequenceHeights.Select(ThrowForHeight).ToList(),
                item.PatternRotation,
                item.PatternIsInclude,
                false
            ),
            CardStackFilterKind.State => new EasyStateFilter.StateFilter(
                item.StateActiveBeats.ToImmutableArray()
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(item)),
        };

    /// <summary>Resolves a throw height to its named Throw (falls back to a plain-number throw).</summary>
    public static Throw ThrowForHeight(int height) =>
        Throw.All(Math.Max(height, 13)).FirstOrDefault(t => t.Height == height)
        ?? new Throw(height.ToString(), height, height.ToString());

    /// <summary>
    /// Builds one SiteswapGenerator per club-count in [ClubsMin, ClubsMax],
    /// adapted from GenerateSiteswapEffect.CreateSiteswapGeneratorInputs but
    /// without any Fluxor dependency.
    /// </summary>
    public static List<SiteswapGenerator> CreateSiteswapGenerators(CardStackFormState state)
    {
        var filterTree = BuildFilterTree(state);
        var useLiteralValue = state.ShowThrowNames is false;

        // MinHeight/MaxHeight must use the expanded per-juggler-count space (Throw.GetHeightForJugglers), not the raw named-throw-height space the UI shows, or the generator searches the wrong range (e.g. "2.67").
        var allowedHeights = state
            .AllowedThrowHeights.SelectMany(h =>
                ThrowForHeight(h).GetHeightForJugglers(state.Jugglers, useLiteralValue)
            )
            .ToHashSet();
        var minHeight = allowedHeights.Count > 0 ? allowedHeights.Min() : 2;
        var maxHeight = allowedHeights.Count > 0 ? allowedHeights.Max() : 10;

        var result = new List<SiteswapGenerator>();
        for (var clubs = state.ClubsMin; clubs <= state.ClubsMax; clubs++)
        {
            var input = new SiteswapGeneratorInput
            {
                Period = state.Period,
                NumberOfObjects = clubs,
                MinHeight = minHeight,
                MaxHeight = maxHeight,
            };

            var visitor = new FilterBuilderVisitor(input, state.Jugglers, useLiteralValue);
            var filters = new List<ISiteswapFilter>();

            if (filterTree.Root is not null)
            {
                filters.Add(filterTree.Root.Visit(visitor));
            }

            // Throws within [min,max] but not in the allowed set must be forbidden explicitly, mirroring the reference logic.
            for (var i = minHeight; i <= maxHeight; i++)
            {
                if (allowedHeights.Contains(i))
                {
                    continue;
                }

                filters.Add(new ExactlyXXXTimesFilter([i], 0));
            }

            result.Add(new SiteswapGenerator(new AndFilter(filters), input));
        }

        return result;
    }
}

internal sealed class FilterBuilderVisitor(
    SiteswapGeneratorInput input,
    int numberOfJugglers,
    bool useLiteralValue
) : IFilterVisitor<ISiteswapFilter>
{
    public ISiteswapFilter Visit(AndNode node) =>
        new AndFilter(node.Children.Select(c => c.Visit(this)));

    public ISiteswapFilter Visit(OrNode node) =>
        new OrFilter(node.Children.Select(c => c.Visit(this)));

    public ISiteswapFilter Visit(FilterLeaf node) => ToFilter(node.Filter);

    private ISiteswapFilter ToFilter(IFilterInformation filterInformation)
    {
        var builder = new FilterBuilder(input);
        return filterInformation switch
        {
            NewPatternFilterInformation patternFilterInformation => BuildPatternFilter(
                patternFilterInformation,
                builder
            ),
            EasyNumberFilter.NumberFilter numberFilter => numberFilter.Type switch
            {
                EasyNumberFilter.NumberFilterType.Exactly => builder
                    .ExactOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, useLiteralValue),
                        numberFilter.Amount
                    )
                    .Build(),
                EasyNumberFilter.NumberFilterType.AtLeast => builder
                    .MinimumOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, useLiteralValue),
                        numberFilter.Amount
                    )
                    .Build(),
                EasyNumberFilter.NumberFilterType.Maximum => builder
                    .MaximumOccurence(
                        numberFilter.Throw.GetHeightForJugglers(numberOfJugglers, useLiteralValue),
                        numberFilter.Amount
                    )
                    .Build(),
                _ => throw new ArgumentOutOfRangeException(nameof(filterInformation)),
            },
            EasyStateFilter.StateFilter stateFilter => builder
                .WithState(new Siteswaps.Generator.Core.Generator.Filter.State(stateFilter.Items))
                .Build(),
            _ => throw new ArgumentOutOfRangeException(nameof(filterInformation)),
        };
    }

    private ISiteswapFilter BuildPatternFilter(
        NewPatternFilterInformation patternFilterInformation,
        IFilterBuilder builder
    )
    {
        var patterns = patternFilterInformation
            .Pattern.Select(t => t.GetHeightForJugglers(numberOfJugglers, useLiteralValue).ToList())
            .ToList();

        ISiteswapFilter filter;
        if (patternFilterInformation.PatternRotation.Value < 0)
        {
            filter = builder
                .FlexiblePattern(
                    patterns,
                    numberOfJugglers,
                    patternFilterInformation.PatternRotation == PatternRotation.Global
                )
                .Build();
        }
        else
        {
            filter = new RotationAwareFlexiblePatternFilter(
                patterns,
                numberOfJugglers,
                input,
                patternFilterInformation.PatternRotation.Value
            );
        }

        return patternFilterInformation.IsIncludePattern ? filter : new NotFilter(filter);
    }
}
