namespace Siteswaps.Components.Feeding;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Generator.Components.State;
using Generator.Generator;
using Shared;

[DebuggerDisplay("{Name} in {TimeZone}")]
public class Juggler
{
    public Juggler(string name, string timeZone, List<string> passesWith)
    {
        this.Name = name;
        this.TimeZone = timeZone;
        this.PassesWith = passesWith;
    }

    public string Name { get; set; }
    public string TimeZone { get; set; }
    public List<string> PassesWith { get; }
    public IEnumerable<int> Clubs { get; set; } = new[] {6, 6};
    public ImmutableList<IFilterInformation> VisibleFilter { get; set; } = ImmutableList<IFilterInformation>.Empty;

    public ImmutableList<IFilterInformation> GenerationFilter
    {
        get
        {
            var filterInformations = this.VisibleFilter
                .Where(x => x is not InterfaceFilterInformation)
                .ToImmutableList();
            var interfaceFilterInformation = this.CombineInterfaceFilterInformations();
            if (interfaceFilterInformation is null)
            {
                return filterInformations;
            }
            return filterInformations
                .Add(interfaceFilterInformation);
        }
    }

    public NewPatternFilterInformation CurrentFilter { get; set; } = new(Enumerable.Empty<Throw>(), true);
    
    private Siteswap? selectedSiteswap { get; set; }

    public Siteswap? SelectedSiteswap
    {
        get => this.selectedSiteswap;
        set
        {
            this.selectedSiteswap = value;
            if (value is not null)
            {
                this.PassingSelection = Enumerable.Repeat(string.Empty, value.Values.Length).ToList();
            }
 
        }
    }

    public IEnumerable<Throw> Throws { get; set; } = Throw.Defaut.ToList();
    public List<string> PassingSelection { get; set; } = new();

    public void EditFilter(int i)
    {
        this.VisibleFilter = this.VisibleFilter.SetItem(i, this.CurrentFilter);
        this.CurrentFilter = new NewPatternFilterInformation(Enumerable.Empty<Throw>(), true);
    }

    public void AddFilter()
    {
        this.VisibleFilter = this.VisibleFilter.Add(this.CurrentFilter);
        this.CurrentFilter = new NewPatternFilterInformation(Enumerable.Empty<Throw>(), true);
    }

    public void SetGlobalPattern(bool b)
    {
        this.CurrentFilter = this.CurrentFilter with{ IsGlobalPattern = b};
    }

    public void ChangeThrows(List<Throw> list)
    {
        this.CurrentFilter = this.CurrentFilter with { Pattern = list };
    }


    public InterfaceFilterInformation? CombineInterfaceFilterInformations()
    {
        return this.CombineInterfaceFilterInformations(this.VisibleFilter.OfType<InterfaceFilterInformation>());
    }

    private InterfaceFilterInformation? CombineInterfaceFilterInformations(IEnumerable<InterfaceFilterInformation> interfaceFilterInformations)
    {
        interfaceFilterInformations = interfaceFilterInformations.ToList();
        if (interfaceFilterInformations.Any() is false)
        {
            return null;
        }

        var pattern = new List<Throw>();
        for (var i = 0; i < interfaceFilterInformations.First().Pattern.Count(); i++)
        {
            pattern.Add(interfaceFilterInformations.Any(x => x.Pattern.ToList()[i] == Throw.AnyPass) ? Throw.AnyPass : Throw.AnySelf);
        }

        return new InterfaceFilterInformation(pattern);
    }

    private IEnumerable<IFilterInformation> CreateFilterFromFeedingKnowledge(IEnumerable<Juggler> others)
    {
        foreach (var passer in others.Where(x => x.PassesWith.Contains(this.Name)))
        {
            if (passer.SelectedSiteswap is null || passer.PassingSelection.Contains(this.Name) is false)
            {
                continue;
            }
            var passOrSelf = ToPassOrSelf(Interface.FromSiteswap(passer.SelectedSiteswap).Values);
            var zippedList = passOrSelf.Zip(passer.PassingSelection);

            var mappedToPattern = zippedList.Select(x => x.First == InterfaceSplitting.PassOrSelf.Pass && x.Second == this.Name ? Throw.AnyPass : Throw.AnySelf).ToList();
            yield return new InterfaceFilterInformation(mappedToPattern, passer.Name);
        }
    }

    public void UpdateFeedingFilter(IEnumerable<Juggler> others)
    {
        this.VisibleFilter = this.CreateFilterFromFeedingKnowledge(others).ToImmutableList().AddRange(this.VisibleFilter.Where(x => x is not InterfaceFilterInformation));
    }
    
    private static List<InterfaceSplitting.PassOrSelf> ToPassOrSelf(CyclicArray<sbyte> values)
    {
        return values.Enumerate(1).Select(x => x.value % 2 == 0 ? InterfaceSplitting.PassOrSelf.Self : InterfaceSplitting.PassOrSelf.Pass).ToList();
    }

    public List<InterfaceSplitting.PassOrSelf> InterfaceAsPassOrSelf()
    {
        if (this.SelectedSiteswap is null)
        {
            return new List<InterfaceSplitting.PassOrSelf>();
        }
        return ToPassOrSelf(Interface.FromSiteswap(this.SelectedSiteswap).Values);
    }
}
