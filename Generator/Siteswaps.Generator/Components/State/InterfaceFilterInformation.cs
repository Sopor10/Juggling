namespace Siteswaps.Generator.Components.State;

using Generator;
using Shared;

public record InterfaceFilterInformation(IEnumerable<Throw> Pattern, string From = "") : IFilterInformation
{
    public FilterType FilterType => FilterType.Interface;

    public string Display() => "interface " + this.From + " " + string.Join(" ", Pattern.Reverse().SkipWhile(x => x == Throw.Empty).Reverse().Select(Display).ToList());

    private static string Display(Throw i) => i.DisplayValue;


    public static InterfaceFilterInformation CreateFrom(Siteswap siteswap, List<string> passerPassingSelection, string name, string passerName)
    {
        var passOrSelf = siteswap.ToPassOrSelf(2);
        var zippedList = passOrSelf.Zip(passerPassingSelection);

        var mappedToPassOrSelf = zippedList.Select(x => x.First == Throw.AnyPass && x.Second == name ? Throw.AnyPass : Throw.AnySelf).ToList();
        
        return new InterfaceFilterInformation(RotateWithInterface(siteswap, mappedToPassOrSelf), passerName);
    }
    
    private static IEnumerable<T> RotateWithInterface<T>(Siteswap siteswap, List<T> other)
    {
        return siteswap.RotateWithInterface(other);
    }
}
