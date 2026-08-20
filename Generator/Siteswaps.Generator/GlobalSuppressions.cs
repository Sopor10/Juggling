using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1716",
    Justification = "Throw is established juggling domain vocabulary in this UI model.",
    Scope = "type",
    Target = "~T:Siteswaps.Generator.Components.State.Throw"
)]
[assembly: SuppressMessage(
    "Performance",
    "CA1822",
    Justification = "CreateFilterFromThrowList is part of the Fluxor/state record surface and must remain an instance member.",
    Scope = "member",
    Target = "~P:Siteswaps.Generator.Components.State.GeneratorState.CreateFilterFromThrowList"
)]
[assembly: SuppressMessage(
    "Performance",
    "CA1859",
    Justification = "Filter builders and visited-step sets are intentionally typed as abstractions for the wizard API.",
    Scope = "namespaceanddescendants",
    Target = "~N:Siteswaps.Generator.Components.WizardPage"
)]
