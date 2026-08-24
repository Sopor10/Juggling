using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.GenerationWorkflow;

/// <summary>
/// Optional locked inputs for a hosted generation workflow. Locked values are applied to
/// session state, hidden in the UI, and rejected if mutated. Clubs, throws, and extra
/// filters stay host-editable (unless <see cref="Clubs"/> seeds initial bounds).
/// </summary>
public sealed record GenerationWorkflowConfig
{
    public int? Period { get; init; }

    public int? NumberOfJugglers { get; init; }

    /// <summary>
    /// Optional Pass/Self pattern (Throw.AnyPass / Throw.AnySelf) injected as an immutable
    /// include-pattern filter. Requires <see cref="Period"/> with matching length.
    /// </summary>
    public IReadOnlyList<Throw>? PassSelfInterface { get; init; }

    /// <summary>
    /// Optional mask for throws made on each beat. It is evaluated at the same cyclic phase as
    /// <see cref="PassSelfInterface"/>.
    /// </summary>
    public IReadOnlyList<Throw>? ThrowInterface { get; init; }

    /// <summary>Optional initial club bounds applied on session create.</summary>
    public Between? Clubs { get; init; }

    public bool HasLockedPeriod => Period is not null;

    public bool HasLockedJugglers => NumberOfJugglers is not null;

    public bool HasLockedInterface => PassSelfInterface is { Count: > 0 };

    public bool Equals(GenerationWorkflowConfig? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Period == other.Period
            && NumberOfJugglers == other.NumberOfJugglers
            && Clubs == other.Clubs
            && InterfaceEqual(PassSelfInterface, other.PassSelfInterface)
            && InterfaceEqual(ThrowInterface, other.ThrowInterface);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Period);
        hash.Add(NumberOfJugglers);
        hash.Add(Clubs);
        if (PassSelfInterface is not null)
        {
            foreach (var item in PassSelfInterface)
            {
                hash.Add(item);
            }
        }

        if (ThrowInterface is not null)
        {
            foreach (var item in ThrowInterface)
            {
                hash.Add(item);
            }
        }

        return hash.ToHashCode();
    }

    private static bool InterfaceEqual(IReadOnlyList<Throw>? left, IReadOnlyList<Throw>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }
}
