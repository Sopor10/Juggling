using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>
/// Which "screen" of the wizard page is currently shown. Kept as a simple enum on a
/// plain C# object instead of Fluxor state, per the vertical-slice constraints for this page.
/// </summary>
public enum WizardPhase
{
    Editing,
    Generating,
    Results,
}

/// <summary>
/// Connector between two neighbouring filters in the flat filter list, mirroring the
/// validated UX from design-mockups/shared/pz-demo.js (`pzInitFilterBuilder`): consecutive
/// "And" connectors form a group, groups are joined by "Or". Every combination of connectors
/// is automatically a valid "Or of And-groups" (DNF) - there is nothing to validate.
/// </summary>
public enum WizardFilterConnector
{
    And,
    Or,
}

/// <summary>
/// One entry of the flat filter list. Wraps one of the existing, Fluxor-free
/// <see cref="IFilterInformation"/> implementations from the main app (read-only reference).
/// </summary>
public sealed record WizardFilterEntry(int Id, IFilterInformation Filter);

/// <summary>
/// The entire state of the wizard page (all steps, the filter list, the generation results).
/// A plain mutable class - no Fluxor, no Radzen - held as a single instance in the
/// WizardPage code-behind and passed down to child components via parameters/EventCallbacks.
/// </summary>
public sealed class WizardState
{
    public const int TotalSteps = 3;

    public const int MinJugglers = 2;
    public const int MaxJugglers = 8;

    /// <summary>Upper bound for the exact-value juggler number input (beyond the quick-pick pills).</summary>
    public const int MaxJugglersExact = 20;

    public const int MinPeriod = 1;
    public const int MaxPeriod = 30;

    public const int MinClubs = 2;
    public const int MaxClubs = 30;

    /// <summary>Highest throw height offered anywhere in the wizard (chip grid, pattern palette, state grid).</summary>
    public const int MaxThrowHeight = 12;

    public int CurrentStep { get; set; }

    public HashSet<int> VisitedSteps { get; } = new() { 0 };

    public WizardPhase Phase { get; set; } = WizardPhase.Editing;

    public int NumberOfJugglers { get; set; } = 3;

    public Period Period { get; set; } = new(5);

    // Deliberately not equal: with a dual-range slider, identical min/max thumbs
    // start out stacked exactly on top of each other, making the lower one hard
    // to grab. Different defaults keep both handles visible/reachable from the start.
    public Between Clubs { get; set; } = new() { MinNumber = 5, MaxNumber = 7 };

    public bool ShowThrowNames { get; set; } = true;

    public List<Throw> AllowedThrows { get; } = DefaultThrows();

    public List<WizardFilterEntry> Filters { get; } = new();

    public List<WizardFilterConnector> Connectors { get; } = new();

    public List<Siteswap> Results { get; } = new();

    public bool WasCancelled { get; set; }

    public CancellationTokenSource? GenerationCancellation { get; set; }

    private int _nextFilterId = 1;

    public void MarkVisited(int step) => VisitedSteps.Add(step);

    public int NextFilterId() => _nextFilterId++;

    public void ResetToDefaults()
    {
        CurrentStep = 0;
        VisitedSteps.Clear();
        VisitedSteps.Add(0);
        Phase = WizardPhase.Editing;
        NumberOfJugglers = 3;
        Period = new Period(5);
        Clubs = new Between { MinNumber = 5, MaxNumber = 7 };
        ShowThrowNames = true;
        AllowedThrows.Clear();
        AllowedThrows.AddRange(DefaultThrows());
        Filters.Clear();
        Connectors.Clear();
        Results.Clear();
        WasCancelled = false;
    }

    private static List<Throw> DefaultThrows() =>
        new()
        {
            Throw.EmptyHand,
            Throw.Zip,
            Throw.Zap,
            Throw.Self,
            Throw.SinglePass,
            Throw.Heff,
            Throw.DoublePass,
            Throw.TripleSelf,
        };
}
