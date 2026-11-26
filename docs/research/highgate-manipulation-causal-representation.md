# Manipulation patterns in causal diagrams

## Finding

There is a source-backed representation for manipulator patterns. Bone008's
JIF code models the manipulator as another juggler with two hands. While the
manipulator is waiting, it fills their lane with alternating one-count,
hand-to-hand throws (`duration: 1`). Intercepts and substitutions alter the
base throw graph and are marked as manipulated. Bone008's web app has a
`causal` arrow mode for the resulting throw table.

Separately, Christian Kästner's **Takeouts in 4-Handed Siteswaps** documents
causal diagrams for takeouts directly. It uses three participant lanes so the
diagram shows both passers and the manipulator. The intercept is marked at the
landing time, extra throws caught by the new manipulator are marked as dotted
red edges, and a solid boundary marks the cycle's role change. The text states
that the manipulator starts with one club in addition to the base pattern.

These sources strongly match the user's clue about a one-club manipulator who
keeps making 1s. I did not find evidence that Bone008 authored the Kästner
page or that this exact diagram was presented at a workshop. The page does
describe patterns developed at Colorado Passout 2014 and EJC 2018; the Passing
Zone workshop article found during this search teaches manipulation mechanics
but does not claim to define the causal-diagram notation.

## Concrete representation to use

1. Keep a distinct event lane for the manipulator, with right- and left-hand
   throw nodes. During inactive intervals, add alternating 1-count
   hand-to-hand events; do not imply that these edges trace the physical path
   of one club.
2. Preserve the base pattern's throw nodes and causal edges. For each
   interception or substitution, record which event is manipulated and the
   destination/owner change. Mark the intercept at its landing time, when the
   passer/manipulator roles change.
3. Add the extra incoming edges for throws that were already in the air to the
   old passer and are consequently caught by the new manipulator. The Takeouts
   page distinguishes these from the single pivotal intercept.
4. Mark each cycle boundary where roles repeat under a relabeling. If the
   manipulation has a finite lead-in or transition, retain that as a
   transition until the editor can represent it; do not force it into one
   periodic graph.
5. Keep physical handling (which hand receives or places a club, and whether
   the club is caught by the handle or body) as explicit action metadata or
   source notes. A causal edge alone does not encode those mechanics.

The established causal-diagram convention treats a zip/1 as a backward causal
edge, rather than a club traveling backward. The exact edge timing still must
be transcribed from each source diagram; Bone008's JIF duration values should
not be copied as causal offsets without converting their time scale.

## Source evidence

- [Bone008/jif-playground — manipulation.ts](https://github.com/Bone008/jif-playground/blob/main/src/manipulation.ts): adds a manipulator juggler with two limbs, routes substituted/intercepted throws, and generates alternating `duration: 1` throws during idle intervals.
- [Bone008/jif-playground — JIF type](https://github.com/Bone008/jif-playground/blob/main/src/jif.ts): represents a throw with time, duration, source/target limb, and an `isManipulated` flag.
- [Bone008/jif-web — ThrowsTable.tsx](https://github.com/Bone008/jif-web/blob/main/src/components/ThrowsTable.tsx): renders manipulated throw markers and computes arrow overlays in `causal` mode over the JIF throws.
- [Takeouts in 4-Handed Siteswaps](https://www.cs.cmu.edu/~ckaestne/takeouts.html): describes the three-line causal diagram, intercept landing/role swap, dotted catches, cycle boundary, and starting with one extra club.
- [Basic Elements for Take Out Passing (Workshop)](https://passing.zone/basic-elements-for-take-out-passing-workshop/): a workshop resource for self substitutions, pass substitutions, pops, and handling conventions. It supports the action vocabulary, but is not the source for the causal diagram layout.
- [Passing Patterns Compendium — terminology and notation](https://www.jugglingedge.com/pdf/PassingPatternsAug06.pdf): describes causal-diagram lanes, hand labels, arrow meanings, and the backwards causal edge used for zips.

## Remaining uncertainty

The exact workshop the user remembers is not identified. The strongest
diagram-specific source found is the Kästner takeout collection; the strongest
Bone008 implementation evidence is the JIF manipulator generator and its
causal-arrow web view. These are complementary evidence, not proof that they
are the same implementation or presentation.
