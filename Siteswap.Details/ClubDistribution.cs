using System.Collections.Immutable;
using Siteswap.Details.StateDiagram;

namespace Siteswap.Details;

public record ClubDistribution(ImmutableList<(JugglerHand, int)> Hands)
{
    public static ClubDistribution FromSiteswap(Siteswap siteswap, int numberOfJugglers)
    {
        var numberOfObjects = (int)siteswap.NumberOfObjects;

        Transition getin =
            Enumerable
                .Range(0, siteswap.Length)
                .Select(i => siteswap.GetIns(i))
                .FirstOrDefault(ins => ins.Count > 0)
                ?.First()
            ?? throw new Exception("Could not find get-in sequence");

        var getinLength = getin.Throws.Length;
        var jugglerClubs = new (int left, int right)[numberOfJugglers];

        for (var juggler = 0; juggler < numberOfJugglers; juggler++)
        {
            var clubsForJuggler = numberOfObjects / numberOfJugglers;
            var jugglerPosition = (getinLength % numberOfJugglers + juggler) % numberOfJugglers;

            if (jugglerPosition < numberOfObjects % numberOfJugglers)
                clubsForJuggler++;

            var getinsForJuggler = getinLength / numberOfJugglers;
            if (getinLength % numberOfJugglers >= (numberOfJugglers - juggler))
                getinsForJuggler++;

            var isStartRight = (getinsForJuggler % 2) == 0;
            var right = isStartRight ? (clubsForJuggler + 1) / 2 : clubsForJuggler / 2;
            var left = isStartRight ? clubsForJuggler / 2 : (clubsForJuggler + 1) / 2;

            jugglerClubs[juggler] = (left, right);
        }

        for (var i = 0; i < getinLength; i++)
        {
            var throwValue = getin.Throws[i].Value;
            var throwingJuggler =
                (i + numberOfJugglers - getinLength % numberOfJugglers) % numberOfJugglers;

            var throwsLeftForJuggler = (getinLength - i) / numberOfJugglers;
            if (throwingJuggler >= numberOfJugglers - (getinLength - i) % numberOfJugglers)
                throwsLeftForJuggler++;

            var isRightHandThrowing = (throwsLeftForJuggler % 2) == 0;
            var catchingJuggler = (throwingJuggler + throwValue) % numberOfJugglers;
            var isRightHandCatching = ((throwingJuggler + throwValue) / numberOfJugglers) % 2 == 0;

            if (!isRightHandThrowing)
                isRightHandCatching = !isRightHandCatching;

            if (isRightHandThrowing)
                jugglerClubs[throwingJuggler].right--;
            else
                jugglerClubs[throwingJuggler].left--;

            if (isRightHandCatching)
                jugglerClubs[catchingJuggler].right++;
            else
                jugglerClubs[catchingJuggler].left++;
        }

        var hands = jugglerClubs.SelectMany(
            (c, i) =>
                new[] { (new JugglerHand(i, "L"), c.left), (new JugglerHand(i, "R"), c.right) }
        );

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
                return $"{(char)('A' + g.Key)}: {left}|{right}";
            })
        );
    }
}
