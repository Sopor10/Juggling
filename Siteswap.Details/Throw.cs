using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Siteswap.Details.StateDiagram;

namespace Siteswap.Details;

[DebuggerDisplay("{PrettyPrint()}")]
[SuppressMessage("Naming", "CA1716", Justification = "Throw is established domain vocabulary.")]
public record Throw(State StartingState, State EndingState, int Value)
{
    public string PrettyPrint()
    {
        return $"{StartingState} -{Value.ToSiteswapString()}> {EndingState} : {EndingStateCalc}";
    }

    private State EndingStateCalc => StartingState.Advance().Throw(Value);
}
