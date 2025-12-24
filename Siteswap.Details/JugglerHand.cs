namespace Siteswap.Details;

public record JugglerHand(int Juggler, string Name)
{
    public override string ToString()
    {
        return $"{'A' + Juggler}{Name}";
    }
}