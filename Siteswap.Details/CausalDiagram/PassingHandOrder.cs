namespace Siteswap.Details.CausalDiagram;

/// <summary>
/// Builds Passist-style limb orders for asynchronous multi-hand siteswaps.
/// Mirrors <c>defaultLimbs</c> from https://github.com/helbling/passist (passist.mjs).
/// Two jugglers → Ar, Br, Al, Bl.
/// </summary>
public static class PassingHandOrder
{
    public static CyclicArray<Hand> Create(int numberOfJugglers)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(numberOfJugglers, 1);

        var limbs = new List<Hand>(numberOfJugglers * 2);
        for (var i = 0; i < 2 * numberOfJugglers; i++)
        {
            var jugglerIndex = i % numberOfJugglers;
            // Alternating R/L for an odd number of jugglers (Co Stuifbergen / passist).
            var isRight = numberOfJugglers % 2 != 0 ? i % 2 == 0 : i < numberOfJugglers;
            var person = new Person(((char)('A' + jugglerIndex)).ToString());
            limbs.Add(new Hand(isRight ? "R" : "L", person));
        }

        return limbs.ToCyclicArray();
    }
}
