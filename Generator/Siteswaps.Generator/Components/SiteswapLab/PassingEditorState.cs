using System.Globalization;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.SiteswapLab;

public sealed class PassingEditorState
{
    public const int MinPeople = 1;
    public const int MaxPeople = 8;

    private readonly List<PassingEditorPerson> _people = [];

    public PassingEditorState(string notation = "531", int? maxThrowHeight = null)
    {
        MaxThrowHeight = SettingsDto.ClampMaxHeight(maxThrowHeight ?? new SettingsDto().MaxHeight);
        var heights = ParseHeights(notation).Select(height => Math.Min(height, MaxThrowHeight));
        _people.Add(
            new PassingEditorPerson(
                "A",
                0,
                heights.Select(height => new PassingEditorCell(height, 0)).ToList()
            )
        );
    }

    public IReadOnlyList<PassingEditorPerson> People => _people;

    public int Period => _people[0].Cells.Count;

    public int PhaseCount => _people.Count;

    public bool ThrowsInitialized { get; private set; }

    public int MaxThrowHeight { get; private set; }

    public int HeightLimitViolationCount =>
        _people.SelectMany(person => person.Cells).Count(cell => cell.Height > MaxThrowHeight);

    public PassingTargetAdjustment? LastTargetAdjustment { get; private set; }

    public int SelectedPerson { get; private set; }

    public int SelectedBeat { get; private set; }

    public PassingEditorLanding SelectedLanding => LandingFor(SelectedPerson, SelectedBeat);

    public double Average =>
        _people.SelectMany(person => person.Cells).Sum(cell => cell.Height)
        / (double)(Period * PhaseCount);

    public string AverageLabel => Average.ToString("0.##", CultureInfo.InvariantCulture);

    public IReadOnlyList<string> ConfigurationErrors
    {
        get
        {
            var errors = new List<string>();
            for (var person = 0; person < _people.Count; person++)
            {
                for (var beat = 0; beat < Period; beat++)
                {
                    var cell = _people[person].Cells[beat];
                    if (cell.TargetPerson is null)
                    {
                        errors.Add($"{_people[person].Name}, beat {beat + 1} has no target.");
                    }
                }
            }

            return errors;
        }
    }

    public IReadOnlySet<PassingLandingSlot> CollisionTargets =>
        AllLandings()
            .Where(landing => landing.TargetPerson is not null)
            .GroupBy(landing => new PassingLandingSlot(
                landing.TargetPerson!.Value,
                landing.TargetBeat
            ))
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet();

    public IReadOnlySet<PassingLandingSlot> EmptyTargets
    {
        get
        {
            var occupied = AllLandings()
                .Where(landing => landing.TargetPerson is not null)
                .Select(landing => new PassingLandingSlot(
                    landing.TargetPerson!.Value,
                    landing.TargetBeat
                ))
                .ToHashSet();
            return Enumerable
                .Range(0, _people.Count)
                .SelectMany(person =>
                    Enumerable.Range(0, Period).Select(beat => new PassingLandingSlot(person, beat))
                )
                .Where(slot => !occupied.Contains(slot))
                .ToHashSet();
        }
    }

    public bool IsValid =>
        HeightLimitViolationCount == 0
        && ConfigurationErrors.Count == 0
        && CollisionTargets.Count == 0
        && EmptyTargets.Count == 0;

    public string Notation =>
        string.Join(
            " | ",
            _people.Select(
                (person, personIndex) =>
                    $"{person.Name}: {string.Join(" ", person.Cells.Select((cell, beat) => FormatCell(personIndex, beat, cell)))}"
            )
        );

    public string LocalNotationFor(int person) =>
        string.Join(
            " ",
            _people[person]
                .Cells.Select(cell =>
                    (cell.Height / (double)PhaseCount).ToString(
                        "0.##",
                        CultureInfo.InvariantCulture
                    )
                )
        );

    public void SelectCell(int person, int beat)
    {
        SelectedPerson = Math.Clamp(person, 0, _people.Count - 1);
        SelectedBeat = Math.Clamp(beat, 0, Period - 1);
    }

    public void ApplyMaxThrowHeight(int maxThrowHeight)
    {
        MaxThrowHeight = SettingsDto.ClampMaxHeight(maxThrowHeight);
    }

    public void SetPersonCount(int count)
    {
        LastTargetAdjustment = null;
        count = Math.Clamp(count, MinPeople, MaxPeople);
        var previousCount = _people.Count;
        while (_people.Count < count)
        {
            var index = _people.Count;
            var newPhaseCount = index + 1;
            _people.Add(
                new PassingEditorPerson(
                    DefaultName(index),
                    index,
                    Enumerable
                        .Range(0, Period)
                        .Select(_ => new PassingEditorCell(
                            DefaultHeight(newPhaseCount, MaxThrowHeight),
                            index
                        ))
                        .ToList()
                )
            );
        }

        while (_people.Count > count)
        {
            _people.RemoveAt(_people.Count - 1);
        }

        foreach (var person in _people)
        {
            person.TimeZone = Math.Clamp(person.TimeZone, 0, count - 1);
        }

        if (ThrowsInitialized && count > previousCount)
        {
            for (var person = previousCount; person < count; person++)
            {
                for (var beat = 0; beat < Period; beat++)
                {
                    InitializeLocalThreeCell(person, beat);
                }
            }
        }

        SelectedPerson = Math.Min(SelectedPerson, _people.Count - 1);
        NormalizeTargets();
    }

    public void CycleTimeZone(int person)
    {
        LastTargetAdjustment = null;
        person = Math.Clamp(person, 0, _people.Count - 1);
        var current = _people[person];
        current.TimeZone = (current.TimeZone + 1) % PhaseCount;
        NormalizeTargets();
    }

    public void SetHeight(int person, int beat, int height)
    {
        LastTargetAdjustment = null;
        var cell = _people[person].Cells[beat];
        cell.Height = Math.Clamp(height, 0, MaxThrowHeight);
        SelectCell(person, beat);
        NormalizeTarget(person, beat);
    }

    public bool SetTarget(int person, int beat, int targetPerson)
    {
        LastTargetAdjustment = null;
        if (
            person < 0
            || person >= _people.Count
            || beat < 0
            || beat >= Period
            || !AvailableTargetsFor(person, beat, _people[person].Cells[beat].Height)
                .Contains(targetPerson)
        )
        {
            return false;
        }

        if (!CanSetTarget(person, beat, targetPerson))
        {
            return false;
        }

        var cell = _people[person].Cells[beat];
        cell.TargetPerson = targetPerson;
        SelectCell(person, beat);
        return true;
    }

    public bool CanSetTarget(int person, int beat, int targetPerson) =>
        AvailableTargetsFor(person, beat, _people[person].Cells[beat].Height)
            .Contains(targetPerson);

    public IReadOnlyList<int> AvailableTargetsFor(int sourcePerson, int sourceBeat, int height)
    {
        if (
            sourcePerson < 0
            || sourcePerson >= _people.Count
            || sourceBeat < 0
            || sourceBeat >= Period
        )
        {
            return [];
        }

        var landingTimeZone = PositiveModulo(_people[sourcePerson].TimeZone + height, PhaseCount);
        return _people
            .Select((person, index) => (person, index))
            .Where(item => item.person.TimeZone == landingTimeZone)
            .Select(item => item.index)
            .ToArray();
    }

    public int LandingTimeZoneFor(int person, int beat)
    {
        var source = _people[person];
        return PositiveModulo(source.TimeZone + source.Cells[beat].Height, PhaseCount);
    }

    public int TimelinePhaseFor(int person) => _people[person].TimeZone;

    public int ToGlobalHeight(int localHeight) => localHeight * PhaseCount;

    public void InitializeThrowsForFirstEntry()
    {
        if (ThrowsInitialized)
        {
            return;
        }

        for (var person = 0; person < _people.Count; person++)
        {
            for (var beat = 0; beat < Period; beat++)
            {
                InitializeLocalThreeCell(person, beat);
            }
        }

        ThrowsInitialized = true;
        LastTargetAdjustment = null;
    }

    public PassingEditorLanding LandingFor(int person, int beat)
    {
        var source = _people[person];
        var cell = source.Cells[beat];
        var targetBeat = LandingBeatFor(person, beat);
        return new PassingEditorLanding(
            person,
            beat,
            cell.TargetPerson,
            targetBeat,
            LandingTimeZoneFor(person, beat),
            cell.Height,
            cell.TargetPerson == person ? PassOrSelf.Self : PassOrSelf.Pass
        );
    }

    public IReadOnlyList<PassingEditorLanding> SourcesLandingAt(int person, int beat) =>
        AllLandings()
            .Where(landing => landing.TargetPerson == person && landing.TargetBeat == beat)
            .ToList();

    public void AddBeat()
    {
        if (Period >= Siteswap.MaxPeriodLength)
        {
            return;
        }

        for (var person = 0; person < _people.Count; person++)
        {
            _people[person]
                .MutableCells.Add(
                    new PassingEditorCell(
                        ThrowsInitialized
                            ? ToGlobalHeight(3)
                            : DefaultHeight(PhaseCount, MaxThrowHeight),
                        person
                    )
                );
        }

        SelectedBeat = Period - 1;
    }

    public void RemoveBeat()
    {
        if (Period <= 1)
        {
            return;
        }

        foreach (var person in _people)
        {
            person.MutableCells.RemoveAt(person.Cells.Count - 1);
        }

        SelectedBeat = Math.Min(SelectedBeat, Period - 1);
    }

    private void InitializeLocalThreeCell(int person, int beat)
    {
        var cell = _people[person].MutableCells[beat];
        cell.Height = ToGlobalHeight(3);
        var available = AvailableTargetsFor(person, beat, cell.Height);
        cell.TargetPerson = available.Contains(person)
            ? person
            : available.Select(target => (int?)target).FirstOrDefault();
    }

    private static PassingEditorPerson CreatePerson(
        string name,
        int timeZone,
        IReadOnlyList<int> heights,
        IReadOnlyList<int> targets
    ) =>
        new(
            name,
            timeZone,
            heights
                .Select((height, index) => new PassingEditorCell(height, targets[index]))
                .ToList()
        );

    private List<PassingEditorLanding> AllLandings() =>
        _people
            .SelectMany(
                (_, person) => Enumerable.Range(0, Period).Select(beat => LandingFor(person, beat))
            )
            .ToList();

    private void NormalizeTargets()
    {
        for (var person = 0; person < _people.Count; person++)
        {
            for (var beat = 0; beat < Period; beat++)
            {
                NormalizeTarget(person, beat);
            }
        }
    }

    private void NormalizeTarget(int person, int beat)
    {
        var cell = _people[person].Cells[beat];
        var available = AvailableTargetsFor(person, beat, cell.Height);
        if (cell.TargetPerson is { } target && available.Contains(target))
        {
            return;
        }

        var previousTarget = cell.TargetPerson;
        cell.TargetPerson = available.Select(target => (int?)target).FirstOrDefault();
        LastTargetAdjustment = new PassingTargetAdjustment(
            person,
            beat,
            previousTarget,
            cell.TargetPerson,
            LandingBeatFor(person, beat)
        );
    }

    private int LandingBeatFor(int person, int beat)
    {
        var source = _people[person];
        var elapsedPhases = source.TimeZone + source.Cells[beat].Height;
        return PositiveModulo(beat + elapsedPhases / PhaseCount, Period);
    }

    private string FormatCell(int person, int beat, PassingEditorCell cell)
    {
        var height = HeightLabel(cell.Height);
        return cell.TargetPerson is { } target && target != person
            ? $"{height}p{_people[target].Name}"
            : height;
    }

    public static string HeightLabel(int height) =>
        height < 10
            ? height.ToString(CultureInfo.InvariantCulture)
            : ((char)('a' + height - 10)).ToString();

    private static List<int> ParseHeights(string notation)
    {
        if (
            string.IsNullOrWhiteSpace(notation)
            || notation.Length > Siteswap.MaxPeriodLength
            || notation.Any(character => !char.IsAsciiLetterOrDigit(character))
        )
        {
            return [5, 3, 1];
        }

        return notation
            .Select(char.ToLowerInvariant)
            .Select(character =>
                character is >= '0' and <= '9' ? character - '0' : character - 'a' + 10
            )
            .ToList();
    }

    private static string DefaultName(int index) =>
        index == 0 ? "A" : ((char)('A' + index)).ToString();

    private static int DefaultHeight(int phaseCount, int maxThrowHeight) =>
        Math.Min(maxThrowHeight, phaseCount * 3);

    private static int PositiveModulo(int value, int modulus) =>
        (value % modulus + modulus) % modulus;
}

public sealed class PassingEditorPerson(string name, int timeZone, List<PassingEditorCell> cells)
{
    public string Name { get; } = name;

    public int TimeZone { get; internal set; } = timeZone;

    public IReadOnlyList<PassingEditorCell> Cells => cells;

    internal List<PassingEditorCell> MutableCells => cells;
}

public sealed class PassingEditorCell(int height, int? targetPerson)
{
    public int Height { get; internal set; } = height;

    public int? TargetPerson { get; internal set; } = targetPerson;
}

public readonly record struct PassingLandingSlot(int Person, int Beat);

public sealed record PassingTargetAdjustment(
    int SourcePerson,
    int SourceBeat,
    int? PreviousTargetPerson,
    int? TargetPerson,
    int LandingBeat
);

public sealed record PassingEditorLanding(
    int SourcePerson,
    int SourceBeat,
    int? TargetPerson,
    int TargetBeat,
    int TargetTimeZone,
    int Height,
    PassOrSelf Kind
);
