using System.Collections.Immutable;

namespace Siteswap.Details;

/// <summary>
///     An interface is the order of catches of a siteswap e.g. 53 will be 35
/// </summary>
/// <param name="Items"></param>
public record Interface(ImmutableList<int> Items)
{
    public override string ToString()
    {
        return string.Join("", Items.Select(Siteswap.Transform));
    }

    public ImmutableList<PassOrSelf> GetPassOrSelf(int numberOfJuggler) =>
        Items
            .Select(x => x % numberOfJuggler == 0 ? PassOrSelf.Self : PassOrSelf.Pass)
            .ToImmutableList();

    public string AsPassOrSelf(int numberOfJugglers) =>
        string.Join(
            "",
            GetPassOrSelf(numberOfJugglers).Select(x => x == PassOrSelf.Pass ? "p" : "s")
        );
}
