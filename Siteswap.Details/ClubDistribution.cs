using System.Collections.Immutable;

namespace Siteswap.Details;

public record ClubDistribution(ImmutableList<(JugglerHand, int)> Hands)
{
    public static ClubDistribution FromSiteswap(Siteswap siteswap, int numberOfJugglers)
    {
        return CalculateGroundStateClubDistribution(siteswap, numberOfJugglers);
    }

    public static ClubDistribution CalculateGroundStateClubDistribution(
        Siteswap siteswap,
        int numberOfJugglers
    )
    {
        var hands = new List<(JugglerHand, int)>();
        var numberOfObjects = (int)siteswap.NumberOfObjects;

        for (var juggler = 0; juggler < numberOfJugglers; juggler++)
        {
            var numberOfClubsForJuggler = numberOfObjects / numberOfJugglers;
            if (juggler < numberOfObjects % numberOfJugglers)
                numberOfClubsForJuggler++;

            var numberOfClubsRightHand = numberOfClubsForJuggler / 2 + numberOfClubsForJuggler % 2;
            var numberOfClubsLeftHand = numberOfClubsForJuggler / 2;

            hands.Add((new JugglerHand(juggler, "L"), numberOfClubsLeftHand));
            hands.Add((new JugglerHand(juggler, "R"), numberOfClubsRightHand));
        }

        return new ClubDistribution(hands.ToImmutableList());
    }

    public override string ToString()
    {
        var grouped = Hands.GroupBy(x => x.Item1.Juggler);
        return string.Join(
            " ",
            grouped.Select(g =>
            {
                var left = g.First(x => x.Item1.Name == "L").Item2;
                var right = g.First(x => x.Item1.Name == "R").Item2;
                return $"{g.Key}: {left}|{right}";
            })
        );
    }
}
