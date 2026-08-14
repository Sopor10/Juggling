using Siteswap.Details;

namespace Siteswaps.Components.Details;

/// <summary>
/// Snapshot of all displayable pattern properties used by detail variants.
/// Computed once when the query parameters change.
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
        PassSelfLabel = FormatPassSelf(PassOrSelf);
        InterfacePassSelfLabel = FormatPassSelf(InterfacePassOrSelf);
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
                            LocalDisplay: (height * 1.0 / NumberOfJugglers).ToString("0.##"),
                            GlobalDisplay: FormatThrow(height),
                            NameDisplay: FormatThrowName(height),
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
    public string GroundOrExcitedLabel => IsExcitedState ? "Excited" : "Ground";
    public string CurrentState { get; }
    public string Interface { get; }
    public IReadOnlyList<int> Throws { get; }
    public IReadOnlyList<PassOrSelf> PassOrSelf { get; }
    public IReadOnlyList<PassOrSelf> InterfacePassOrSelf { get; }
    public string PassSelfLabel { get; }
    public string InterfacePassSelfLabel { get; }
    public IReadOnlyList<ThrowChip> ThrowChips { get; }
    public IReadOnlyList<ThrowChip> ThrowPassSelfChips { get; }
    public IReadOnlyList<ThrowChip> InterfaceChips { get; }
    public IReadOnlyList<ThrowChip> InterfacePassSelfChips { get; }
    public IReadOnlyList<OrbitRow> Orbits { get; }
    public IReadOnlyList<StateRow> AllStates { get; }
    public IReadOnlyList<JugglerRow> Jugglers { get; }
    public bool ShowLocalAverages { get; }
    public string PassistLink { get; }
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
    /// Common four-handed / passing throw names keyed by global height.
    /// Unknown heights fall back to siteswap digit notation.
    /// </summary>
    public static string FormatThrowName(int height) =>
        height switch
        {
            0 => "0",
            2 => "Zip",
            4 => "Hold",
            5 => "Zap",
            6 => "Self",
            7 => "Single",
            8 => "Heff",
            9 => "Double",
            10 => "Triple S",
            11 => "Triple",
            12 => "Quad",
            13 => "Quad Pass",
            _ => FormatThrow(height),
        };

    private static string FormatPassSelfLetter(global::Siteswap.Details.PassOrSelf value) =>
        value switch
        {
            global::Siteswap.Details.PassOrSelf.Pass => "p",
            global::Siteswap.Details.PassOrSelf.Self => "s",
            _ => "?",
        };

    private static string FormatPassSelf(IReadOnlyList<global::Siteswap.Details.PassOrSelf> values) =>
        string.Concat(values.Select(FormatPassSelfLetter));

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
