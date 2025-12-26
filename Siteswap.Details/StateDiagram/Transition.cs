using System.Diagnostics;

namespace Siteswap.Details.StateDiagram;

[DebuggerDisplay("{PrettyPrint()}")]
public record Transition(Siteswap From, Siteswap To, Throw[] Throws)
{
    public virtual bool Equals(Transition? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return From.Equals(other.From) && To.Equals(other.To) && Throws.SequenceEqual(other.Throws);
    }

    public override int GetHashCode()
    {
        var t = 397 * (Throws.Aggregate(0, (old, curr) => (old * 397) ^ curr.GetHashCode()));
        return HashCode.Combine(From, To, t);
    }

    public string PrettyPrint()
    {
        return $"{From} -{Throws.Aggregate("", (s, i) => s + Siteswap.Transform(i.Value))}-> {To}";
    }

    /// <summary>
    /// True if the transition is minimal (does not visit any state more than once, including the start and end state)
    /// False otherwise
    /// </summary>
    public bool IsMinimal
    {
        get
        {
            if (Throws.Length == 0)
            {
                return true;
            }

            var transitionStates = Throws
                .Select(x => x.StartingState)
                .Append(Throws.Last().EndingState)
                .ToHashSet();
            var noLoopsInTheTransition = transitionStates.Count == Throws.Length + 1;

            return noLoopsInTheTransition;
        }
    }
}
