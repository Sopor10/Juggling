using System.Collections.Immutable;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>
/// Fluxor-free adaptation of the filter-tree construction and generator-input building logic
/// from Components/Internal/Generate/GenerateSiteswapEffect.cs (read-only reference, not
/// modified). Builds an AndNode/OrNode/FilterLeaf tree from the wizard's flat filter list +
/// connectors (the DNF "Or of And-groups" construction from design-mockups/shared/pz-demo.js,
/// computeGroups()), then reuses the same FilterBuilder/ISiteswapFilter machinery from
/// Siteswaps.Generator.Core to build one SiteswapGenerator per club count.
/// </summary>
internal static class FilterTranslation
{
    public static List<List<WizardFilterEntry>> ComputeGroups(
        IReadOnlyList<WizardFilterEntry> filters,
        IReadOnlyList<WizardFilterConnector> connectors
    )
    {
        if (filters.Count == 0)
        {
            return new List<List<WizardFilterEntry>>();
        }

        var groups = new List<List<WizardFilterEntry>> { new() { filters[0] } };
        for (var i = 1; i < filters.Count; i++)
        {
            if (i - 1 < connectors.Count && connectors[i - 1] == WizardFilterConnector.And)
            {
                groups[^1].Add(filters[i]);
            }
            else
            {
                groups.Add(new List<WizardFilterEntry> { filters[i] });
            }
        }

        return groups;
    }

    public static FilterTree BuildFilterTree(
        IReadOnlyList<WizardFilterEntry> filters,
        IReadOnlyList<WizardFilterConnector> connectors
    )
    {
        var groups = ComputeGroups(filters, connectors);
        if (groups.Count == 0)
        {
            return new FilterTree(new AndNode());
        }

        var groupNodes = groups
            .Select(group =>
                group.Count == 1
                    ? (FilterNode)new FilterLeaf(group[0].Filter)
                    : new AndNode(
                        group
                            .Select(entry => (FilterNode)new FilterLeaf(entry.Filter))
                            .ToImmutableList()
                    )
            )
            .ToList();

        FilterNode root =
            groupNodes.Count == 1 ? groupNodes[0] : new OrNode(groupNodes.ToImmutableList());

        return new FilterTree(root);
    }

    public static List<SiteswapGenerator> CreateGenerators(WizardState state)
    {
        var result = new List<SiteswapGenerator>();

        if (state.AllowedThrows.Count == 0)
        {
            return result;
        }

        var useLiteralValue = state.ShowThrowNames is false;
        var allowedHeights = state
            .AllowedThrows.SelectMany(t =>
                t.GetHeightForJugglers(state.NumberOfJugglers, useLiteralValue)
            )
            .ToHashSet();

        if (allowedHeights.Count == 0)
        {
            return result;
        }

        var maxHeight = allowedHeights.Max();
        var minHeight = allowedHeights.Min();

        var filterTree = BuildFilterTree(state.Filters, state.Connectors);

        for (var number = state.Clubs.MinNumber; number <= state.Clubs.MaxNumber; number++)
        {
            var input = new SiteswapGeneratorInput
            {
                Period = state.Period.Value,
                MaxHeight = maxHeight,
                MinHeight = minHeight,
                NumberOfObjects = number,
            };

            var filters = new List<ISiteswapFilter>();

            var treeFilter = filterTree.Root?.Visit(new FilterBuilderVisitor(input, state));
            if (treeFilter is not null)
            {
                filters.Add(treeFilter);
            }

            for (var height = input.MinHeight; height <= input.MaxHeight; height++)
            {
                if (allowedHeights.Contains(height))
                {
                    continue;
                }
                filters.Add(new ExactlyXXXTimesFilter([height], 0));
            }

            result.Add(new SiteswapGenerator(new AndFilter(filters), input));
        }

        return result;
    }

    private sealed class FilterBuilderVisitor(SiteswapGeneratorInput input, WizardState state)
        : IFilterVisitor<ISiteswapFilter>
    {
        public ISiteswapFilter Visit(AndNode node) =>
            new AndFilter(node.Children.Select(x => x.Visit(this)));

        public ISiteswapFilter Visit(OrNode node) =>
            new OrFilter(node.Children.Select(x => x.Visit(this)));

        public ISiteswapFilter Visit(FilterLeaf node) =>
            ToFilter(node.Filter, state.NumberOfJugglers, state.ShowThrowNames is false);

        private ISiteswapFilter ToFilter(
            IFilterInformation filterInformation,
            int numberOfJugglers,
            bool useLiteralValue
        )
        {
            var builder = new FilterBuilder(input);
            return filterInformation switch
            {
                NewPatternFilterInformation patternFilterInformation => BuildPatternFilter(
                    patternFilterInformation,
                    numberOfJugglers,
                    builder,
                    useLiteralValue
                ),
                EasyNumberFilter.NumberFilter numberFilter => numberFilter.Type switch
                {
                    EasyNumberFilter.NumberFilterType.Exactly => builder
                        .ExactOccurence(
                            numberFilter.Throw.GetHeightForJugglers(
                                numberOfJugglers,
                                useLiteralValue
                            ),
                            numberFilter.Amount
                        )
                        .Build(),
                    EasyNumberFilter.NumberFilterType.AtLeast => builder
                        .MinimumOccurence(
                            numberFilter.Throw.GetHeightForJugglers(
                                numberOfJugglers,
                                useLiteralValue
                            ),
                            numberFilter.Amount
                        )
                        .Build(),
                    EasyNumberFilter.NumberFilterType.Maximum => builder
                        .MaximumOccurence(
                            numberFilter.Throw.GetHeightForJugglers(
                                numberOfJugglers,
                                useLiteralValue
                            ),
                            numberFilter.Amount
                        )
                        .Build(),
                    _ => throw new ArgumentOutOfRangeException(),
                },
                EasyStateFilter.StateFilter stateFilter => builder
                    .WithState(
                        new Siteswaps.Generator.Core.Generator.Filter.State(stateFilter.Items)
                    )
                    .Build(),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private ISiteswapFilter BuildPatternFilter(
            NewPatternFilterInformation newPatternFilterInformation,
            int numberOfJugglers,
            IFilterBuilder builder,
            bool useLiteralValue
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
                    _ => t.GetHeightForJugglers(numberOfJugglers, useLiteralValue).ToList(),
                };
                patterns.Add(heights);
            }

            ISiteswapFilter filter;
            if (newPatternFilterInformation.PatternRotation.Value < 0)
            {
                filter = builder
                    .FlexiblePattern(
                        patterns,
                        numberOfJugglers,
                        newPatternFilterInformation.PatternRotation == PatternRotation.Global
                    )
                    .Build();
            }
            else
            {
                filter = new RotationAwareFlexiblePatternFilter(
                    patterns,
                    numberOfJugglers,
                    input,
                    newPatternFilterInformation.PatternRotation.Value
                );
            }

            var buildPatternFilter = newPatternFilterInformation.IsIncludePattern
                ? filter
                : new NotFilter(filter);

            if (newPatternFilterInformation.IsValidLocally)
            {
                return builder
                    .And(
                        buildPatternFilter,
                        new LocallyValidFilter(
                            numberOfJugglers,
                            newPatternFilterInformation.PatternRotation.Value
                        )
                    )
                    .Build();
            }

            return buildPatternFilter;
        }
    }
}
