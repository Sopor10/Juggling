using System.Diagnostics;

namespace Siteswaps.Generator.Core.Generator.Filter;

/// <summary>
/// Matches a flexible Pass/Self/height mask against the landing interface: slot <c>j</c>
/// constrains the throw that <em>lands</em> on beat <c>j</c>. The pattern filters constrain the
/// throw <em>made</em> on beat <c>j</c> instead, which is a different sequence of the same throws.
/// <para>
/// An optional throw mask constrains the made throws in addition. Both masks are rotated in
/// lockstep, so a candidate has to satisfy them at the <em>same</em> phase — which two separate
/// rotation-flexible filters could not guarantee.
/// </para>
/// </summary>
public class InterfaceFilter : ISiteswapFilter
{
    private const int EmptySlot = -1;
    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;

    private readonly List<InterfaceMask> _acceptedMasks;

    public InterfaceFilter(
        List<List<int>> landingInterface,
        int numberOfJugglers,
        SiteswapGeneratorInput input,
        bool allowRotation,
        List<List<int>>? throwPattern = null
    )
    {
        if (numberOfJugglers < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numberOfJugglers),
                numberOfJugglers,
                "At least one juggler is required to tell passes from selfs."
            );
        }

        var interfaceBeats = PadToPeriod(landingInterface, input.Period);
        var throwSlots = throwPattern is null ? null : PadToPeriod(throwPattern, input.Period);

        _acceptedMasks = allowRotation
            ? Enumerable
                .Range(0, input.Period)
                .Select(rotation => new InterfaceMask(
                    interfaceBeats.Rotate(rotation),
                    throwSlots?.Rotate(rotation),
                    numberOfJugglers
                ))
                .ToList()
            : [new InterfaceMask(interfaceBeats, throwSlots, numberOfJugglers)];
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        foreach (var mask in _acceptedMasks)
        {
            if (mask.IsStillPossible(value))
            {
                return true;
            }
        }

        return false;
    }

    public int Order => 1;

    private static List<List<int>> PadToPeriod(List<List<int>> slots, int period)
    {
        var padded = Enumerable.Repeat(new List<int> { DontCare }, period).ToList();
        for (var i = 0; i < slots.Count && i < period; i++)
        {
            padded[i] = slots[i];
        }

        return padded;
    }

    [DebuggerDisplay("{DebugDisplay}")]
    private sealed record InterfaceMask(
        List<List<int>> InterfaceBeats,
        List<List<int>>? ThrowSlots,
        int NumberOfJugglers
    )
    {
        private string DebugDisplay =>
            "interface "
            + Format(InterfaceBeats)
            + (ThrowSlots is null ? "" : " throws " + Format(ThrowSlots));

        private static string Format(List<List<int>> slots) =>
            string.Join(" ", slots.Select(slot => "{" + string.Join(",", slot) + "}"));

        /// <summary>
        /// Interface beats are filled as soon as their throw is placed and stay fixed until the
        /// generator backtracks, so a partial siteswap can already be rejected here.
        /// </summary>
        public bool IsStillPossible(PartialSiteswap value)
        {
            for (var i = 0; i < InterfaceBeats.Count; i++)
            {
                var height = value.Interface[i];
                if (height == EmptySlot)
                {
                    continue;
                }

                if (!Accepts(InterfaceBeats[i], height))
                {
                    return false;
                }
            }

            if (ThrowSlots is null)
            {
                return true;
            }

            for (var i = 0; i < ThrowSlots.Count; i++)
            {
                var height = value.Items[i];
                if (height == EmptySlot)
                {
                    continue;
                }

                if (!Accepts(ThrowSlots[i], height))
                {
                    return false;
                }
            }

            return true;
        }

        private bool Accepts(List<int> slot, int height)
        {
            foreach (var expected in slot)
            {
                var accepted = expected switch
                {
                    DontCare => true,
                    Pass => height % NumberOfJugglers != 0,
                    Self => height % NumberOfJugglers == 0,
                    _ => height == expected,
                };

                if (accepted)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
