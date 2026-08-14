using System.Collections.Immutable;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.CardStackPage.Models;

/// <summary>
/// The entire mutable state of the Card-Stack page. This is a plain, Fluxor-free
/// class - the page owns one instance of it and mutates it directly in response
/// to two-way bindings from its child components.
/// </summary>
public sealed class CardStackFormState
{
    public int Jugglers { get; set; } = 3;
    public int Period { get; set; } = 5;

    // Deliberately not equal: identical dual-range-slider thumbs start out stacked, making the lower one hard to grab.
    public int ClubsMin { get; set; } = 5;
    public int ClubsMax { get; set; } = 7;
    public bool ShowThrowNames { get; set; } = true;

    /// <summary>Allowed throw heights, mirrors the "Erlaubte Würfe" chip grid.</summary>
    public List<int> AllowedThrowHeights { get; set; } = [3, 4, 5, 6, 7, 8];

    /// <summary>Flat list of filters, in visual order.</summary>
    public List<CardStackFilterItem> Filters { get; set; } = [];

    /// <summary>
    /// One connector per gap between adjacent filters. Length is always
    /// Filters.Count - 1 (or 0 for 0/1 filters). Mirrors the pz-demo.js
    /// "computeGroups" data model 1:1.
    /// </summary>
    public List<CardStackFilterConnector> Connectors { get; set; } = [];

    public const int MinClubs = 2;
    public const int MaxClubs = 30;
    public const int MinPeriod = 1;
    public const int MaxPeriod = 30;
    public const int MinJugglers = 2;
    public const int MaxJugglers = 8;

    /// <summary>Upper bound for the exact-value juggler number input (beyond the quick-pick pills).</summary>
    public const int MaxJugglersExact = 20;

    public int MaxAllowedThrowHeight =>
        AllowedThrowHeights.Count > 0 ? AllowedThrowHeights.Max() : 8;

    public int MinAllowedThrowHeight =>
        AllowedThrowHeights.Count > 0 ? AllowedThrowHeights.Min() : 2;

    /// <summary>
    /// Groups the flat filter list into "OR of AND-groups" (DNF), exactly like
    /// computeGroups() in pz-demo.js: consecutive 'and' connectors extend the
    /// current group, an 'or' connector starts a new group.
    /// </summary>
    public List<List<CardStackFilterItem>> ComputeGroups()
    {
        if (Filters.Count == 0)
        {
            return [];
        }

        var groups = new List<List<CardStackFilterItem>> { new() { Filters[0] } };
        for (var i = 1; i < Filters.Count; i++)
        {
            if (Connectors[i - 1] == CardStackFilterConnector.And)
            {
                groups[^1].Add(Filters[i]);
            }
            else
            {
                groups.Add(new List<CardStackFilterItem> { Filters[i] });
            }
        }

        return groups;
    }
}

public enum CardStackFilterConnector
{
    And,
    Or,
}

public enum CardStackFilterKind
{
    Number,
    Pattern,
    State,
}

public enum CardStackNumberComparison
{
    Exactly,
    Maximum,
    AtLeast,
}

/// <summary>
/// One filter card in the flat list. Carries data for all three kinds so that
/// switching tabs in the editor sheet doesn't lose previously entered values.
/// </summary>
public sealed class CardStackFilterItem
{
    public required int Id { get; init; }
    public CardStackFilterKind Kind { get; set; } = CardStackFilterKind.Number;

    public CardStackNumberComparison NumberComparison { get; set; } =
        CardStackNumberComparison.Exactly;
    public int NumberAmount { get; set; } = 2;
    public int NumberThrowHeight { get; set; } = 8;

    public PatternRotation PatternRotation { get; set; } = PatternRotation.Global;
    public bool PatternIsInclude { get; set; } = true;
    public List<int> PatternSequenceHeights { get; set; } = [];

    /// <summary>true = an object is scheduled to land on this future beat.</summary>
    public bool[] StateActiveBeats { get; set; } = new bool[9];

    public CardStackFilterItem Clone() =>
        new()
        {
            Id = Id,
            Kind = Kind,
            NumberComparison = NumberComparison,
            NumberAmount = NumberAmount,
            NumberThrowHeight = NumberThrowHeight,
            PatternRotation = PatternRotation,
            PatternIsInclude = PatternIsInclude,
            PatternSequenceHeights = [.. PatternSequenceHeights],
            StateActiveBeats = [.. StateActiveBeats],
        };
}
