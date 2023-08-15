namespace Siteswaps.Generator.Generator;

public record Throw(string Name, int Height, string DisplayValue)
{
    public static Throw AnySelf => new("Any S", -3, "Any S");
    public static Throw AnyPass => new("Any P", -2, "Any P");
    public static Throw Empty => new("Empty", -1, "_");
    public static Throw EmptyHand => new("0", 0, "0");
    public static Throw Zip => new("Zip", 2, "Zip");
    public static Throw Hold => new("Hold", 4, "Hold");
    public static Throw Zap => new("Zap", 5, "Zap");
    public static Throw Self => new("Self", 6, "Self");
    public static Throw SinglePass => new("Single", 7, "Single");
    public static Throw Heff => new("Heff", 8, "Heff");
    public static Throw DoublePass => new("Double", 9, "Double");
    public static Throw TripleSelf => new("Triple S", 10, "Triple S");
    public static Throw TriplePass => new("Triple", 11, "Triple");

    private bool IsPass => this.Height % 2 == 1;

    public static IEnumerable<Throw> All => new List<Throw>
    {
        EmptyHand,
        Zip,
        Hold,
        Zap,
        Self,
        SinglePass,
        Heff,
        DoublePass,
        TripleSelf,
        TriplePass
    };

    public static IEnumerable<Throw> Defaut =>
        new[]
        {
            EmptyHand,
            Zip,
            Zap,
            Self,
            SinglePass,
            Heff,
            DoublePass,
            TripleSelf
        };

    public IEnumerable<int> GetHeightForJugglers(int amountOfJugglers)
    {
        var result = new HashSet<int>();
        if (this.IsPass)
        {
            var min = this.Height - 1;
            var max = this.Height + 1;
            for (var i = min * amountOfJugglers + 1; i < max * amountOfJugglers; i++)
            {
                var item = i / 2;
                if (item % amountOfJugglers != 0) result.Add(item);
            }
        }
        else
        {
            result.Add(this.Height * amountOfJugglers / 2);
        }

        return result;
    }
    
    public static HashSet<int> PassValues(int minHeight, int maxHeight, int numberOfJugglers)=>Enumerable.Range(minHeight, maxHeight - minHeight + 1)
        .Where(x => x % numberOfJugglers != 0).ToHashSet();    
    
    public static HashSet<int> SelfValues(int minHeight, int maxHeight, int numberOfJugglers)=>Enumerable.Range(minHeight, maxHeight - minHeight + 1)
        .Where(x => x % numberOfJugglers == 0).ToHashSet();
}
