# Manipulator causal-diagram / workshop research

Research status: strong match for a published causal-diagram collection; no source yet identifies the exact workshop talk described by the user. The one-extra-club clue matches standard takeouts broadly, but there are two distinct notations that should not be conflated.

## 1. Direct causal-diagram collection: Christian Kästner's 4-handed takeouts

Source: [Takeouts in 4-Handed Siteswaps](https://www.cs.cmu.edu/~ckaestne/takeouts.html), by Christian Kästner (page footer). This is the clearest direct evidence found for genuine causal diagrams of manipulator patterns.

The page's “Mechanics, assumptions, and notation” section says:

- The manipulator starts with one extra club, in addition to the props needed by the base pattern.
- Each pattern requires one intercept (`I`); it can also have a carry (`C`) and any number of substitutions (`S`). Because the two passers pass on distinct beats, these actions can be marked on the beat without extra indices.
- The intercept is the role-change point. The landing of the intercepted club changes the passer/manipulator roles. The diagrams mark this with a red line and a vertical dotted line; clubs already airborne to the old passer are shown as now caught by the manipulator with dotted red lines.
- Each diagram has three person-lines and spells out who passes to whom and when. The author explicitly says the diagrams follow from the notation.
- Text notation begins at the intercept, while the diagram includes earlier beats to show a typical lead-in.

Concrete examples on the page:

- **What's that about** — `786277862778627`, `I` plus `C`; based on WhyNot. The page credits Warren Hammond, developed at Colorado Passout 2014.
- **Manège à trois** — `786786786`, `I` plus `C`; developed at EJC 2018 on the Azores.
- Other diagrammed examples include `567-about` (`756756756`, `I C`), The Guillotine (`729729729`, `I C`), Holy-Grail-About (`759759759`, `I C`), `726-about`, `777-about`, `978-about`, `966-about`, Dropbear (`S I C`), Parsnip-about (`S I`), and Catch the Carrot (`C I`).

This is a strong match for a user seeking manipulation patterns rendered as causal diagrams. The one club here is an extra club held by the manipulator. It does **not** mean that the manipulator continuously performs a solo siteswap of 1s.

## 2. Explicit continuous manipulator siteswap: Prechac Chronicles, “Overlay Takeouts”

Source: [Overlay Takeouts](https://prechac.github.io/2016/12/25/overlay-takeouts.html), dated 25 Dec 2016 and credited to Florian Marienfeld, Jochen Hänsel, Jochen Pfeiffer, and Tine Oymann.

This paper-like tutorial is the closest exact match to the “manipulator has one club and continuously juggles 1s” clue, but its diagrams/model are Prechac overlay diagrams, not the three-line causal diagrams on Kästner's page or the `S/I/C` action notation.

Locators and derivation:

- Lines 19–20 explain the method: overlay a custom manipulation siteswap (itself another Prechac pattern) onto a passing pattern.
- “Delightful,” lines 23–45, starts with five clubs for passers and one for the manipulator. Base pattern: `3p 3 1 3`. The solo manipulator pattern is `3 1 3 1`; overlaying it transforms a shared 3 into a 1p. The resulting role sequences are `A: 3p 3 1 1p`, `B: 1 3 3p 3`, `M: 1 1p 1 3`. Thus M does indeed juggle repeated `1`s, interspersed with the manipulation pass (`1p`), rather than holding one club and using only S/I/C labels.
- Lines 48–62 explain role exchange and the general construction algorithm.
- “Mission Impossible,” lines 63–89, shows another three-role overlay; M's row is `2 3 1.5p 0 1`, with a low-count passing overlay.
- Lines 96–97 explicitly contrast this notation with conventional takeouts and note that their representation spells out the passer/manipulator interaction.

This is likely relevant if the remembered clue is specifically a continuous `1` sequence. It is not by itself evidence of a workshop presentation, and the authors do not call these diagrams causal diagrams.

## 3. Modern Club Passing's causal-arrow notation

Source: [Modern Club Passing — Manipulator Pattern Notation](https://modernpassing.com/6b-notation.html), chapter source in [ckaestne/modernpassing](https://github.com/ckaestne/modernpassing) (`src/6b-notation.md`). The chapter's “Local Notation” section explains a different representation: write manipulator actions as ordinary passes in the usual passing pattern, then use causal arrows to show where each club goes.

Its concrete Roundabout walk-through maps:

- A substitution to two passes: the original throw becomes a `pelf` (a gentle self/pass to M) with the timing of a siteswap `1`, shown by a backward causal arrow; M then places the replacement club as an ordinary pass.
- An intercept to an incoming pass directed to M; the landing switches the roles.
- A carry to an ordinary pass sent to the feeder/new target.
- Empty and held beats are written explicitly.

This explains the “1” clue in a third way: the substitution timing corresponds to a `1`, but the chapter represents the actual manipulator action as a normal passing pattern with arrows. It is not evidence that an identified workshop presenter gave a talk.

## 4. Workshop resources and the named presenters found so far

### Passing.Zone basic-elements workshop

Source: [Basic Elements for Take Out Passing (Workshop)](https://passing.zone/basic-elements-for-take-out-passing-workshop/), author Wiebke S., published 18 Apr 2025 (site metadata; page shows a 2026 modification date).

The article is a video-led skill workshop, not a causal-diagram lecture. Its text teaches:

- self-substitute: replacing a self with an extra club and calls the gentled throw a **pelf**;
- pass-substitute variants (early, middle, late);
- pops and the “Pass ’n Pop Practice Pattern” on 4-count;
- hand conventions and zips after manipulations.

It links follow-on practice patterns Catch the Carrot and Dolby 5.1. No causal-diagram or continuous manipulator-siteswap notation appears in the article text inspected. The workshop page itself is concrete evidence of practical teaching, but it does not identify the historic workshop event suggested by the user.

### IJA/eJuggle articles

The IJA May 2014 newsletter identifies **“Scrambled Passing Patterns and Takeout Notations: Part 1” by Warren Hammond** as a May 2014 article (newsletter PDF p. 16; [PDF](https://dev.juggle.org/business/newsletters/IJAnews-2014-05.pdf)). The June 2014 newsletter reproduces the article excerpt and says Part 2 by Warren Hammond and Aidan Burns was forthcoming (PDF p. 11; [PDF](https://dev.juggle.org/business/newsletters/IJAnews-2014-06.pdf)). These establish the authorship and date of an important takeout-notation article, not a workshop presentation or the causal-diagram system. Kästner's takeout page separately credits Hammond with the original 4-handed siteswap takeout at Colorado Passout 2014.

## 5. Search result and open question

The evidence presently supports three related but separate things:

1. Kästner: one-extra-club, S/I/C takeout patterns drawn as explicit three-person causal diagrams, including named 4-handed siteswap examples.
2. Prechac Chronicles: an actual solo manipulation siteswap overlaid onto a passing pattern, with a manipulator row containing repeated `1`s; this is an algorithmic Prechac representation.
3. Modern Club Passing: a substitution represented as ordinary passes and causal arrows, with a timing-1 pelf.

I did not find a primary source yet naming the particular workshop, presenter, and date that join the user's remembered causal diagrams to one of these representations. The passing.zone video workshop is by Wiebke S. and covers physical elements; the 2014 eJuggle notation article is by Warren Hammond (Part 2 also Aidan Burns); the online causal-diagram collection is by Christian Kästner. Do not infer that any of these people presented the remembered workshop until a primary event schedule, recording, or handout confirms it.

## 6. Bone008 identity and connection check

The parent project identified GitHub user Bone008 as **Lukas Bonauer**. Public sources support a connection to pattern tooling and Aidan-pattern combinations, but the evidence inspected does not connect him to the causal-diagram method or a workshop presentation:

- Passing.Zone's [Resources page](https://passing.zone/resources/) credits the “Notations & Orbits” application to Lukas and describes it as generating notation, orbits, and causal arrows for synchronous passing patterns (2-count, 3-count, PPS). It does not say that the application encodes takeout/manipulator diagrams.
- Modern Club Passing's [Aidan Patterns chapter](https://modernpassing.com/6f-aidan-patterns.html) says Lukas Bonauer created a spreadsheet of combinations of Aidan manipulator actions. This is evidence of Aidan-notation work, not authorship of Aidan notation or workshop slides.
- In the cloned `ckaestne/modernpassing` repository, the inspected history for `src/6b-notation.md` attributes the edits to Christian Kästner. The causal-arrow chapter's `Local Notation` should therefore be credited to Kästner as presented there, not automatically to Bone008/Lukas.
- The GitHub repositories identified by the parent (`Bone008/jif-playground`, `Bone008/jif-web`) concern Juggling Interchange Format; finding a serialization for diagrams there would not alone establish the historical workshop or the intended published diagrams.

No primary event schedule, dated slide deck, talk recording, or handout was found that identifies Lukas Bonauer as presenter of a causal-diagram translation workshop. This remains open.

## 7. Workshop event now identified; exact handout remains unverified

A concrete workshop source surfaced after this note's initial assessment:

- Passing.Zone, Nicki, [“Take-Outs in 4-handed siteswaps”](https://passing.zone/take-outs-in-4-handed-siteswaps/), published 2 Aug 2019. The opening paragraph reports that at “the last Passout in Krelingen” Christian “from Pittsburgh” introduced takeouts in a 4-handed siteswap and taught `726-about`, “What's that about?” and “Manège à trois” (`786-about`). The convention recap identifies this event as PassOut Krelingen 2018–2019: [recap](https://passing.zone/passout-krelingen-recap/). This triangulates the person as Christian Kästner and the teaching context/patterns.
- Independently, the [MONDO 2018 Festival schedule PDF](https://www.mondofest.org/wordpress/wp-content/uploads/2018/02/MONDO_2018_Schedule_v4.pdf), Sunday 11:00–12:00, lists “4 Person Club Take Out Patterns — Christian Kaestner.” This is an earlier matching workshop listing. MONDO XXIX ran 16–18 Feb 2018 (festival registration form); the schedule identifies his session on Sunday, 18 Feb, 11:00–12:00. The schedule does not describe its notation or handout.
- Kästner's [CMU collection](https://www.cs.cmu.edu/~ckaestne/takeouts.html) provides actual three-line causal diagrams for the 4-handed takeout family, including `What's that about` and `Manège à trois`, and says the manipulator begins with one extra club. The Passing.Zone post confirms he taught those named patterns, but neither source proves that the exact CMU diagrams were the diagrams used in the Krelingen/MONDO sessions. No surviving slide deck or workshop recording/handout was located in the searches conducted.

### Refined conclusion

The likely remembered causal-diagram workshop is Christian Kästner's teaching of four-handed-siteswap takeouts (explicitly documented at PassOut Krelingen 2018–2019; also listed at MONDO 2018). The matching pattern examples and the genuine causal-diagram versions are on his CMU page. Treat the direct link between the historical session and the currently published SVGs as a strong inference, not a documented fact.

The specific “one club continuously juggling 1s” clue remains a different formalism. In Kästner's collection, the manipulator begins with one additional held club; the normal throw sequence is a 4-handed siteswap annotated with S/I/C actions. In Prechac Chronicles' “Overlay Takeouts,” M's role row is an actual siteswap sequence containing repeated 1s (`1 1p 1 3`), derived from solo `3 1 3 1`; its role rows are Prechac overlay notation. No located primary source combines that exact repeated-1 row with Kästner's causal diagrams or associates it with the named workshop.

## 8. New close match to the exact “continuous 1s” clue: Lukas Bonauer / Bone008

The Bone008 `jif-web` project combines the two features that earlier sources kept separate: a causal-arrow rendering mode and an explicit manipulator whose idle beats are filled with continuous siteswap-1 throws.

### Direct implementation evidence

- [Bone008/jif-web `src/jif/manipulation.ts`](https://github.com/Bone008/jif-web/blob/main/src/jif/manipulation.ts#L258-L283) documents `fillManipulatorThrows` as generating manipulator throws “(1s)” over every beat in an interval. The function creates duration-1 throws from alternating right and left hands of the new manipulator. The resulting JIF adds two hand limbs for M, so one object zips between the hands as repeated 1s between manipulations.
- [The app's view settings](https://github.com/Bone008/jif-web/blob/main/src/components/ViewSettings.tsx#L6-L7) includes a `causal` arrow mode, alongside orbits, ladder and none. This means this repo's rendering can display the manipulation-augmented pattern in causal mode; its source implementation is not just the S/I/C line notation.
- [Named manipulation presets](https://github.com/Bone008/jif-web/blob/main/src/jif/presets.ts#L104-L169) include 3-count Roundabout, 4-count Roundabout, Ronjabout, Phoenician Waltz, Göttinger Opernball, 7-club PPS-about, Dolby 5.1, Dumb Ways to Die, and others. For example, 3-count Roundabout combines a base-pattern data array with the manipulator instruction `- - - sa - i1b`; the implementation fills other beats with 1s. `Dumb Ways to Die` has a single manipulator instruction string `- i1A - - - i2B - - - i1C - -` over a 12-beat 3-person base.
- The repository README describes the app as a visualizer for siteswap notations of passing patterns and links to the running app at [notations.bonauer.me](https://notations.bonauer.me/).

This fits the remembered mechanics more closely than Kästner's public “one extra held club” prose or the Prechac `1 1p 1 3` overlay row. It should be described precisely as: one added manipulator person, with a single active club continuously transferred hand-to-hand on 1s between instructed substitutions/intercepts; at manipulation beats the incoming/outgoing club count changes. Calling this “one club” is correct for the idle zipping sequence, but can be misleading at an action beat where the manipulator catches/places a pattern club.

### Matching workshop evidence

Passing.Zone's [PassOut Dreiskau-Muckern recap](https://passing.zone/passout-dreiskau-muckern/), published 11 Jan 2025 about the 2024–2025 PassOut, records in its Theory section: “Orbits in manipulator patterns — Lukas demonstrated some awesome software which can find orbits in takeout patterns e.g. magic clubs and ‘beer clubs’.” The article says the software was then only available on Lukas's localhost. This identifies **Lukas Bonauer** and a relevant workshop/demo context shortly after the `jif-web` repo history's initial manipulation code (December 2024).

This is a stronger candidate for the user's precise combination of “manipulator continuously juggles 1s” and manipulation-pattern diagrams than the Christian Kästner workshop: the developer's app explicitly draws manipulation patterns with a continuous M=1 fill and has causal view selection, and Lukas is documented presenting software about manipulator-pattern orbits at PassOut. However, the PassOut recap calls it orbit-finding software and does not say that causal diagrams or the continuous-1 representation were part of that presentation. No exact slide deck, handout, screen recording or workshop-specific pattern list was found. The exact presentation-to-rendering link therefore remains inferred, although the repository independently demonstrates that the same person's tool implements both.

### Candidate pattern examples to inspect / match

For any future comparison with workshop media, useful distinctive candidates already present in the repo are:

- **3-count Roundabout** — base 3-count passing with manipulator `- - - sa - i1b`.
- **4-count Roundabout** — standard 4-count base, manipulator `sa - sb - ia -`.
- **Göttinger Opernball** — three manipulator lines over a two-passer base, a distinctive pattern with multiple role changes.
- **Dumb Ways to Die** — three-person base, one manipulator, multiple intercepts and 1-filled gaps.
- **Muckabout** — two manipulators, marked unverified/unfinished in the preset.

The above are verified as current repository presets, not as patterns proven to have appeared in the workshop.

### Chronology adds a strong, but still indirect, bridge

The public GitHub history is unusually close in time to that convention: Lukas imported the JIF implementation (including `src/jif/manipulation.ts` and orbit calculation) into `Bone008/jif-web` on 26 Dec 2024; the visible arrow-mode controls, with `causal` among the choices, were added 28 Dec 2024; more presets and “beerable” functionality were added 30 Dec 2024. The PassOut recap covers the same 2024–2025 New Year gathering and reports Lukas's orbit-software demonstration. This chronology makes it likely that the showcased local software was the project now published as `jif-web`, though the recap does not name the app/repository and therefore cannot prove the identification.

## 9. Direct data recipe for the Highgate translator (Bone008 snapshot)

Use the pinned source snapshot [`63134a9`](https://github.com/Bone008/jif-web/tree/63134a91063cebd3820479cd364e714663ac60f8), especially:

- [`src/jif/presets.ts`](https://github.com/Bone008/jif-web/blob/63134a91063cebd3820479cd364e714663ac60f8/src/jif/presets.ts): preset names, optional slugs, base local-throw strings, manipulator-action strings, and warning notes.
- [`src/jif/test_data.ts`](https://github.com/Bone008/jif-web/blob/63134a91063cebd3820479cd364e714663ac60f8/src/jif/test_data.ts): reusable 3-count/4-count/PPS bases and 9-club/10-club walking-feed bases.
- [`src/jif/high_level_converter.ts`](https://github.com/Bone008/jif-web/blob/63134a91063cebd3820479cd364e714663ac60f8/src/jif/high_level_converter.ts): parses Prechac rows (`3B` means a height-3 pass to B; bare `3` is a self) and manipulator-action strings.
- [`src/jif/preset_loader.ts`](https://github.com/Bone008/jif-web/blob/63134a91063cebd3820479cd364e714663ac60f8/src/jif/preset_loader.ts): the exact preset-to-JIF conversion order.
- [`src/jif/manipulation.ts`](https://github.com/Bone008/jif-web/blob/63134a91063cebd3820479cd364e714663ac60f8/src/jif/manipulation.ts): adds M, replaces base throws, swaps role/limb permutations on intercepts, and fills idle M beats with 1s.
- [`src/jif/jif.ts`](https://github.com/Bone008/jif-web/blob/63134a91063cebd3820479cd364e714663ac60f8/src/jif/jif.ts): output serialization fields.
- [`src/components/ViewSettings.tsx`](https://github.com/Bone008/jif-web/blob/63134a91063cebd3820479cd364e714663ac60f8/src/components/ViewSettings.tsx): `arrowMode=causal` view option.

### Input/output fields and transform

A `Preset` contains `{name, slug?, instructions: string[], manipulators?: string[], warningNote?}`. Preserve the warning note; several experimental items are explicitly unverified or not supported by the converter.

Conversion order from `preset_loader.ts`:

1. Join `instructions` with newlines. If it begins with `{`, it is raw JIF JSON; if it has no whitespace, parse it as an asynchronous siteswap; otherwise parse each line as a Prechac-style row.
2. `loadWithDefaults` determines the repeat period and the role/hand permutation.
3. Parse each `manipulators` string and call `addManipulator` once per string. M is an added person with right/left hands. Every `-` interval is *not an empty pattern row*: the implementation fills those beats with duration-1 throws alternating R/L, until the next action.
4. Serialize the expanded JIF: `jugglers`, `limbs`, `throws[{time,duration,from,to,isManipulated}]`, and `repetition{period,limbPermutation}`. This throw graph is the data from which the app draws arrows; causal diagram SVG is rendered, not stored as a separate causal-diagram string.
5. In the app, select causal rendering via `?pattern=<slug>&arrowMode=causal`. Example preset route shape: `https://notations.bonauer.me/?pattern=scrambled-v&arrowMode=causal`. Slugs are explicit where provided; otherwise `getPresetSlug` lowercases and hyphenates the name. Some Ambled names therefore need to be resolved with the exported slug function rather than guessed.

Manipulator tokens are whitespace-separated and their position is a zero-based beat index in that role cycle:

- `-`: no action at that beat; M's continuous 1 throws fill the idle interval.
- `sA`, `sB`, `sC`: substitute the throw from that named base role. Bone's transform turns the original into a duration-1 transfer to M and sends M's held object along the original throw's path/duration.
- `i1A` / `i1B`: one intercept timing variant (`intercept1b`).
- `iA`, `i2A`, etc.: the other intercept timing variant (`intercept2b`). The `1` vs `2` suffix is semantically meaningful; preserve the token and transformation rather than normalizing it away.
- Optional prefix `M_label:` names the manipulator. Role target letters refer to the base juggler labels; intercept changes who the future manipulator is.

Base row tokens are siteswap height in base-36 plus optional recipient (`3B` = 3 to B, `3` = self). A final `-> A` sets explicit next-period role relabeling. The causal-arrow timing can run backward for 1s, as normal in causal diagrams; it should be derived from JIF `time`/`duration` and hand endpoints, not treated as an ordinary forward pass duration.

### Complete preset inventory

The source currently contains **82 manipulation presets**: 10 in “Manipulation Patterns”, 27 “Scrambled”, and 45 “Ambled”. Keep category and sequence number: the numbering encodes the action-order families and acts as stable human-readable identity even when a slug is absent.

**Manipulation Patterns (10):** 3-count Roundabout; 4-count Roundabout; Ronjabout; Phoenician Waltz; Göttinger Opernball; 7-club PPS-about; Dolby 5.1; Dolby 5.1 with Doppelgänger; Dumb Ways to Die; Muckabout (TODO).

**Scrambled (27):**

- #1.1 Aidan; #1.2 Casbia; #1.3 Cascia; #1.4 Scrambled 3; #1.5 Wrong One; #1.6 Zig Zag; #1.7 Chopped up V; #1.8 Scrambled V; #1.9 Postmen.
- #2.1 Around the World; #2.2 Unscrambled LB; #2.3 Ivy; #2.4 Unscrambled B; #2.5 Chopped up B; #2.6 Gentle Romble; #2.7 Moonwalk; #2.8 3 Variation; #2.9 Wankle Engine.
- #3.1 Anna-Maria; #3.2 Panto; #3.3 Buddy Check; #3.4 Last One; #3.5 WUST; #3.6 Saibca; #3.7 Toast; #3.8 Right One; #3.9 Vegemite Toast.

All 27 use `RAW_DATA_WALKING_FEED_9C`, with one action-string variation each. In source, e.g. #1.8 Scrambled V is `- - sB - i2C -`; the prior action IDs in the preset name describe carry/substitute/intercept recipient roles, while `sX`/`iY` are the stored executable operations.

**Ambled (45):**

- #1.1 Turn Around; #1.2 Ambled Back to the Future; #1.3 Ambled Wust; #1.4 Ambled Saibca.
- #2.1 Zap Me Maybe; #2.2 Super Lazy B; #2.3 Choptopus; #2.4 De Dikke; #2.5 Guck in die Luft.
- #3.1 Unnamed (to juggle!); #3.2 Ambled V; #3.3 Ambled Postmen; #3.4 Ambled Chopped V.
- #4.1 Unnamed (to juggle!); #4.2 Ambled Wrong One; #4.3 Ambled Zig Zag; #4.4 Ambled 3.
- #5.1 Ambled Casbia; #5.2 Ambled Cascia; #5.3 Ambled Aidan; #5.4 Unnamed (to juggle!); #5.5 Unnamed (to juggle!).
- #6.1 Unnamed (to juggle!); #6.2 Ambled Chopped B; #6.3 Ambled Gentle Romble; #6.4 Ambled B; #6.5 Unnamed (to juggle!); #6.6 Unnamed (to juggle!).
- #7.1 Unnamed (to juggle!); #7.2 Ambled LB; #7.3 Ambled Ivy; #7.4 Around the Ambled World.
- #8.1 Ambled 3 Variation; #8.2 Ambled Wankle Engine; #8.3 Ambled Moonwalk; #8.4 Unnamed (to juggle!).
- #9.1 Unnamed (to juggle!); #9.2 Ambled Buddy Chicks; #9.3 Ambled Panto; #9.4 Ambled Anna-Maria.
- #10.1 Unnamed (to juggle!); #10.2 Ambled Vegemite Toast; #10.3 Ambled Right One; #10.4 Ambled Toast; #10.5 Unnamed (to juggle!).

All Ambled presets use `RAW_DATA_WALKING_FEED_10C`, except #5.5 uses `RAW_DATA_WALKING_FEED_10C_RIGHT_HANDED_DS`; they differ in the action strings. Preserve `warningNote` on #2.4 (“Claude did wrong and Lukas didn't catch on!”) and #5.5, #6.1, #10.1, whose notes say the app does not yet support multiple manipulation annotations on the same beat. Muckabout's warning explicitly says unfinished/unverified.

### Practical pattern-to-causal-data examples

- **Aidan (Scrambled #1.1):** base `RAW_DATA_WALKING_FEED_9C`; manip `- - sC - i2B -`.
- **Scrambled V (#1.8):** same 9-club base; manip `- - sB - i2C -`.
- **Ambled V (#3.2):** 10-club base; manip `- - - sB - i2C -`.
- **Göttinger Opernball:** explicit two-row `3B/3A` 9-beat base plus three action rows, which is the best repo fixture showing multiple manipulators/role lines.
- **Dumb Ways to Die:** explicit three-row base plus `- i1A - - - i2B - - - i1C - -`, useful for multiple role switches and intercept timing.

For generating actual arrow coordinates/diagram assets, read the `JIF` after `loadPreset`; `ThrowsTable.tsx` derives causal `ArrowData` from each throw. Do not translate the printed S/I/C summary alone when the expanded JIF is available: the preset contains specific target roles and base throw timings needed for causal endpoints.


### Causal-arrow endpoint rule in Bone008 renderer

For each expanded JIF throw, `ThrowsTable.tsx` uses the throw's start coordinate as the arrow source and computes destination time as `t2 = time + duration - arrowTimeDelta`; destination hand/juggler comes from the JIF `to` limb. In the inspected snapshot, `arrowTimeDelta` is a heuristic: 2 for patterns classified synchronous, 4 otherwise (the code itself marks this TODO). In a limb table, endpoints are limb indices; in a juggler table, endpoints are `limbs[from/to].juggler`, wrapped across the role permutation. This is the app's operational causal rendering rule; use the actual target tool's standard arrow semantics if those differ.

The JIF's `isManipulated` marker identifies altered source throws and the replacement throw. It is useful metadata for highlighting, but the arrow endpoints still come from the same `time/duration/from/to` rules. Do not collapse duration-1 manipulator zips: they intentionally produce causal arrows that point backward in time.
