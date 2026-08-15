using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.Feeding;

public sealed record LocalFeedSiteswap(Siteswap Global, string LocalNotation);

public readonly record struct ClubHands(int Left, int Right);

/// <summary>
/// Orchestrates a normal three-person feed built from a fixed two-person feeder siteswap.
/// Partner assignment, P/S interface translation, local projection, shared rotation, and
/// starting-club calculation live here — outside Razor and outside Generator Core.
/// </summary>
public sealed class NormalFeedSession
{
    private const int NumberOfJugglersInPair = 2;

    private readonly Siteswap _originalFeeder;
    private readonly string?[] _passAssignments;
    private readonly Dictionary<string, Siteswap> _selected = new();

    private NormalFeedSession(Siteswap feederSiteswap, NormalFeed topology)
    {
        Topology = topology;
        _originalFeeder = feederSiteswap;
        FeederSiteswap = feederSiteswap;
        _passAssignments = new string?[feederSiteswap.Items.Length];
        ClubsB1 = CreateDefaultClubs();
        ClubsB2 = CreateDefaultClubs();
    }

    public NormalFeed Topology { get; }

    public Siteswap FeederSiteswap { get; private set; }

    /// <summary>Explicit club bounds for B1 (generator input, not inferred).</summary>
    public Between ClubsB1 { get; set; }

    /// <summary>Explicit club bounds for B2 (generator input, not inferred).</summary>
    public Between ClubsB2 { get; set; }

    public IReadOnlyList<string?> PassAssignments => Array.AsReadOnly(_passAssignments);

    public IReadOnlyList<PassOrSelf> ThrowKinds =>
        FeederSiteswap.Items.Select(ToPassOrSelf).ToList();

    public IReadOnlyList<int> PassBeatIndexes =>
        FeederSiteswap
            .Items.Select((height, index) => (height, index))
            .Where(x => ToPassOrSelf(x.height) == PassOrSelf.Pass)
            .Select(x => x.index)
            .ToList();

    public IReadOnlyList<int> RemainingPassBeats =>
        PassBeatIndexes.Where(i => _passAssignments[i] is null).ToList();

    public bool ArePassAssignmentsComplete =>
        PassBeatIndexes.Count > 0 && PassBeatIndexes.All(i => _passAssignments[i] is "B1" or "B2");

    public bool CanGenerate => GenerationBlockCode == GenerationBlockCode.None;

    public GenerationBlockCode GenerationBlockCode
    {
        get
        {
            if (PassBeatIndexes.Count == 0)
            {
                return GenerationBlockCode.NoPasses;
            }

            if (!ArePassAssignmentsComplete)
            {
                return GenerationBlockCode.IncompleteAssignments;
            }

            if (!BothFedeesReceiveAtLeastOnePass())
            {
                return GenerationBlockCode.SingleFedeeOnly;
            }

            if (!AreClubsConfigured(ClubsB1) || !AreClubsConfigured(ClubsB2))
            {
                return GenerationBlockCode.ClubsUnset;
            }

            return GenerationBlockCode.None;
        }
    }

    /// <summary>
    /// Stable English fallback for tests/diagnostics. UI should localize via
    /// <see cref="GenerationBlockCode"/>.
    /// </summary>
    public string? GenerationBlockReason =>
        GenerationBlockCode switch
        {
            GenerationBlockCode.None => null,
            GenerationBlockCode.NoPasses => "Feeder has no passes.",
            GenerationBlockCode.IncompleteAssignments => "Pass assignments are incomplete.",
            GenerationBlockCode.SingleFedeeOnly => "Each fedee must receive at least one pass.",
            GenerationBlockCode.ClubsUnset => "Club bounds for B1 and B2 must be set.",
            _ => "Generation is blocked.",
        };

    public static NormalFeedSession FromFeederSiteswap(Siteswap feederSiteswap)
    {
        ArgumentNullException.ThrowIfNull(feederSiteswap);

        var passCount = feederSiteswap.Items.Count(height =>
            ToPassOrSelf(height) == PassOrSelf.Pass
        );
        if (passCount < 2)
        {
            throw new ArgumentException(
                "Feeder must be a two-person pattern with at least two passes (one for each fedee).",
                nameof(feederSiteswap)
            );
        }

        return new NormalFeedSession(feederSiteswap, NormalFeed.Create());
    }

    public void AssignPass(int beatIndex, string partner)
    {
        if (beatIndex < 0 || beatIndex >= FeederSiteswap.Items.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(beatIndex));
        }

        if (ToPassOrSelf(FeederSiteswap.Items[beatIndex]) != PassOrSelf.Pass)
        {
            throw new ArgumentException(
                $"Beat {beatIndex} is a self throw and cannot receive a passing partner.",
                nameof(beatIndex)
            );
        }

        if (partner is not ("B1" or "B2"))
        {
            throw new ArgumentOutOfRangeException(
                nameof(partner),
                partner,
                "Partner must be B1 or B2."
            );
        }

        _passAssignments[beatIndex] = partner;
        InvalidateSelections();
    }

    public void ClearPass(int beatIndex)
    {
        if (beatIndex < 0 || beatIndex >= FeederSiteswap.Items.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(beatIndex));
        }

        if (ToPassOrSelf(FeederSiteswap.Items[beatIndex]) != PassOrSelf.Pass)
        {
            throw new ArgumentException(
                $"Beat {beatIndex} is a self throw and has no pass assignment.",
                nameof(beatIndex)
            );
        }

        _passAssignments[beatIndex] = null;
        InvalidateSelections();
    }

    public void Reset()
    {
        FeederSiteswap = _originalFeeder;
        Array.Fill(_passAssignments, null);
        _selected.Clear();
        ClubsB1 = CreateDefaultClubs();
        ClubsB2 = CreateDefaultClubs();
    }

    public IReadOnlyList<Throw> ThrowTimeInterfaceFor(string role) => BuildThrowTimeInterface(role);

    /// <summary>
    /// Builds a locked generation-workflow config for generating one fedee's pair pattern.
    /// </summary>
    public GenerationWorkflowConfig ToGenerationWorkflowConfig(string role) =>
        new()
        {
            Period = FeederSiteswap.Items.Length,
            NumberOfJugglers = 2,
            PassSelfInterface = InterfaceFor(role).ToList(),
            Clubs = role switch
            {
                "B1" => ClubsB1,
                "B2" => ClubsB2,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(role),
                    role,
                    "Unknown feed role."
                ),
            },
        };

    public IReadOnlyList<Throw> InterfaceFor(string role)
    {
        var throwTime = BuildThrowTimeInterface(role);
        return FeedInterface.RotateToLanding(FeederSiteswap.Items, throwTime);
    }

    /// <summary>
    /// B1/B2 only pass back to A; for A without a beat, returns one assigned/topology partner.
    /// </summary>
    public string PassingPartnerFor(string role, Throw throwKind)
    {
        EnsurePassThrowKind(throwKind);

        if (role == "A")
        {
            throw new InvalidOperationException(
                "PassingPartnerFor for A requires a beat index; partners differ per pass beat — use the 3-argument overload."
            );
        }

        return Topology[role].PassingPartners.Single();
    }

    /// <summary>
    /// Beat-aware partner lookup — required for feeder A, whose passes go to different fedees.
    /// </summary>
    public string PassingPartnerFor(string role, Throw throwKind, int beatIndex)
    {
        EnsurePassThrowKind(throwKind);

        if (beatIndex < 0 || beatIndex >= FeederSiteswap.Items.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(beatIndex));
        }

        if (role == "A")
        {
            if (ToPassOrSelf(FeederSiteswap.Items[beatIndex]) != PassOrSelf.Pass)
            {
                throw new ArgumentException(
                    $"Beat {beatIndex} is a self throw and has no passing partner.",
                    nameof(beatIndex)
                );
            }

            return _passAssignments[beatIndex]
                ?? throw new InvalidOperationException(
                    $"Pass at beat {beatIndex} is not assigned yet."
                );
        }

        return Topology[role].PassingPartners.Single();
    }

    public IReadOnlyList<LocalFeedSiteswap> ProjectLocalResults(
        string role,
        IEnumerable<Siteswap> globalResults
    )
    {
        var timeZone = Topology[role].TimeZone;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<LocalFeedSiteswap>();

        foreach (var global in globalResults)
        {
            var local = global.GetLocalSiteswap(timeZone, NumberOfJugglersInPair);
            var notation = local.GlobalNotation;
            if (!seen.Add(notation))
            {
                continue;
            }

            result.Add(new LocalFeedSiteswap(global, notation));
        }

        return result;
    }

    public void SelectSiteswap(string role, Siteswap siteswap)
    {
        if (role is not ("B1" or "B2"))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown feed role.");
        }

        ArgumentNullException.ThrowIfNull(siteswap);

        if (!ArePassAssignmentsComplete)
        {
            throw new InvalidOperationException(
                "Pass assignments must be complete before selecting a siteswap."
            );
        }

        if (siteswap.Items.Length != FeederSiteswap.Items.Length)
        {
            throw new ArgumentException(
                "Selected siteswap period must match the feeder/interface.",
                nameof(siteswap)
            );
        }

        EnsureSelectionMatchesLandingInterface(role, siteswap);

        _selected[role] = siteswap;
    }

    public Siteswap? SelectedSiteswap(string role) =>
        _selected.TryGetValue(role, out var siteswap) ? siteswap : null;

    public void Rotate(int steps)
    {
        if (steps == 0)
        {
            return;
        }

        FeederSiteswap = FeedSiteswapRotation.Rotate(FeederSiteswap, steps);
        FeedSiteswapRotation.RotateInPlace(_passAssignments, steps);

        foreach (var key in _selected.Keys.ToList())
        {
            _selected[key] = FeedSiteswapRotation.Rotate(_selected[key], steps);
        }
    }

    public ClubHands StartingClubs(string role)
    {
        if (!TryStartingClubs(role, out var clubs))
        {
            throw new InvalidOperationException($"No siteswap selected for {role}.");
        }

        return clubs;
    }

    public bool TryStartingClubs(string role, out ClubHands clubs)
    {
        if (role is not ("A" or "B1" or "B2"))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown feed role.");
        }

        if (role == "A")
        {
            clubs = StartingClubDistribution.ForJuggler(FeederSiteswap.Items, Topology.A.TimeZone);
            return true;
        }

        if (SelectedSiteswap(role) is not { } siteswap)
        {
            clubs = default;
            return false;
        }

        clubs = StartingClubDistribution.ForJuggler(siteswap.Items, Topology[role].TimeZone);
        return true;
    }

    private IReadOnlyList<Throw> BuildThrowTimeInterface(string role)
    {
        if (!ArePassAssignmentsComplete)
        {
            throw new InvalidOperationException(
                "Pass assignments are incomplete; generation and interface translation are blocked."
            );
        }

        if (role is not ("B1" or "B2"))
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "Interface is built for B1 or B2."
            );
        }

        return FeederSiteswap
            .Items.Select(
                (height, i) =>
                    ToPassOrSelf(height) == PassOrSelf.Pass && _passAssignments[i] == role
                        ? Throw.AnyPass
                        : Throw.AnySelf
            )
            .ToList();
    }

    private void EnsureSelectionMatchesLandingInterface(string role, Siteswap siteswap)
    {
        var landing = InterfaceFor(role);
        for (var i = 0; i < landing.Count; i++)
        {
            var required =
                landing[i].Height == Throw.AnyPass.Height ? PassOrSelf.Pass : PassOrSelf.Self;
            var actual = ToPassOrSelf(siteswap.Items[i]);
            if (required != actual)
            {
                throw new ArgumentException(
                    "Selection must place passes on the landing beats required by the interface.",
                    nameof(siteswap)
                );
            }
        }
    }

    private bool BothFedeesReceiveAtLeastOnePass()
    {
        var hasB1 = false;
        var hasB2 = false;
        foreach (var beat in PassBeatIndexes)
        {
            switch (_passAssignments[beat])
            {
                case "B1":
                    hasB1 = true;
                    break;
                case "B2":
                    hasB2 = true;
                    break;
            }
        }

        return hasB1 && hasB2;
    }

    private static bool AreClubsConfigured(Between clubs) =>
        clubs.MinNumber >= WizardState.MinClubs
        && clubs.MaxNumber >= clubs.MinNumber
        && clubs.MaxNumber <= WizardState.MaxClubs;

    /// <summary>
    /// Same default club window as the wizard so DualRangeSlider display and
    /// session state stay aligned (no 0–0 vs clamped 2–2 mismatch).
    /// </summary>
    private static Between CreateDefaultClubs() => new() { MinNumber = 5, MaxNumber = 7 };

    private static void EnsurePassThrowKind(Throw throwKind)
    {
        if (throwKind.Height != Throw.AnyPass.Height)
        {
            throw new ArgumentException(
                "Only pass throws have a passing partner.",
                nameof(throwKind)
            );
        }
    }

    private void InvalidateSelections() => _selected.Clear();

    private static PassOrSelf ToPassOrSelf(int height) =>
        height % NumberOfJugglersInPair == 0 ? PassOrSelf.Self : PassOrSelf.Pass;
}
