# Highgate transcription audit: printed pp. 196–210

## Included

- `highgate-tenclubonecountfeedingcountdown` — 2017 expanded PDF printed p. 196 / PDF p. 197. The diagram and three written throw sequences were visually checked. Ben's eight throws go to Anne, Anne, Clare, Anne, Clare, Clare, Anne, Clare; Anne and Clare each have the listed self throws and crossing single passes to Ben. Ben starts at phase 0; Anne and Clare start half a beat later. The two listed cyclic siteswaps are retained in the JSON. No corresponding entry was found in the local 2014 PDF.
- This is a regular three-person feed, not a manipulator pattern. “Gorilla” on the following pages describes a feeder splitting ten clubs between two partners; it does not describe a solo manipulator.

The JSON stores 24 events, one per person per beat over an eight-beat cycle, including their hands, phase offsets, throw routes, and source provenance. A state reconstructed from the serialized graph passed `IsValidForEditor()` after `SynchronizeHeights()`. All source cycle offsets survived synchronization unchanged.

## Visually reviewed, omitted from the chunk

- The synchronous one-count feed and the dark/ambidextrous/itchy gorilla examples include same-person throws from both hands at one instant. The editor has one event lane per person, so those events overlap and cannot be represented faithfully.
- The 972 feeds and ten-club period-five feeds include zip or gap arrows. The current editor normalization does not preserve those backward arrows. Hijacking in period five is a transition between patterns rather than a stationary repeating pattern; it is documented separately in the source.
- Ten-club one-count feasts have simultaneous hand throws, so they need the same-time lane support before they can be imported.

The reviewed pages provide no solo manipulator pattern. The period-five hijacking discussion concerns transitions between ordinary passing patterns; it should be classified separately from stationary manipulator patterns.
