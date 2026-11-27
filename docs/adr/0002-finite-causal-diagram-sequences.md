# ADR 0002: Represent finite causal-diagram sequences as ordered metadata

## Status

Accepted

## Context

The editor stores one periodic graph. Some named passing patterns are sequences of distinct periodic graphs separated by finite actions such as a walk. Treating one phase as the whole pattern loses both ordering and the action. Replacing the periodic state model would also invalidate existing `j4.` links.

## Decision

Keep each periodic graph in the existing editor graph fields. Define finite sequences in a separate immutable catalog as ordered phase state IDs and explicit transition events. Add optional sequence ID and phase index metadata to editor state and serialize those states as `j5.`; continue emitting and decoding unchanged `j4.` states when no sequence metadata is present. Sequence metadata identifies catalog data and does not duplicate graph data.

The first example is Highgate's thirteen-club Runaway: four ordered phase IDs and two documented walk events (Daniel after phase 0, Anne after phase 2). Each event records the described 2.5-beat interval and three self throws. The source does not provide enough detail to animate the walk trajectory or encode every throw during that interval; those are not inferred by this model.

## Consequences

- Existing periodic deep links remain byte-shape compatible.
- A finite-sequence deep link can identify both its sequence and selected phase while retaining the normal editable graph.
- Catalog validation can reject unknown sequence IDs and phase indices.
- Rendering and animation can be added without moving pattern data into component state.
