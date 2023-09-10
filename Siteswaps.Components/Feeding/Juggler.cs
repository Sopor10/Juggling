namespace Siteswaps.Components.Feeding;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Generator.Components.State;
using Generator.Generator;

[DebuggerDisplay("{Name} in {TimeZone}")]
public class Juggler
{
    public Juggler(string name, int timeZone, List<string> passingPartners)
    {
        this.Name = name;
        this.TimeZone = timeZone;
        this.PassingPartners = passingPartners;
    }

    public string Name { get; set; }
    public int TimeZone { get; set; }
    public List<string> PassingPartners { get; }
    public IEnumerable<int> Clubs { get; set; } = new[] {6, 6};
    public ImmutableList<IFilterInformation> VisibleFilter { get; set; } = ImmutableList<IFilterInformation>.Empty;

    public ImmutableList<IFilterInformation> GetGenerationFilter(bool allPassingPartnersHaveSiteswapSelected)
    {
        var filterInformations = this.VisibleFilter
            .Where(x => x is not InterfaceFilterInformation)
            .ToImmutableList();
        var interfaceFilterInformation = this.CombineInterfaceFilterInformations(allPassingPartnersHaveSiteswapSelected);
        if (interfaceFilterInformation is null)
        {
            return filterInformations;
        }

        return filterInformations
            .Add(interfaceFilterInformation);
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
                this.PassingTargets = Enumerable.Repeat(string.Empty, value.Values.Length).ToList();
            }
 
        }
    }

    public IEnumerable<Throw> Throws { get; set; } = Throw.Defaut.ToList();
    public List<string> PassingTargets { get; set; } = new();

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


    public InterfaceFilterInformation? CombineInterfaceFilterInformations(bool allPassingPartnersHaveSiteswapSelected)
    {
        return this.CombineInterfaceFilterInformations(this.VisibleFilter.OfType<InterfaceFilterInformation>(), allPassingPartnersHaveSiteswapSelected);
    }

    public List<InterfaceSplitting.PassOrSelf> InterfaceAsPassOrSelf()
    {
        if (this.SelectedSiteswap is null)
        {
            return new List<InterfaceSplitting.PassOrSelf>();
        }
        return this.SelectedSiteswap.Interface.Values.ToPassOrSelf();
    }

    private InterfaceFilterInformation? CombineInterfaceFilterInformations(
        IEnumerable<InterfaceFilterInformation> interfaceFilterInformations,
        bool allPassingPartnersHaveSiteswapSelected)
    {
        var fillerThrow = allPassingPartnersHaveSiteswapSelected ? Throw.AnySelf : Throw.Empty;
        interfaceFilterInformations = interfaceFilterInformations.ToList();
        if (interfaceFilterInformations.Any() is false)
        {
            return null;
        }

        var pattern = new List<Throw>();
        for (var i = 0; i < interfaceFilterInformations.First().Pattern.Count(); i++)
        {
            pattern.Add(interfaceFilterInformations.Any(x => x.Pattern.ToList()[i] == Throw.AnyPass) ? Throw.AnyPass : fillerThrow);
        }

        return new InterfaceFilterInformation(pattern);
    }

    public void UpdateFeedingFilter(FeedingPattern pattern)
    {
        this.VisibleFilter = this.CreateFilterFromFeedingKnowledge(pattern).ToImmutableList().AddRange(this.VisibleFilter.Where(x => x is not InterfaceFilterInformation));
    }

    private IEnumerable<IFilterInformation> CreateFilterFromFeedingKnowledge(FeedingPattern pattern)
    {
        foreach (var passer in pattern.Jugglers.Where(x => x.PassingPartners.Contains(this.Name)))
        {
            if (passer.SelectedSiteswap is null || passer.PassingTargets.Contains(this.Name) is false)
            {
                continue;
            }
            
            yield return InterfaceFilterInformation.CreateFrom(passer.SelectedSiteswap, passer.PassingTargets,this.Name, passer.Name, pattern.PassingPartnerHaveSiteswapSelected(this));
        }
    }

    public Siteswap RotateCorrectly(Siteswap siteswap)
    {
        var pattern = this.VisibleFilter.OfType<InterfaceFilterInformation>().FirstOrDefault()?.Pattern;
        if (pattern is not null)
        {
            siteswap = siteswap.RotateToMatchInterface(Pattern.FromThrows(pattern, 2))?? throw new ArgumentException("Siteswap is not valid at this point!");
        }
        return siteswap;
    }
}
