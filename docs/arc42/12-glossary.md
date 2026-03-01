# Glossary

## Domain Terms

| Term | Definition | Example |
|------|------------|---------|
| **Siteswap** | Mathematical notation for juggling patterns using sequence of numbers | `531`, `97531` |
| **Period** | Number of throws in one complete pattern cycle | Period of `531` is 3 |
| **Throw Height** | Number indicating how many beats later an object lands | `5` means lands 5 beats later |
| **Objects** | Number of balls/clubs being juggled | `531` has 3 objects on average |
| **Ground State** | Pattern can be started from the default object distribution | `333` for 3 objects |
| **default object distribution** | same amount of objects per hand or one more object in the hand that throws first | siteswap `531` |
| **Excited State** | a siteswap that is not in ground state. Usually this refers only to the current rotation of the siteswap | `315` is excited state |
| **State** | Pattern of which future positions will receive throws. 1/x indicates an object landing there, 0/- indicates no object landing there. | `101` |
| **Orbit** | Path a single object takes through the pattern | Shows how one ball moves |
| **Transition** | Sequence of throws connecting two different siteswaps | From `531` to `441` |
| **Pass** | Throw to another juggler | `7` is a pass (single) |
| **Self** | Throw to yourself | `6` is a single self |
| **Global Siteswap** | A multi person view on a siteswap. Default convention for hand ordering is R R ... L L ... for all jugglers  | `96672` (2 jugglers). Every second throw is thrown from juggler A, every fourth throw from juggler As right hand. |
| **Local Siteswap** | One juggler's view of a multi-person pattern. Useful if you want to quickly see what you have to juggle | `4.5,3,1.5,3,3.5` from global `96672` |
| **Multiplex** | Multiple objects thrown from same hand simultaneously | `[33]23` |
| **Synchronous** | Both hands throw at same time | `(4,4)` |
| **Causal Diagram** | Visual representation showing why an object needs to be thrown |
| **State Graph** | Diagram showing all possible states and transitions between them. Closed loops on this graph are siteswaps. |

## Technical Terms

| Term | Definition |
|------|------------|
| **Backtracking** | Algorithm that explores possibilities by trying candidates and undoing invalid choices |
| **Filter** | Predicate that determines if a partial/complete siteswap meets constraints |
| **Early Pruning** | Rejecting invalid candidates before fully generating them |
| **Async Streaming** | Returning results incrementally as they are found |
| **Cyclic Array** | Data structure that wraps around, treating indices modulo length |

| **Property-Based Testing** | Testing by verifying mathematical properties hold for random inputs |

## Architecture Terms

| Term | Definition |
|------|------------|
| **MCP** | Model Context Protocol - standard for AI tool integration |
| **Blazor** | .NET framework for building interactive web UIs |
| **WebAssembly (WASM)** | Binary format enabling languages like C# to run in browsers |
| **Component** | Reusable UI element with its own logic and rendering |
| **Domain Model** | Core business logic independent of UI or infrastructure |
| **Immutable** | Objects that cannot be modified after creation |
| **Record** | C# type that implements value semantics and immutability by default |

## Acronyms

| Acronym | Full Form | Context |
|---------|-----------|---------|
| **MCP** | Model Context Protocol | AI integration |
| **WASM** | WebAssembly | Browser runtime |
| **UI** | User Interface | Presentation layer |
| **API** | Application Programming Interface | Library interface |
| **SVG** | Scalable Vector Graphics | Diagram format |
| **E2E** | End-to-End | Testing type |
| **ADR** | Architecture Decision Record | Documentation |

## Pattern Notation

| Notation | Meaning | Example |
|----------|---------|---------|
| `*` | Wildcard (any value) | `5**` matches `531`, `522`, etc. |
| `-1` | Pattern filter: Any value (wildcard) | `5,-1,1` = `5` at pos 0, any at pos 1, `1` at pos 2 → `501`, `511`, `521`, `531` |
| `-2` | Pattern filter: Any pass (multi-person patterns) | `-2,-3,-2` = pass, self, pass sequence |
| `-3` | Pattern filter: Any self (multi-person patterns) | `-3,-2,-3` = self, pass, self sequence |
| `.5` | Half beat (passing notation) | `4.5` is a crossing pass |
| `0` | Empty hand | `330` has one empty beat |
| `x` | Crossing throw | `51x` = crossing 1 throw |
| `p` | Pass | In interface notation |
| `s` | Self | In interface notation |
| `a-z` | Values 10-35 | `b` = 11, `c` = 12, etc. |

