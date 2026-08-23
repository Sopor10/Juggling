using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Design",
    "CA1822",
    Justification = "Stateless diagram and state-graph helpers retain their public service API."
)]
[assembly: SuppressMessage(
    "Design",
    "CA1816",
    Justification = "Enumerator disposal follows the existing API contract."
)]
[assembly: SuppressMessage(
    "Globalization",
    "CA1305",
    Justification = "Siteswap formatting is culture-independent domain notation."
)]
[assembly: SuppressMessage(
    "Globalization",
    "CA1309",
    Justification = "Siteswap equality uses domain notation deliberately."
)]
[assembly: SuppressMessage(
    "Naming",
    "CA1716",
    Justification = "Throw and interface are established domain vocabulary."
)]
[assembly: SuppressMessage(
    "Performance",
    "CA1859",
    Justification = "Public state APIs intentionally expose abstractions."
)]
