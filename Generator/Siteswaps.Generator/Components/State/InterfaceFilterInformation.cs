using System.Diagnostics;

namespace Siteswaps.Generator.Components.State;

/// <summary>
/// Landing constraint: slot <c>j</c> describes the throw that lands on beat <c>j</c>.
/// Unlike <see cref="NewPatternFilterInformation"/>, which describes the throw made on beat
/// <c>j</c>, this matches the siteswap's interface.
/// <para>
/// <see cref="Throws"/> optionally adds a mask on the made throws, evaluated at the same phase
/// as <see cref="Landing"/>. Feeding uses it to keep a fedee off the feeder's beats that the
/// other fedee's chosen pattern already occupies.
/// </para>
/// </summary>
[DebuggerDisplay("{Display()}")]
public record InterfaceFilterInformation(
    List<Throw> Landing,
    bool AllowRotation,
    List<Throw>? Throws = null
) : IFilterInformation
{
    public List<Throw> Landing { get; set; } = Landing;
    public bool AllowRotation { get; set; } = AllowRotation;
    public List<Throw>? Throws { get; set; } = Throws;

    public string Display() =>
        "interface "
        + (AllowRotation ? "any rotation " : "fixed beats ")
        + string.Join(",", Landing.Select(x => x.DisplayValue))
        + (
            Throws is { Count: > 0 }
                ? " throws " + string.Join(",", Throws.Select(x => x.DisplayValue))
                : string.Empty
        );
}
