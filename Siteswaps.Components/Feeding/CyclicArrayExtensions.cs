namespace Siteswaps.Components.Feeding;

using System.Collections.Generic;
using System.Linq;
using Shared;

public static class CyclicArrayExtensions
{
    public static List<InterfaceSplitting.PassOrSelf> ToPassOrSelf(this CyclicArray<sbyte> values)
    {
        return values.Enumerate(1).Select(x => x.value % 2 == 0 ? InterfaceSplitting.PassOrSelf.Self : InterfaceSplitting.PassOrSelf.Pass).ToList();
    }
}