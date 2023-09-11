using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Components.State;

using Siteswap = Generator.Siteswap;

public record InterfaceFilterInformation(IEnumerable<Throw> Pattern, string From = "") : IFilterInformation
{
    public FilterType FilterType => FilterType.Interface;

    public string Display()
    {
        return "interface " + From + " " + string.Join(" ",
            Pattern.Select(Display).ToList());
    }

    private static string Display(Throw i)
    {
        return i.DisplayValue;
    }


    public static InterfaceFilterInformation CreateFrom(Siteswap siteswap, List<string> passerPassingSelection,
        string name, string passerName, bool nonPassesShouldBeSelf)
    {
        var passOrSelf = siteswap.ToPassOrSelf(2);
        var zippedList = passOrSelf.Zip(passerPassingSelection);

        var mappedToPassOrSelf = zippedList
            .Select(x => x.First == Throw.AnyPass && x.Second == name ? Throw.AnyPass : Throw.AnySelf).ToList();

        var rotatedThrows = RotateWithInterface(siteswap, mappedToPassOrSelf);
        if (nonPassesShouldBeSelf is false)
        {
            rotatedThrows = rotatedThrows.Replace(Throw.AnySelf, Throw.Empty);
        }

        return new InterfaceFilterInformation(rotatedThrows, passerName);
    }

    private static IEnumerable<T> RotateWithInterface<T>(Siteswap siteswap, List<T> other)
    {
        return siteswap.RotateWithInterface(other);
    }
}

public static class IEnumerableExtensions
{
    public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T value, T replace)
    {

        foreach (var t in source)
        {
            if (t is null && value is null)
            {
                yield return replace;
                continue;
            }

            if (t is null)
            {
                yield return value;
                continue;
            }
            
            if (t.Equals(value))
            {
                yield return replace;
                continue;
            }

            yield return t;
        }
    }
}
