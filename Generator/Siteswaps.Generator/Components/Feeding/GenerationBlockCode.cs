namespace Siteswaps.Generator.Components.Feeding;

/// <summary>
/// Stable, UI/i18n-friendly reason why a normal-feed session cannot generate yet.
/// </summary>
public enum GenerationBlockCode
{
    None = 0,
    NoPasses,
    IncompleteAssignments,
    SingleFedeeOnly,
    ClubsUnset,
}
