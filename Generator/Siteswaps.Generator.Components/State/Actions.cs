using Siteswaps.Generator.Api;
using Siteswaps.Generator.Components.Internal;

namespace Siteswaps.Generator.Components.State;

public record GenerateSiteswapsAction(GeneratorState State);
public record PeriodChangedAction(int? Value);
public record ExactNumberChangedAction(int? Value);
public record MinNumberChangedAction(int? Value);
public record MaxNumberChangedAction(int? Value);

public record NumberOfJugglersChangedAction(int? Value);

public record MinThrowChangedAction(int? Value);

public record MaxThrowChangedAction(int? Value);

public record RemoveFilterNumber(int Value);

public record ExactNumberOrRangeOfBallsSwitchedAction(bool Value);

public record NewFilterCreatedAction(IFilterInformation Value);

public record SiteswapsGeneratedAction(IReadOnlyCollection<ISiteswap> Siteswaps);
public record FilterTypeSelectionChangedAction(FilterType FilterType);
public record PatternFilterValueChangedAction(int Pos, int Value);
public record SetStateFromIntuitiveUiAndGenerateSiteswaps(IEnumerable<int> Clubs, int Period, IEnumerable<NewUI.Throw> Throws, int NumberOfJugglers);
public record SetState(GeneratorState State);
