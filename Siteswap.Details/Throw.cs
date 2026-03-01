using System.Diagnostics;
using Siteswap.Details.StateDiagram;

namespace Siteswap.Details;

[DebuggerDisplay("{PrettyPrint()}")]
public record Throw(State StartingState, State EndingState, int Value)
{
    public string PrettyPrint()
    {
        return $"{StartingState} -{Value.ToSiteswapString()}> {EndingState} : {EndingStateCalc}";
    }

    private State EndingStateCalc => StartingState.Advance().Throw(Value);
}
