using System.Diagnostics;

namespace Siteswaps.Generator.Components.State;

[DebuggerDisplay("{Display}")]
public record PatternRotation(int Value)
{
    public static PatternRotation Global => new(-2);
    public static PatternRotation Local => new(-1);
    public static PatternRotation A => new(0);
    public static PatternRotation B => new(1);

    public string Display =>
        Value switch
        {
            -2 => "global",
            -1 => "local",
            _ => ((char)('A' + Value)).ToString(),
        };
}
