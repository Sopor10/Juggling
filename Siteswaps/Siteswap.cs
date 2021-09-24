using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Siteswaps
{
    public record Siteswap
    {
        public static bool TryCreate(IEnumerable<int> items, [NotNullWhen(true)] out Siteswap? siteswap)
        {
            return TryCreate(new(items.ToArray()), out siteswap);
        }
        private Siteswap(CyclicArray<int> items)
        {
            Items = items;
        }

        public CyclicArray<int> Items { get; }

        public static bool TryCreate(CyclicArray<int> items, [NotNullWhen(true)]out Siteswap? siteswap)
        {
            if (IsValid(items))
            {
                siteswap = new(items);
                return true;
            }

            siteswap = null;
            return false;
        }

        private static bool IsValid(CyclicArray<int> items)
        {
            return items.Select((x, i) => (x + i) % items.Length).ToHashSet().Count == items.Length;
        }
        public bool IsGroundState() => HasNoRethrow();

        private bool HasNoRethrow() => !Items.Enumerate(1).Any(x => x.position + x.value < NumberOfObjects());

        public bool IsExcitedState() => !IsGroundState();
        public decimal NumberOfObjects() => (decimal)Items.Enumerate(1).Average(x => x.value);
        
    }
}