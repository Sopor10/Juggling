using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.Feeding;

public sealed record LocalFeedSiteswap(Siteswap Global, string LocalNotation);

public readonly record struct ClubHands(int Left, int Right);

/// <summary>Who currently constrains one beat of A's landing interface.</summary>
public enum FeedInterfaceOwner
{
    Free,
    Self,
    B1,
    B2,
}

public sealed record FeedInterfaceBeat(int Beat, FeedInterfaceOwner Owner);

/// <summary>One interface slot placed on A's visible local timeline.</summary>
public sealed record FeedInterfaceTimelineBeat(
    int LocalBeat,
    int GlobalBeat,
    FeedInterfaceOwner Owner
);

/// <summary>One valid phase of a fedee pattern and the interface beats it claims.</summary>
public sealed record FeedInterfaceOption(int RotationSteps, IReadOnlyList<int> PassBeats);

/// <summary>The source and destination of one displayed throw in the normal-feed topology.</summary>
public sealed record FeedingThrowLanding(
    string SourceRole,
    int SourceLocalBeat,
    int SourceGlobalBeat,
    string TargetRole,
    int TargetLocalBeat,
    int TargetGlobalBeat,
    int Height,
    PassOrSelf Kind
);

/// <summary>
/// Orchestrates a normal three-person feed built from a fixed two-person feeder siteswap.
/// Partner assignment, P/S interface translation, local projection, shared rotation, and
/// starting-club calculation live here — outside Razor and outside Generator Core.
/// </summary>
public sealed class NormalFeedSession
{
    private const int NumberOfJugglersInPair = 2;
    private static readonly string[] FedeeOrder = ["B1", "B2"];

    private readonly Siteswap _originalFeeder;
    private readonly string?[] _passAssignments;
    private readonly Dictionary<string, Siteswap> _selected = new();
    private int _rotationSteps;

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
        _rotationSteps = 0;
    }

    /// <summary>
    /// Undoes cumulative <see cref="Rotate"/> so the feeder matches the original notation
    /// again, co-rotating pass assignments and selected siteswaps to stay aligned.
    /// </summary>
    public void RestoreOriginalRotation()
    {
        if (_rotationSteps == 0)
        {
            return;
        }

        var steps = _rotationSteps;
        _rotationSteps = 0;
        ApplyRotation(-steps);
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
            PassSelfInterface = PartialInterfaceFor(role),
            ThrowInterface = ThrowTimeInterfaceFor(role).ToList(),
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

    public IReadOnlyList<int> OpenPassInterfaceBeats()
    {
        var forcedSelf = SelfInterfaceBeats();
        return Enumerable
            .Range(0, FeederSiteswap.Items.Length)
            .Where(beat => !forcedSelf.Contains(beat))
            .ToList();
    }

    public IReadOnlyList<int> PassInterfaceBeatsOf(Siteswap siteswap)
    {
        ArgumentNullException.ThrowIfNull(siteswap);

        if (siteswap.Items.Length != FeederSiteswap.Items.Length)
        {
            throw new ArgumentException(
                "Siteswap period must match the feeder interface.",
                nameof(siteswap)
            );
        }

        var period = siteswap.Items.Length;
        return Enumerable
            .Range(0, period)
            .Where(i => ToPassOrSelf(siteswap.Items[i]) == PassOrSelf.Pass)
            .Select(i => (i + siteswap.Items[i]) % period)
            .Distinct()
            .Order()
            .ToList();
    }

    public IReadOnlyList<int> ForcedSelfInterfaceBeatsFor(string role)
    {
        EnsureFedeeRole(role);

        var forced = new SortedSet<int>(SelfInterfaceBeats());
        foreach (var earlier in FedeeOrder.TakeWhile(name => name != role))
        {
            if (!_selected.TryGetValue(earlier, out var siteswap))
            {
                continue;
            }

            foreach (var beat in PassInterfaceBeatsOf(siteswap))
            {
                forced.Add(beat);
            }
        }

        return forced.ToList();
    }

    public IReadOnlyList<Throw> PartialInterfaceFor(string role)
    {
        var forcedSelf = ForcedSelfInterfaceBeatsFor(role);
        return Enumerable
            .Range(0, FeederSiteswap.Items.Length)
            .Select(beat => forcedSelf.Contains(beat) ? Throw.AnySelf : Throw.Empty)
            .ToList();
    }

    public IReadOnlyList<FeedInterfaceBeat> FeederInterfaceOccupancy()
    {
        var period = FeederSiteswap.Items.Length;
        var selfBeats = SelfInterfaceBeats().ToHashSet();
        var claims = new Dictionary<int, FeedInterfaceOwner>();

        foreach (var role in FedeeOrder)
        {
            if (!_selected.TryGetValue(role, out var siteswap))
            {
                continue;
            }

            var owner = role == "B1" ? FeedInterfaceOwner.B1 : FeedInterfaceOwner.B2;
            foreach (var beat in PassInterfaceBeatsOf(siteswap))
            {
                claims.TryAdd(beat, owner);
            }
        }

        return Enumerable
            .Range(0, period)
            .Select(beat => new FeedInterfaceBeat(beat, OwnerOf(beat)))
            .ToList();

        FeedInterfaceOwner OwnerOf(int beat) =>
            selfBeats.Contains(beat) ? FeedInterfaceOwner.Self
            : claims.TryGetValue(beat, out var owner) ? owner
            : FeedInterfaceOwner.Free;
    }

    public IReadOnlyList<FeedInterfaceTimelineBeat> FeederInterfaceTimeline()
    {
        var occupancy = FeederInterfaceOccupancy().ToDictionary(beat => beat.Beat);
        var localPeriod = FeederSiteswap.Period.GetLocalPeriod(NumberOfJugglersInPair).Value;
        return Enumerable
            .Range(0, localPeriod)
            .Select(localBeat =>
            {
                var globalBeat = GlobalBeatFor("A", localBeat, FeederSiteswap.Items.Length);
                return new FeedInterfaceTimelineBeat(
                    localBeat,
                    globalBeat,
                    occupancy[globalBeat].Owner
                );
            })
            .ToList();
    }

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

    public void SelectSiteswap(string role, Siteswap siteswap, int? passInterfaceBeat = null)
    {
        EnsureFedeeRole(role);
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

        if (!siteswap.IsValid())
        {
            throw new ArgumentException(
                "Selection must be a valid siteswap; its Interface is undefined otherwise.",
                nameof(siteswap)
            );
        }

        if (
            !TryAlignToFeedInterface(
                role,
                siteswap,
                passInterfaceBeat,
                out var aligned,
                out var failure
            )
        )
        {
            throw new ArgumentException(failure, nameof(siteswap));
        }

        _selected[role] = aligned;
        DropSelectionsIncompatibleWith(role);
    }

    public IReadOnlyList<FeedInterfaceOption> InterfaceOptionsFor(string role, Siteswap siteswap)
    {
        EnsureFedeeRole(role);
        ArgumentNullException.ThrowIfNull(siteswap);

        if (
            !ArePassAssignmentsComplete
            || siteswap.Items.Length != FeederSiteswap.Items.Length
            || !siteswap.IsValid()
        )
        {
            return [];
        }

        var throwTime = ThrowTimeInterfaceFor(role);
        var forcedSelf = ForcedSelfInterfaceBeatsFor(role);
        var options = new List<FeedInterfaceOption>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (var offset = 0; offset < siteswap.Items.Length; offset++)
        {
            var candidate = FeedSiteswapRotation.Rotate(siteswap, offset);
            if (
                !MatchesThrowInterface(throwTime, candidate)
                || ClaimsForcedSelfBeat(candidate, forcedSelf)
            )
            {
                continue;
            }

            var passBeats = PassInterfaceBeatsOf(candidate);
            if (seen.Add(string.Join(",", passBeats)))
            {
                options.Add(new FeedInterfaceOption(offset, passBeats));
            }
        }

        return options;
    }

    public IReadOnlyList<int> SelectablePassInterfaceBeatsFor(
        string role,
        IEnumerable<Siteswap> candidates
    )
    {
        ArgumentNullException.ThrowIfNull(candidates);

        var beats = new SortedSet<int>();
        foreach (var candidate in candidates)
        {
            foreach (var beat in InterfaceOptionsFor(role, candidate).SelectMany(o => o.PassBeats))
            {
                beats.Add(beat);
            }
        }

        return beats.ToList();
    }

    public bool TrySelectPassInterfaceBeat(string role, int beat, IEnumerable<Siteswap> candidates)
    {
        EnsureFedeeRole(role);
        ArgumentNullException.ThrowIfNull(candidates);

        var ordered = candidates.ToList();
        if (SelectedSiteswap(role) is { } current)
        {
            ordered.Insert(0, current);
        }

        foreach (var candidate in ordered)
        {
            if (InterfaceOptionsFor(role, candidate).Any(option => option.PassBeats.Contains(beat)))
            {
                SelectSiteswap(role, candidate, beat);
                return true;
            }
        }

        return false;
    }

    private void DropSelectionsIncompatibleWith(string changedRole)
    {
        foreach (var role in _selected.Keys.Where(key => key != changedRole).ToList())
        {
            if (ClaimsForcedSelfBeat(_selected[role], ForcedSelfInterfaceBeatsFor(role)))
            {
                _selected.Remove(role);
            }
        }
    }

    public Siteswap? SelectedSiteswap(string role) =>
        _selected.TryGetValue(role, out var siteswap) ? siteswap : null;

    public FeedingThrowLanding LandingFor(string role, int localBeat)
    {
        var siteswap =
            role == "A"
                ? FeederSiteswap
                : SelectedSiteswap(role)
                    ?? throw new InvalidOperationException($"No siteswap selected for {role}.");
        var localPeriod = siteswap.Period.GetLocalPeriod(NumberOfJugglersInPair).Value;
        if (localBeat < 0 || localBeat >= localPeriod)
        {
            throw new ArgumentOutOfRangeException(nameof(localBeat));
        }

        var period = siteswap.Items.Length;
        var sourceGlobalBeat = GlobalBeatFor(role, localBeat, period);
        var height = siteswap.Items[sourceGlobalBeat];
        var kind = ToPassOrSelf(height);
        var targetRole =
            kind == PassOrSelf.Self
                ? role
                : PassingPartnerFor(role, Throw.AnyPass, sourceGlobalBeat);
        var targetGlobalBeat = PositiveModulo(sourceGlobalBeat + height, period);
        var targetTimeZone = Topology[targetRole].TimeZone;
        var targetLocalBeat = Enumerable
            .Range(0, localPeriod)
            .Single(beat =>
                PositiveModulo(targetTimeZone + beat * NumberOfJugglersInPair, period)
                == targetGlobalBeat
            );

        return new FeedingThrowLanding(
            role,
            localBeat,
            sourceGlobalBeat,
            targetRole,
            targetLocalBeat,
            targetGlobalBeat,
            height,
            kind
        );
    }

    public void Rotate(int steps)
    {
        if (steps == 0)
        {
            return;
        }

        _rotationSteps += steps;
        ApplyRotation(steps);
    }

    private void ApplyRotation(int steps)
    {
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

    private List<Throw> BuildThrowTimeInterface(string role)
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

    private bool TryAlignToFeedInterface(
        string role,
        Siteswap siteswap,
        int? passInterfaceBeat,
        out Siteswap aligned,
        out string? failure
    )
    {
        var throwTime = ThrowTimeInterfaceFor(role);
        var forcedSelf = ForcedSelfInterfaceBeatsFor(role);
        var matchedThrowTime = false;
        var matchedOpenInterface = false;

        for (var offset = 0; offset < siteswap.Items.Length; offset++)
        {
            var candidate = FeedSiteswapRotation.Rotate(siteswap, offset);
            if (!MatchesThrowInterface(throwTime, candidate))
            {
                continue;
            }

            matchedThrowTime = true;
            if (ClaimsForcedSelfBeat(candidate, forcedSelf))
            {
                continue;
            }

            matchedOpenInterface = true;
            if (passInterfaceBeat is { } beat && !PassInterfaceBeatsOf(candidate).Contains(beat))
            {
                continue;
            }

            aligned = candidate;
            failure = null;
            return true;
        }

        aligned = siteswap;
        failure =
            !matchedThrowTime
                ? "Selection must pass on the beats the feeder throws to this fedee on."
            : !matchedOpenInterface
                ? "Selection places a Pass on an Interface beat of A that is already forced to Self."
            : $"Selection cannot place a Pass on Interface beat {passInterfaceBeat}.";
        return false;
    }

    private static bool MatchesThrowInterface(IReadOnlyList<Throw> throwTime, Siteswap siteswap)
    {
        for (var i = 0; i < throwTime.Count; i++)
        {
            var required =
                throwTime[i].Height == Throw.AnyPass.Height ? PassOrSelf.Pass : PassOrSelf.Self;
            if (ToPassOrSelf(siteswap.Items[i]) != required)
            {
                return false;
            }
        }

        return true;
    }

    private List<int> SelfInterfaceBeats()
    {
        var period = FeederSiteswap.Items.Length;
        return Enumerable
            .Range(0, period)
            .Where(i => ToPassOrSelf(FeederSiteswap.Items[i]) == PassOrSelf.Self)
            .Select(i => (i + FeederSiteswap.Items[i]) % period)
            .Distinct()
            .Order()
            .ToList();
    }

    private bool ClaimsForcedSelfBeat(Siteswap siteswap, IReadOnlyList<int> forcedSelfBeats) =>
        forcedSelfBeats.Count > 0 && PassInterfaceBeatsOf(siteswap).Any(forcedSelfBeats.Contains);

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

    private static void EnsureFedeeRole(string role)
    {
        if (role is not ("B1" or "B2"))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown feed role.");
        }
    }

    private void InvalidateSelections() => _selected.Clear();

    private int GlobalBeatFor(string role, int localBeat, int period) =>
        PositiveModulo(Topology[role].TimeZone + localBeat * NumberOfJugglersInPair, period);

    private static int PositiveModulo(int value, int modulus) =>
        ((value % modulus) + modulus) % modulus;

    private static PassOrSelf ToPassOrSelf(int height) =>
        height % NumberOfJugglersInPair == 0 ? PassOrSelf.Self : PassOrSelf.Pass;
}
