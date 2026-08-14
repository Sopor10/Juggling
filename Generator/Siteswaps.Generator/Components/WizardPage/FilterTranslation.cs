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
/// from Components/Internal/Generate/GenerateSiteswapEffect.cs. Reuses the wizard's nested
/// <see cref="FilterTree"/> and the FilterBuilder/ISiteswapFilter machinery from
/// Siteswaps.Generator.Core to build one SiteswapGenerator per club count.
/// </summary>
internal static class FilterTranslation
{
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

            var treeFilter = state.FilterTree.Root?.Visit(new FilterBuilderVisitor(input, state));
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
            ToFilter(
                WizardFilterTree.Unwrap(node.Filter),
                state.NumberOfJugglers,
                state.ShowThrowNames is false
            );

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
