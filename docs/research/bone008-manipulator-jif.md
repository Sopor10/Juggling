# Manipulator patterns in Bone008's JIF tools

## Finding

The closest direct match is [`Bone008/jif-web`](https://github.com/Bone008/jif-web), the source for the passing-pattern notation and visualization app linked from [modernpassing.com](https://modernpassing.com/). It does not store a finished causal-diagram drawing. Instead, a preset stores the underlying pattern notation plus optional per-beat manipulator instructions; the loader expands that into JIF throws, limbs, jugglers, and repetition data. That full JIF is the portable representation from which a diagram/animation is rendered.

Useful source files:

- [`src/jif/presets.ts`](https://github.com/Bone008/jif-web/blob/main/src/jif/presets.ts#L15-L24) defines presets with `instructions` and optional `manipulators`; the manipulation examples start at [line 104](https://github.com/Bone008/jif-web/blob/main/src/jif/presets.ts#L104).
- [`src/jif/preset_loader.ts`](https://github.com/Bone008/jif-web/blob/main/src/jif/preset_loader.ts#L27-L50) parses the base pattern, supplies JIF defaults, then applies each manipulator to produce the complete pattern.
- [`src/jif/high_level_converter.ts`](https://github.com/Bone008/jif-web/blob/main/src/jif/high_level_converter.ts#L140-L189) defines the manipulator text syntax and maps it to typed events.
- [`src/jif/manipulation.ts`](https://github.com/Bone008/jif-web/blob/main/src/jif/manipulation.ts#L23-L50) adds a manipulator and limbs; [lines 93–122](https://github.com/Bone008/jif-web/blob/main/src/jif/manipulation.ts#L93-L122) implement substitutions; [lines 123–220](https://github.com/Bone008/jif-web/blob/main/src/jif/manipulation.ts#L123-L220) implement intercepts/carries and role changes.
- [`src/jif/jif.ts`](https://github.com/Bone008/jif-web/blob/main/src/jif/jif.ts#L3-L9) defines the JIF record; [lines 47–66](https://github.com/Bone008/jif-web/blob/main/src/jif/jif.ts#L47-L66) define throws and period limb relabeling.

`Bone008/jif-playground` is the earlier prototype with the same core approach: [JIF schema](https://github.com/Bone008/jif-playground/blob/main/src/jif.ts) and [manipulation implementation](https://github.com/Bone008/jif-playground/blob/main/src/manipulation.ts). Prefer `jif-web` as the current reference because it adds explicit repetition/limb-permutation handling and maintains named presets.

## Manipulator DSL and mapping rules

The DSL is one whitespace-separated instruction per beat of the base pattern. A dash is an idle/unspecified beat. Each event ends in the letter of the passer whose throw is manipulated. Letters are case-insensitive and map `A` to juggler index 0, `B` to 1, etc. An optional `label:` prefix names the manipulator.

| Instruction | Meaning in parser | Effect on the JIF / causal diagram |
| --- | --- | --- |
| `sA` | `substitute` at this beat on passer A | The passer's normal throw is redirected to the manipulator (one-beat handoff); a new throw from the manipulator goes to the normal destination. Both throws are marked `isManipulated`. |
| `iA` or `i2A` | `intercept2b` (the default intercept) on A | Manipulator intercepts A's incoming throw, pauses/carries it for two beats, and A and manipulator exchange roles at the period boundary. Future causes/destinations and limb relabeling are updated. |
| `i1A` | `intercept1b` on A | One-beat/early carry variant; it uses the same intercept operation with the one-beat carry timing. |
| `-` | no event | Placeholder for a beat with no manipulator instruction. |

For the rendered diagram, the key is not a hand-drawn arrow annotation. It is the changed throw graph: each JIF throw carries `time`, `duration`, source limb index (`from`), target limb index (`to`), and optional `isManipulated`; limbs identify their juggler and right/left hand; `repetition.limbPermutation` records how hands/roles continue after one period. The implementation marks the intercepted/substituted throw, adds the manipulator's two limbs, redirects the throw edges, and changes future throw ownership/targets. A renderer can therefore distinguish the manipulator's physical prop path and the causal handoff from ordinary throws.

Use the full transform rather than merely adding a third juggler or marking the catch. The intercept changes who is doing which role after the event. For odd periods, the manipulator's right/left-hand permutation is swapped; at an intercept, the intercepted passer and manipulator's role permutation is exchanged. The implementation also shifts the cyclic origin to avoid intercept/carry crossing the period boundary.

### Concrete example: 3-count Roundabout

The preset in `jif-web` uses the two-person 3-count base pattern (`RAW_DATA_3_COUNT_PASSING_2X`) and the single manipulator row `- - - sa - i1b`. That means: after three empty beats, manipulator M substitutes a throw from A; after one empty beat, M performs an `i1` intercept on B. The parser lowercases `sa`/`i1b`, so capitalization does not change the target. The corresponding preset is [`presets.ts` lines 104–109](https://github.com/Bone008/jif-web/blob/main/src/jif/presets.ts#L104-L109).

The anthology describes the same family at “Roundabout” (PDF chapter 12.3.1, printed page 55): two passers A/B and manipulator M, a role transition `A → B → M → A`, manipulated throws indicated by dots, substitutions/intercepts/carries given as explicit events. Its notes state that an intercept swaps the manipulator and intercepted juggler roles and relabels their clubs (PDF printed page vii / “Notes on manipulator patterns”). See [Passing Patterns Compendium (PDF)](https://passing.zone/wp-content/uploads/anthology.pdf).

This confirms that the JIF transformation captures the same underlying semantics as the book notation: substituted throws, the intercept/carry, manipulated status, club ownership, and period-to-period role transitions.

## Other supplied sources checked

- [Modern Club Passing](https://modernpassing.com/) links to the author's interactive passing-pattern book and its [Takeout Explorer](https://takeout-explorer.modernpassing.deno.net/). The repo's README describes the project as visualizing passing siteswap notations and says its named presets are kept in `src/jif/presets.ts`.
- The [Passing Patterns Compendium](https://passing.zone/wp-content/uploads/anthology.pdf) is the useful source for manipulation semantics and notational definitions (sections “Notes on manipulator patterns” and “Roundabout family”). It documents `m` substitutions, `i` intercepts, `c` carries, dots over throws to mark manipulation, and role/club relabeling after intercepts.
- [The Passing Patterns Book (majbook v3)](https://passing.zone/wp-content/uploads/majbook_v3.pdf) has general passing patterns and diagrams, but a text search of the supplied PDF extraction found no dedicated manipulator/causal-diagram representation. It is supplementary context, not the implementation reference for manipulation.

## Limits and cautions

- Bone008's JIF is a pattern/event representation, not a causal-diagram JSON schema. It is useful as a reliable transform recipe and then can be mapped to this app's existing JIF and causal-diagram mapper.
- Some listed presets are explicitly incomplete or have known unsupported orbit calculations; do not treat every named preset as validated. For example, `Muckabout` carries a TODO warning and Dolby 5.1 warns about crossing carry.
- The DSL implementation has a TODO for validating overlapping manipulations (`manipulation.ts` around line 85). Validate the source data separately before accepting generated patterns.
- The compendium includes physical execution details (which hand receives a club, flips/zips, catches); those are not all encoded by JIF. The transform expresses the ownership/timing graph. Do not infer body orientation or walk path from it unless those are separately represented.
