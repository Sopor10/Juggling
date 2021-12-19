using System.Collections.Generic;

namespace Siteswaps.Components.Generator.State;

public record GenerateSiteswapsAction(GeneratorState State);

public record PeriodChangedAction(int Value);

public record ExactNumberChangedAction(int Value);

public record MinNumberChangedAction(int Value);

public record MaxNumberChangedAction(int Value);

public record NumberOfJugglersChangedAction(int Value);

public record MinThrowChangedAction(int Value);

public record MaxThrowChangedAction(int Value);

public record RemoveFilterNumber(int Value);

public record ExactNumberOrRangeOfBallsSwitchedAction(bool Value);

public record NewFilterCreatedAction(IFilterInformation Value);

public record SiteswapsGeneratetAction(IReadOnlyCollection<Siteswap> Siteswaps);
public record FilterTypeSelectionChangedAction(FilterType FilterType);