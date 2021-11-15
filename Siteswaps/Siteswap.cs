using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Siteswaps
{
    public record Siteswap
    {
        public CyclicArray<int> Items { get; }

        public static bool TryCreate(IEnumerable<int> items, [NotNullWhen(true)] out Siteswap? siteswap)
        {
            return TryCreate(new(items), out siteswap);
        }
        private Siteswap(CyclicArray<int> items)
        {
            Items = ToUniqueRepresentation(items);
        }

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

        private static bool IsValid(CyclicArray<int> items) =>
            items
                .Enumerate(1)
                .Select(x => x.value)
                .Select((x, i) => (x + i) % items.Length)
                .ToHashSet()
                .Count == items.Length;

        private static CyclicArray<int> ToUniqueRepresentation(CyclicArray<int> input)
        {

            var result = Enumerable.Range(0, input.Length).Select(x =>
            {
                var cyclicArray = input.Rotate(x);
                return new
                {
                 Array = cyclicArray,
                 Rank = Rank(cyclicArray)
                };
                
            });

            return result.MaxBy(x => x.Rank)?.Array??throw new ArgumentNullException();
        }

        private static int Rank(CyclicArray<int> input)
        {
            var values = input
                .Enumerate(1)
                .Select(x => x.value)
                .Reverse();

            var pos = 0;
            var res = 0;
            foreach (var value in values)
            {
                res += (int)Math.Pow(10, pos) * value;
                pos++;
            }

            return res;
        }

        public bool IsGroundState() => HasNoRethrow();

        private bool HasNoRethrow() => !Items.Enumerate(1).Any(x => x.position + x.value < NumberOfObjects());

        public bool IsExcitedState() => !IsGroundState();
        public decimal NumberOfObjects() => (decimal)Items.Enumerate(1).Average(x => x.value);

        public override string ToString()
        {
            return string.Join("", Items.EnumerateValues(1).Select(x => Transform(x)));
        }

        private string Transform(int i)
        {
            return i switch
            {
                < 10 => $"{i}",
                _ => Convert.ToChar(i + 87).ToString()
            };
        }
    }
}