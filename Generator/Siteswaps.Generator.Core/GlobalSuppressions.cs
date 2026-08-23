using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Design",
    "CA1822",
    Justification = "Stateless generator helpers are intentionally retained as instance API."
)]
[assembly: SuppressMessage(
    "Design",
    "CA1852",
    Justification = "Public generator API types remain extensible."
)]
[assembly: SuppressMessage(
    "Design",
    "CA1816",
    Justification = "Enumerator disposal follows the existing API contract."
)]
[assembly: SuppressMessage(
    "Globalization",
    "CA1305",
    Justification = "Siteswap formatting is domain-specific and culture-independent."
)]
[assembly: SuppressMessage(
    "Globalization",
    "CA1309",
    Justification = "Siteswap equality is intentionally based on domain notation."
)]
[assembly: SuppressMessage(
    "Naming",
    "CA1716",
    Justification = "Filter and siteswap terminology is established public domain vocabulary."
)]
[assembly: SuppressMessage(
    "Performance",
    "CA1859",
    Justification = "Public filter APIs intentionally expose interface abstractions."
)]
[assembly: SuppressMessage(
    "Design",
    "CA1068",
    Justification = "Backtracking parameter order is part of the existing private algorithm contract."
)]
[assembly: SuppressMessage(
    "Usage",
    "CA2208",
    Justification = "Validation exceptions describe domain values rather than method parameters."
)]
[assembly: SuppressMessage(
    "Performance",
    "CA1860",
    Justification = "Sequence APIs intentionally support general enumerables."
)]
