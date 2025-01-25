using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Components.State;

public record GenerateSiteswapsAction(
    GeneratorState State,
    CancellationTokenSource CancellationTokenSource
);

public record PeriodChangedAction(Period Value);

public record ExactNumberChangedAction(int? Value);

public record MinNumberChangedAction(int? Value);

public record MaxNumberChangedAction(int? Value);

public record NumberOfJugglersChangedAction(int? Value);

public record MinThrowChangedAction(int? Value);

public record MaxThrowChangedAction(int? Value);

public record RemoveFilterNumber(int Value);

public record ExactNumberOrRangeOfBallsSwitchedAction(bool Value);

public record NewFilterCreatedAction(IFilterInformation Value);

public record ChangedFilterAction(IFilterInformation NewPatternFilterInformation, int FilterNumber);

public record SiteswapGeneratedAction(params IEnumerable<Siteswap> Siteswaps);

public record NavigatedAwayFromSiteswapResultListAction;

public record SetState(GeneratorState State);

public record ThrowsChangedAction(IEnumerable<Throw> Throws);

public record CreateFilterFromThrowList(bool Value);
public record LoadSettings;
public record SettingsLoadedAction(Settings.SettingsDto Settings);
