# Highgate manipulation-class inventory audit (2017 pages 62–208)

This audit covers the 22 inventory rows currently marked open with
`manipulation_class_2017=manipulation`. It visually checks the relevant 2017
expanded-book diagrams and both-edition matches in the 2014 book where the
same material is present. The 2017 page numbers below are printed page / PDF
page; the 2014 page number is the printed page and PDF page.

The individual periodic patterns transcribed in this pass are kept in
[highgate-manipulation-standalone-records.json](highgate-manipulation-standalone-records.json).
A focused NUnit test validates those records and checks every edge against the
source throw sequence and its stated landing time. No runtime catalog or
inventory file was edited.

## Entry-by-entry audit

| Inventory ID | Printed source reviewed | Classification / disposition |
|---|---|---|
| `highgate-78956` | 2017 100 / PDF 101; 2014 64 | Individual steady-state graph transcribed; 20 nodes and 20 edges. Zap is a 2.5-beat half-spin pass; straight/crossing route is retained. |
| `highgate-79685` | 2017 100 / PDF 101; 2014 64 | Separate steady-state graph transcribed; 20 nodes and 20 edges. |
| `highgate-97586` | 2017 100 / PDF 101; 2014 64 | Separate steady-state graph transcribed; 20 nodes and 20 edges. |
| `highgate-fiveclubwhynotvsfivecountpopcorn` | 2017 63 / PDF 64; 2014 34 | Individual two-juggler steady state; transcribed. Not a manipulation transition. |
| `highgate-fivecountpopcornvsfiveclubwhynot` | 2017 63 / PDF 64; 2014 34 | Reverse-role steady state; distinct diagram/graph from the preceding entry. Transcribed. |
| `highgate-funkybookendsfeedingfivecountpopcornandwhynot` | 2017 208 / PDF 209; 2014 146 | The indexed page is a ten-club feed-hijack transition matrix, not a standalone repeating feed graph. Keep transition identity and actor labels. |
| `highgate-funkybookendsfeedingmaybeandfivecountpopcorn` | 2017 208 / PDF 209; 2014 146 | Same transition-matrix source; do not promote an individual box to a complete periodic pattern without tracing its labeled source/target states. |
| `highgate-hijackinginmartinsonecount1` | 2017 64 / PDF 65; 2014 36 | Explicit finite start → hijack → reaction → return example. A repeating cycle alone loses the transition. |
| `highgate-hijackinginperiodfivepatterns` | 2017 62–65 / PDF 63–66; 2014 36–37 | Section heading and rules plus examples, not an individual pattern. |
| `highgate-hijackinginwhynot` | 2017 62–63 / PDF 63–64; 2014 36 | Explicit finite transition example. Preserve the starting, hijack, reaction, and return phases. |
| `highgate-martinsonecountfeedingfivecountpopcornandparsnip` | 2017 130 / PDF 131; 2014 86 | Indexed feed-hijack transition-matrix material; not enough to define a standalone periodic three-person graph. |
| `highgate-martinsonecountfeedingjimstwocount122martinsonecountfeedingmaybe` | 2017 130 / PDF 131; 2014 86 | Joined/index-derived row spanning multiple names; split/resolve aliases before treating it as a pattern. |
| `highgate-martinsonecountfeedingmaybeandwhynot` | 2017 130 / PDF 131; 2014 86 | Transition-matrix candidate. The labels identify a switch between periodic states, not a full event graph. |
| `highgate-maybefeedingfivecountpopcorn` | 2017 208 / PDF 209; 2014 146 | Ten-club feed-hijack transition-matrix entry; retain transition labels rather than a fabricated steady-state cycle. |
| `highgate-maybenot` | 2017 63 / PDF 64; 2014 34 | Individual periodic pattern; transcribed. |
| `highgate-notlikely` | 2017 63 / PDF 64; 2014 34 | Individual periodic pattern; transcribed. |
| `highgate-othersevenclubpatterns` | 2017 100–108 / PDF 101–109; 2014 64–68 | Section heading. Its numbered contents include independent siteswaps; keep the heading out of the pattern list. |
| `highgate-programminginparsnipvsfunkybookends` | 2017 64 / PDF 65; 2014 35 | Finite programming/steal transition with separate initial, transition, and resulting throws. |
| `highgate-sevenclubjimstwocountvariation1` | 2017 105 / PDF 106; 2014 67 | Periodic individual graph transcribed; 28 event nodes and 24 causal edges. Holds have no outgoing causal edge. |
| `highgate-sevenclubjimstwocountvariation2` | 2017 105 / PDF 106; 2014 67 | Periodic individual graph transcribed; 28 event nodes and 24 causal edges. |
| `highgate-sevenclubjimstwocountvariation3` | 2017 105 / PDF 106; 2014 67 | Periodic individual graph transcribed; 28 event nodes and 24 causal edges. |
| `highgate-sevenclubwhynotvariation` | 2017 105–106 / PDF 106–107 | Periodic individual graph transcribed; 28 event nodes and 28 causal edges. The 2014 “Seven club why not? style” at p. 65 is not the same named variation; I did not claim it as a matching edition. |

## What the model can and cannot preserve

The eleven graph records are periodic two-juggler diagrams. The four seven-club
records use a 14-beat period; the seven period-five records use a 10-beat
period. Each period repeats the listed per-juggler sequence twice, preserving
right/left hand parity. Ben's lane begins half a beat after Anne's, as stated
in the source. The edge timing follows the source convention
`landing time = source event time + throw height − 2`; pass/double-pass edges
change jugglers, self and heff edges remain with the thrower, and zips point
one beat back in the same lane. Source text identifies zaps as 2.5-beat
half-spin passes and specifies straight/crossing zap directions; those routes
were checked against the drawn lanes. A hold is represented as a node with no
outgoing edge. Heights remain explicit so diagram spacing is not mistaken for
causal timing.

The periodic editor state cannot encode finite start/hijack/reaction/return
sequences or an individual transition selected from a labeled transition
matrix. A future representation should contain a sequence of named periodic
states plus finite transition events, including the acting juggler, changed
throw, recipient/role reassignment, and event timing. The source diagrams
show that a hijack club is sent to a hand expecting a zip; the displaced club
is then sent back to the hijacker, and a missing club is zipped to avoid a
gap. These action rules do not by themselves specify every edge in each
example; transcribe the drawn arrows and phases rather than synthesizing an
edge from the prose rule alone.

## Visual source checks

The 2017 scans were visually reviewed at printed pp. 62–65, 100–108, 130, and
208. The matching 2014 scans were reviewed at pp. 34–37, 64–68, 86, and 146.
The 2014 p. 34 diagrams support the four period-five graphs; the 2014 pp. 64–68 diagrams support the three Jim two-count variation graphs
and the adjacent seven-club variants. The 2014 p. 65 “why not style” is a
different named entry from the 2017 “why not variation” record, so it is not
listed as a cross-check for that JSON record.
