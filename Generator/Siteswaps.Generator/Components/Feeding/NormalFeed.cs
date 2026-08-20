namespace Siteswaps.Generator.Components.Feeding;

public enum PassOrSelf
{
    Pass,
    Self,
}

public sealed record FeedJuggler(string Name, int TimeZone, IReadOnlyList<string> PassingPartners);

/// <summary>
/// Fixed normal-feed topology: feeder A on time layer 0, B1/B2 on time layer 1.
/// </summary>
public sealed class NormalFeed
{
    private NormalFeed(FeedJuggler a, FeedJuggler b1, FeedJuggler b2)
    {
        A = a;
        B1 = b1;
        B2 = b2;
    }

    public FeedJuggler A { get; }
    public FeedJuggler B1 { get; }
    public FeedJuggler B2 { get; }

    public IEnumerable<FeedJuggler> Jugglers
    {
        get
        {
            yield return A;
            yield return B1;
            yield return B2;
        }
    }

    public FeedJuggler this[string name] =>
        name switch
        {
            "A" => A,
            "B1" => B1,
            "B2" => B2,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown feed role."),
        };

    public static NormalFeed Create() =>
        new(
            new FeedJuggler("A", 0, ["B1", "B2"]),
            new FeedJuggler("B1", 1, ["A"]),
            new FeedJuggler("B2", 1, ["A"])
        );
}
