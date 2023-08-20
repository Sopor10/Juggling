namespace Siteswaps.Components.Feeding;

using System.Collections.Generic;
using System.Collections.Immutable;

public record FeedingPattern(ImmutableList<Juggler> Jugglers)
{
    public void UpdateFeedingFilter()
    {
        foreach (var juggler in Jugglers)
        {
            juggler.UpdateFeedingFilter(Jugglers);
        }
    }
    
    public static FeedingPattern NormalFeed()
    {
        return new (new List<Juggler>()
        {
            new("A", 0, new List<string>() {"B1", "B2"}),
            new("B1", 1, new List<string>() {"A"}),
            new("B2", 1, new List<string>() {"A"})
        }.ToImmutableList());
    }

    public static FeedingPattern N_Feed()
    {
        return new (new List<Juggler>()
        {
            new("A1", 0, new List<string>() {"B1", "B2"}),
            new("A2", 0, new List<string>() {"B1"}),
            new("B1", 1, new List<string>() {"A1", "A2"}),
            new("B2", 1, new List<string>() {"A1"})
        }.ToImmutableList());
    }

    public static FeedingPattern W_Feed()
    {
        return new (new List<Juggler>()
        {
            new("A1", 0, new List<string>() {"B1", "B2"}),
            new("B1", 1, new List<string>() {"A1", "A2"}),
            new("B2", 1, new List<string>() {"A1", "A3"}),
            new("A2", 0, new List<string>() {"B1"}),
            new("A3", 0, new List<string>() {"B2"})
        }.ToImmutableList());
    }
}
