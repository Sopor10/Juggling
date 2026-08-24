# Review Perspective Catalog

Select perspectives from the actual change risks, not from a fixed checklist.
Every round has at least three independent reviewers. Three to seven is the
normal range; use more only when each additional assignment names a distinct
risk.

## Perspective library

this is a list of examples, and does not have to be exhaustive.
Feel free to add new perspectives, either temporarily for a single run or permanently into the library.

### Domain and algorithm changes

- **Correctness and invariants**: mathematical/domain contracts, rotations,
  filter semantics, deterministic output.
- **Adversarial boundaries**: empty/min/max input, malformed input, overflow,
  cancellation, timeout, impossible combinations.
- **State and integration**: translation between UI/MCP inputs and core domain,
  lifecycle, repeated execution, stale state.
- **Performance and complexity**: backtracking explosion, allocations, repeated
  enumeration, cancellation responsiveness.
- **Architecture and testability**: dependency direction, duplicate concepts,
  stable public contracts, regression coverage.

### UI and browser changes

- **Functional browser behavior**: complete happy path, controls, routing,
  results, validation, reload/back/repeat.
- **Usability and recovery**: clarity, affordances, disabled/loading/error/empty
  states, cancellation, recoverability.
- **Visual consistency and responsive layout**: established components and
  tokens, hierarchy, spacing, clipping, touch layouts.
- **Accessibility and localization**: keyboard/focus, names/roles/states,
  announcements, contrast where observable, English/German parity.
- **Resilience and state transitions**: console/network failures, rapid input,
  double activation, stale state, history, direct URLs, cache/reload.

### MCP, infrastructure, and delivery changes

- **Contract compatibility**: inputs, outputs, validation, errors, backward
  compatibility.
- **Architecture boundaries**: project references, Core/UI separation,
  ownership of state and configuration.
- **Operational resilience**: startup, health, ports, cancellation, logs,
  service-worker/cache, container behavior.
- **CI and environment parity**: SDK/workload versions, deterministic commands,
  local versus CI behavior.
- **Security**: only when the diff changes trust boundaries, external input,
  secrets, authentication, authorization, or executable content.

## Selection rules

1. Select the highest-risk perspective first.
2. Add perspectives that could find a different class of failure.
3. Use at least three reviewers even for a narrow change.
4. If fewer than three distinct perspectives are relevant, assign an
   independent second opinion for the highest-risk perspective.
5. A second-opinion reviewer receives the same scope and contract but must not
   receive the first reviewer's findings.
6. Browser review is required only when behavior is user-visible or depends on
   a real browser.
7. Do not send a pure logic change to Playwright.
8. Do not use a browser reviewer as a substitute for focused deterministic
   tests.

## Example batches

### Pure filter change

1. Correctness and invariants
2. Adversarial boundaries
3. Performance and cancellation
4. Architecture and testability when public contracts changed

### Razor/CSS-only change

1. Functional browser behavior
2. Visual consistency and responsive layout
3. Accessibility and localization
4. Usability and recovery

### Generation workflow change

1. Workflow/state invariants
2. Cancellation and race conditions
3. Functional browser behavior
4. Adversarial navigation and reload
5. Accessibility/localization when controls or text changed

### AppHost or service-worker change

1. Operational resilience
2. CI/environment parity
3. Browser cache and startup behavior
4. Architecture boundaries
