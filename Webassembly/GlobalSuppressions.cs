using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1716",
    Justification = "Webassembly.Shared is the established Blazor shared-components namespace; renaming would churn all UI imports.",
    Scope = "namespace",
    Target = "~N:Webassembly.Shared"
)]
