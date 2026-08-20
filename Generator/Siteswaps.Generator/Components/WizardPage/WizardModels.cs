using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
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
/// One leaf in the nested filter tree, used when opening the editor sheet.
/// </summary>
public sealed record WizardFilterEntry(int Id, IFilterInformation Filter);

/// <summary>
/// The entire state of the wizard page (all steps, the filter list, the generation results).
/// A plain mutable class - no Fluxor - held as a single instance in the
/// WizardPage code-behind and passed down to child components via parameters/EventCallbacks.
/// </summary>
public sealed class WizardState
{
    public const int TotalSteps = 3;

    /// <summary>Progress-dot index for the results screen (editing steps are 0..TotalSteps-1).</summary>
    public const int ResultsStepIndex = TotalSteps;

    /// <summary>Editing steps plus the results step shown in the header progress dots.</summary>
    public const int ProgressStepCount = TotalSteps + 1;

    public const int MinJugglers = 2;
    public const int MaxJugglers = 8;

    /// <summary>Upper bound for the exact-value juggler number input (beyond the quick-pick pills).</summary>
    public const int MaxJugglersExact = 20;

    public const int MinPeriod = 1;
    public const int MaxPeriod = 30;

    public const int MinClubs = 2;
    public const int MaxClubs = 30;

    /// <summary>
    /// Absolute ceiling for Settings MaxHeight (matches Settings.razor Max="50").
    /// </summary>
    public const int AbsoluteMaxThrowHeight = 50;

    /// <summary>
    /// Highest throw height offered in the throws chip grid.
    /// Loaded from Settings (localStorage key "settings"); default matches SettingsDto.
    /// </summary>
    public int MaxThrowHeight { get; set; } = 13;

    public int CurrentStep { get; set; }

    public HashSet<int> VisitedSteps { get; } = new() { 0 };

    public WizardPhase Phase { get; set; } = WizardPhase.Editing;

    public int NumberOfJugglers { get; set; } = 2;

    public Period Period { get; set; } = new(5);

    public Between Clubs { get; set; } = new() { MinNumber = 5, MaxNumber = 7 };

    public bool ShowThrowNames { get; set; } = true;

    public List<Throw> AllowedThrows { get; } = DefaultThrows();

    /// <summary>
    /// Nested And/Or filter tree. Null root means no filters.
    /// </summary>
    public FilterTree FilterTree { get; set; } = new(null);

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
        NumberOfJugglers = 2;
        Period = new Period(5);
        Clubs = new Between { MinNumber = 5, MaxNumber = 7 };
        ShowThrowNames = true;
        AllowedThrows.Clear();
        AllowedThrows.AddRange(DefaultThrows().Where(t => t.Height <= MaxThrowHeight));
        FilterTree = new FilterTree(null);
        Results.Clear();
        WasCancelled = false;
    }

    public void ApplyMaxThrowHeight(int maxHeight)
    {
        MaxThrowHeight = Math.Clamp(maxHeight, 1, AbsoluteMaxThrowHeight);
        AllowedThrows.RemoveAll(t => t.Height > MaxThrowHeight);
    }

    private static List<Throw> DefaultThrows() =>
        new()
        {
            Throw.Zip,
            Throw.Hold,
            Throw.Zap,
            Throw.Self,
            Throw.SinglePass,
            Throw.Heff,
            Throw.DoublePass,
        };
}
