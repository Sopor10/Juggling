using Dunet;
using Siteswap.Details.Jif;

namespace Siteswaps.Jif.Components;

/// <summary>Accepts either a typed JIF document or its JSON representation.</summary>
[Union]
public partial record JifInput
{
    /// <summary>A deserialized JIF document.</summary>
    public partial record Structured(JifPattern Value);

    /// <summary>Raw JIF JSON.</summary>
    public partial record Json(string Value);
}
