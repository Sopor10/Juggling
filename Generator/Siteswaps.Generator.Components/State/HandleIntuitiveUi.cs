namespace Siteswaps.Generator.Components.State;

static internal class HandleIntuitiveUi
{
    public static GeneratorState CreateState(SetStateFromIntuitiveUiAndGenerateSiteswaps action)
    {
        return new GeneratorState()
        {
            Objects = new Between
            {
                MaxNumber = action.Clubs.Last(),
                MinNumber = action.Clubs.First()
            },
            Period = action.Period,
            MaxThrow = action.Throws.Select(x => x.Height).Max(),
            MinThrow = action.Throws.Select(x => x.Height).Min(),
            NumberOfJugglers = action.NumberOfJugglers
        };
    }
}