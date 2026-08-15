using Siteswap.Details;

namespace Siteswaps.Components.Details;

/// <summary>
/// Thin facade over a <see cref="Siteswap"/> for the details page:
/// derived domain values and projections only — no UI copy.
/// </summary>
public sealed class DetailViewModel
{
    private DetailViewModel(global::Siteswap.Details.Siteswap value, int numberOfJugglers)
    {
        Value = value;
        NumberOfJugglers = Math.Max(1, numberOfJugglers);
        Notation = value.ToString();
        Period = value.Period.Value;
        LocalPeriod = value.Period.GetLocalPeriod(NumberOfJugglers).Value;
        NumberOfObjects = value.NumberOfObjects();
        MaxHeight = value.Max();
        Length = value.Length;
        IsExcitedState = value.IsExcitedState();
        CurrentState = value.State.StateRepresentation();
        Interface = value.Interface.ToString();
        Throws = value.Items.EnumerateValues(1).ToList();
        PassOrSelf = value.GetPassOrSelf(NumberOfJugglers).ToList();
        InterfacePassOrSelf = value.Interface.GetPassOrSelf(NumberOfJugglers).ToList();
        ThrowChips = Throws
            .Select((height, i) => new ThrowChip(FormatThrow(height), PassOrSelf[i]))
            .ToList();
        ThrowPassSelfChips = PassOrSelf
            .Select(kind => new ThrowChip(FormatPassSelfLetter(kind), kind))
            .ToList();
        InterfaceChips = Interface
            .Select((ch, i) => new ThrowChip(ch.ToString(), InterfacePassOrSelf[i]))
            .ToList();
        InterfacePassSelfChips = InterfacePassOrSelf
            .Select(kind => new ThrowChip(FormatPassSelfLetter(kind), kind))
            .ToList();
        Orbits = value
            .GetOrbits()
            .Select(o => new OrbitRow(o.DisplayValue, o.Items.ToList()))
            .ToList();
        AllStates = value
            .AllStates()
            .Select(kvp => new StateRow(
                kvp.Key.StateRepresentation(),
                kvp.Value.Select(s => s.ToString()).ToList()
            ))
            .ToList();
        Jugglers = Enumerable
            .Range(0, NumberOfJugglers)
            .Select(i =>
            {
                var local = value.GetLocalSiteswap(i, NumberOfJugglers);
                var localThrows = Enumerable
                    .Range(0, LocalPeriod)
                    .Select(beat =>
                    {
                        var globalIndex = (i + beat * NumberOfJugglers) % Period;
                        var height = value.Items[i + beat * NumberOfJugglers];
                        return new JugglerThrow(
                            LocalDisplay: PassingThrowNames.ToLocalDisplay(
                                height,
                                NumberOfJugglers
                            ),
                            GlobalDisplay: FormatThrow(height),
                            NameDisplay: FormatThrowName(height, NumberOfJugglers),
                            Kind: PassOrSelf[globalIndex]
                        );
                    })
                    .ToList();

                return new JugglerRow(
                    Index: i,
                    Label: ((char)('A' + i)).ToString(),
                    LocalNotation: local.LocalNotation,
                    GlobalNotation: local.GlobalNotation,
                    AverageObjects: local.Average(),
                    Throws: localThrows
                );
            })
            .ToList();
        ShowLocalAverages = Period % NumberOfJugglers == 0;
        PassistLink = $"https://passist.org/siteswap/{Notation}?jugglers={NumberOfJugglers}";
        ThreePersonFeedHref = $"feeding?s={Uri.EscapeDataString(Notation)}";
        CanCreateThreePersonFeed =
            NumberOfJugglers == 2
            && PassOrSelf.Any(kind => kind == global::Siteswap.Details.PassOrSelf.Pass);
        Diagrams = Siteswap.Details.CausalDiagram.SiteswapDiagramBuilder.Build(
            value,
            NumberOfJugglers
        );
    }

    public global::Siteswap.Details.Siteswap Value { get; }
    public int NumberOfJugglers { get; }
    public string Notation { get; }
    public int Period { get; }
    public int LocalPeriod { get; }
    public decimal NumberOfObjects { get; }
    public int NumberOfClubs => (int)NumberOfObjects;
    public int MaxHeight { get; }
    public int Length { get; }
    public bool IsExcitedState { get; }
    public string CurrentState { get; }
    public string Interface { get; }
    public IReadOnlyList<int> Throws { get; }
    public IReadOnlyList<PassOrSelf> PassOrSelf { get; }
    public IReadOnlyList<PassOrSelf> InterfacePassOrSelf { get; }
    public IReadOnlyList<ThrowChip> ThrowChips { get; }
    public IReadOnlyList<ThrowChip> ThrowPassSelfChips { get; }
    public IReadOnlyList<ThrowChip> InterfaceChips { get; }
    public IReadOnlyList<ThrowChip> InterfacePassSelfChips { get; }
    public IReadOnlyList<OrbitRow> Orbits { get; }
    public IReadOnlyList<StateRow> AllStates { get; }
    public IReadOnlyList<JugglerRow> Jugglers { get; }
    public bool ShowLocalAverages { get; }
    public string PassistLink { get; }
    public string ThreePersonFeedHref { get; }
    public bool CanCreateThreePersonFeed { get; }
    public Siteswap.Details.CausalDiagram.DiagramSet Diagrams { get; }

    public static DetailViewModel? TryCreate(string? notation, int numberOfJugglers)
    {
        if (string.IsNullOrWhiteSpace(notation))
        {
            return null;
        }

        return global::Siteswap.Details.Siteswap.TryCreate(notation, out var value)
            ? new DetailViewModel(value, numberOfJugglers)
            : null;
    }

    public static string FormatThrow(int height) =>
        global::Siteswap.Details.Siteswap.Transform(height);

    /// <summary>
    /// Passing throw names for a global height, scaled by juggler count
    /// (see <see cref="PassingThrowNames"/>).
    /// </summary>
    public static string FormatThrowName(int height, int numberOfJugglers) =>
        PassingThrowNames.Format(height, numberOfJugglers);

    private static string FormatPassSelfLetter(global::Siteswap.Details.PassOrSelf value) =>
        value switch
        {
            global::Siteswap.Details.PassOrSelf.Pass => "p",
            global::Siteswap.Details.PassOrSelf.Self => "s",
            _ => "?",
        };

    public enum ThrowDisplayMode
    {
        Local,
        Global,
        Name,
    }

    public record ThrowChip(string Display, global::Siteswap.Details.PassOrSelf Kind);

    public record JugglerThrow(
        string LocalDisplay,
        string GlobalDisplay,
        string NameDisplay,
        global::Siteswap.Details.PassOrSelf Kind
    )
    {
        public ThrowChip ForMode(ThrowDisplayMode mode) =>
            mode switch
            {
                ThrowDisplayMode.Global => new ThrowChip(GlobalDisplay, Kind),
                ThrowDisplayMode.Name => new ThrowChip(NameDisplay, Kind),
                _ => new ThrowChip(LocalDisplay, Kind),
            };
    }

    public record JugglerRow(
        int Index,
        string Label,
        string LocalNotation,
        string GlobalNotation,
        double AverageObjects,
        IReadOnlyList<JugglerThrow> Throws
    );

    public record OrbitRow(string DisplayValue, IReadOnlyList<int> Items);

    public record StateRow(string State, IReadOnlyList<string> Notations);
}
