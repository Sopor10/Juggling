---
name: adaptive-review-loop
description: Proposes and, only after explicit user confirmation, orchestrates iterative implementation, parallel independent reviews, red regression tests, fixes, commits, and convergence. Use when substantial or high-risk work may benefit from several reviewers, repeated feedback rounds, or a builder-critic workflow.
disable-model-invocation: false
---

# Adaptive Review Loop

Run a bounded builder-critic loop for this repository. The parent agent is the
coordinator, implementer, triager, and fixer. Use fresh generic subagents for
independent reviews and regression-test authoring; do not require custom
subagent definitions.

Read before starting:

- [Perspective catalog](perspectives.md)
- [Generic subagent prompts](subagent-prompts.md)
- [Review contract](review-contract.md)

## Explicit confirmation gate

Model invocation means "consider and propose this workflow," not "start it."
Never begin the loop merely because this skill was automatically selected.

Before any implementation, subagent launch, Aspire lifecycle action, test
mutation, or commit:

1. Gather only the minimal read-only context needed to make a useful proposal.
2. Tell the user why this change may benefit from the adaptive loop.
3. Propose the initial reviewer count and perspectives, maximum rounds, browser
   usage, model policy, and commit checkpoints.
4. Ask the user explicitly whether to start the proposed loop.
5. Wait for an affirmative answer.

Use a structured confirmation question when available. A prior general request
to implement or review work is not confirmation for this multi-agent loop.
Even an explicit skill invocation must show the proposed configuration and
receive a separate confirmation before execution.

If the user declines or does not confirm, continue with the original task
without this loop. Do not repeatedly propose it in the same task.

## Non-negotiable rules

- Preserve unrelated user changes in the working tree.
- Create one commit after every non-empty mutating coordinator checkpoint:
  implementation, confirmed red regression tests, and the green fix.
- Never include unrelated changes, generated browser artifacts, build outputs,
  logs, screenshots, secrets, or credentials in a checkpoint commit.
- Store every workflow artifact under the gitignored `review-artifacts/`
  directory and never force-add it to Git.
- Run all coordinator and subagent work locally in the same checkout unless the
  user explicitly changes the execution model.
- Use English for code and repository artifacts.
- Run deterministic checks before probabilistic review.
- Launch at least three independent reviewers per round.
- Launch a review batch concurrently, not one reviewer after another.
- Do not expose one reviewer's findings to another reviewer in the same round.
- Add a regression test before fixing every accepted reproducible defect when
  a reliable automated test is sensible.
- Stop on convergence; do not invent findings to consume a round budget.

## Inputs and defaults

Determine from the request:

- implementation goal and acceptance criteria;
- existing implementation versus work still to build;
- scope of the current diff;
- explicit model requirements;
- requested round limit.

If the user does not specify a round limit, use at most five rounds. Stop
earlier after one complete clean review batch with no accepted material
findings and green deterministic gates.

If scope is ambiguous enough to change which product behavior may be edited,
ask before implementation. Do not ask about choices that can be derived from
the code, repository conventions, or this skill.

## Commit checkpoints

The coordinator commits after each non-empty mutating checkpoint, not merely at
the end of a round:

1. **Implementation checkpoint** after the requested implementation passes its
   applicable deterministic gates.
2. **Red-test checkpoint** after the independent test author has added tests
   and demonstrated that they fail for the intended behavioral reason.
3. **Fix checkpoint** after the production fix makes the new tests and required
   affected suites green.

The red-test checkpoint is the only intentional exception to the green-gate
rule. Its commit message must make the regression-test intent clear, and the
evidence pack must record the exact expected failing command and reason. Do not
commit setup failures, compilation failures, flaky tests, or unrelated failing
tests as a red checkpoint.

At every checkpoint:

1. Inspect git status, staged and unstaged diffs, and recent commit-message
   style.
2. Verify every file and hunk belongs to the checkpoint's scope.
3. Stage only the checkpoint's implementation, tests, or intentional supporting
   changes.
4. If unrelated and in-scope changes share a file and cannot be separated
   safely, ask the user before staging that file.
5. Create one non-empty commit with a concise message that explains the
   checkpoint's purpose and follows repository style.
6. Verify success, capture the commit hash, and confirm unrelated changes
   remain untouched.
7. Append the checkpoint name, commit hash, and gate status to the current
   round's `checkpoints.md`.

Do not create empty commits for read-only review, triage, or verification
steps.

## Phase 1: Establish the baseline

1. Inspect git status and the relevant diff.
2. Separate the requested change from unrelated, generated, and pre-existing
   working-tree files.
3. Create a local run directory:
   `review-artifacts/<yyyyMMdd-HHmmss>-<short-goal>/`.
4. Write `manifest.md` with the user goal, scope, baseline, requested model
   policy, maximum rounds, and proposed reviewer assignments.
5. Record the round baseline so later staging includes only in-scope work.
6. Read the affected code, tests, and architecture contracts.
7. Classify the change using `perspectives.md`.
8. Record the focused deterministic checks for the affected projects.

Useful project mapping:

- `Siteswaps.Test` for `Siteswap.Details`.
- `Generator/Siteswaps.Generator.Test` for generator, filters, workflow, state,
  feeding, and generator-component contracts.
- `Siteswaps.Mcp.Server.Test` for MCP and Filter DSL.
- `Siteswaps.E2ETests` for browser-observable behavior.
- Architecture analyzers and architecture tests for dependency changes.

Ignore `.playwright-cli/`, `bin/`, `obj/`, logs, screenshots, and
`review-artifacts/` when classifying or staging the product change.

Use this local artifact structure:

```text
review-artifacts/<run-id>/
├── manifest.md
├── round-01/
│   ├── reviewer-01-<perspective>/
│   │   ├── report.md
│   │   └── playwright/
│   ├── reviewer-02-<perspective>/
│   ├── triage.md
│   ├── test-author.md
│   ├── implementation-tests.log
│   ├── red-tests.log
│   ├── green-tests.log
│   └── checkpoints.md
└── final-evidence.md
```

Create only the directories needed by the selected reviewers. Store reviewer
prompts, reports, screenshots, snapshots, traces, console/network output,
triage, test output, and checkpoint hashes in this tree.

## Phase 2: Implement and gate

The parent agent implements the requested change or the accepted findings from
the previous round.

Then:

1. Run the narrowest relevant build/test/format checks.
2. Fix deterministic failures introduced by the current change before spending
   reviewer contexts.
3. Do not hide baseline failures. Distinguish them from introduced failures.
4. Keep the implementation scope narrow.
5. Save relevant gate output as `implementation-tests.log`.
6. Create the implementation checkpoint commit when this phase changed files.

## Phase 3: Prepare runtime review when needed

Skip this phase for pure logic, documentation, or non-browser contracts.

For browser-visible or cross-service behavior:

1. Read and follow `../aspire-cli/SKILL.md`.
2. Ensure the Aspire AppHost serves the latest implementation. Reuse a healthy
   instance only when it already contains the current build; otherwise stop it
   cleanly and restart once.
3. Resolve the current Webassembly URL dynamically with Aspire. Never hardcode
   a port.
4. Give all browser reviewers the same application build and URL.
5. Give every reviewer a unique Playwright session and a unique output
   directory under its assigned `review-artifacts/<run-id>/<round>/reviewer-*`
   directory.

Browser reviewers must read
`../../../.claude/skills/playwright-cli/SKILL.md`, run headless, and must not
start, stop, restart, or reconfigure Aspire themselves. This restriction
applies to every reviewer, including infrastructure reviewers. They may inspect
Aspire status, application logs, traces, and metrics read-only.

## Phase 4: Select and launch reviewers

Select at least three assignments from `perspectives.md`. Three to seven is
typical. Use more only when each extra assignment names a distinct risk.

If fewer than three distinct perspectives are relevant, use an independent
second opinion for the highest-risk perspective. A duplicated perspective must
run in a fresh context without seeing the first result.

For every assignment:

1. Fill the reviewer contract in `subagent-prompts.md`.
2. Include all required context; generic subagents do not inherit this chat.
3. Select an available model appropriate to the assignment.
4. Honor an explicit user model choice over coordinator preference.
5. When no override is useful, use `inherit`.
6. Assign a unique local artifact directory and require the reviewer to save
   its report and all generated evidence there.

Launch all generic reviewer subagents locally in the same checkout and in one
concurrent batch. If the current
interaction mode requires background execution, launch the entire batch in the
background and wait for completion notifications without polling. Do not begin
triage until every reviewer has returned or definitively failed.

Record the requested and actual model when the tool reports it. Do not claim a
specific model ran when Cursor applied a plan, availability, or admin fallback.

## Phase 5: Triage

The parent agent owns triage; do not delegate acceptance blindly.

For every finding:

1. Merge duplicates while retaining independent evidence.
2. Check that reproduction, expected behavior, actual behavior, and impact are
   concrete.
3. Reproduce disputed or high-severity findings when practical.
4. Separate defects from subjective recommendations.
5. Separate relevant new regressions from unrelated pre-existing issues.
6. Resolve conflicting reports with direct evidence or a fresh second opinion.

Write the complete triage decision to the current round's `triage.md`.

Accept a finding for automatic test/fix work only when it is in scope,
reproducible, and violates a defensible contract. Blocker, High, and Medium are
material. Accept Low only when objective, directly in scope, and cheap to fix
without churn.

## Phase 6: Establish red regression tests

When accepted defects exist, start one fresh local generic subagent in the same
checkout using the
regression-test-author contract in `subagent-prompts.md`.

The test author must:

- edit tests and fixtures only;
- choose the fastest reliable level that proves the behavior;
- execute the narrowest command;
- demonstrate red for the intended behavioral reason;
- decline brittle or purely subjective tests.

If accepted findings span unrelated test projects, process the clusters
sequentially unless parallel edits are guaranteed not to conflict.

Do not proceed to a production fix when a claimed red test fails only because
of setup, compilation, timing, or environment. Correct the test contract or
reject the finding first.

After the coordinator validates the intended red result, create the red-test
checkpoint commit before changing production code. Save the focused failing
test output as `red-tests.log` in the current round directory.

## Phase 7: Fix and verify

The parent agent fixes the production behavior against the established test
contract.

1. Do not weaken the new test to accommodate the implementation.
2. Run the new focused tests until green.
3. Run the affected project's complete test suite.
4. Run broader format/build/test gates in proportion to the change risk.
5. For browser changes, ensure the next review round receives a freshly built
   Aspire instance.
6. Save focused green output as `green-tests.log`.
7. Create the green fix checkpoint commit.

## Phase 8: Repeat or stop

Start a new round with fresh reviewer contexts after the green fix checkpoint.

Stop when:

- one complete batch reports no accepted material findings;
- relevant deterministic gates are green; and
- no blocker prevents required verification.

Also stop at the user-specified limit or the default maximum of five rounds.
Reaching the limit with open material findings is not success; report the
remaining risk.

## Final evidence pack

Write `final-evidence.md` in the run directory and report:

- rounds completed and why the loop stopped;
- reviewer assignments and actual models when known;
- accepted, rejected, duplicate, and deferred findings;
- regression tests added and their red-to-green evidence;
- focused and broader gates run;
- browser routes/viewports covered when applicable;
- one commit hash per non-empty implementation, red-test, and fix checkpoint;
- remaining risks, blockers, and manual checks;
- files changed by the workflow.

Keep the report concise enough for a human to verify.
