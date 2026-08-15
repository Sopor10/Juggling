# Review Contract

Every reviewer receives one bounded scope and one primary perspective.

## Independence

- Work from the code, diff, requirements, and runtime evidence.
- Do not read other reviewers' reports before submitting yours.
- Do not assume implementation claims are correct.
- Do not edit product code, tests, configuration, or documentation.
- The only workflow files you may create or change are inside your assigned
  `review-artifacts/<run-id>/<round>/reviewer-*` directory.
- Do not start, stop, restart, or reconfigure Aspire or any application
  resource. The coordinator provides an already-running current instance.
- You may inspect Aspire status, application logs, traces, and metrics
  read-only. Do not clear, delete, or otherwise mutate observability data. Save
  copies only inside your assigned review-artifact directory.
- Reviewers share one local checkout. Do not run restore, build, format,
  snapshot update, or other commands that rewrite shared outputs. Use gate
  results provided by the coordinator. If an additional test is essential and
  safe, run it with no restore/build and direct its result files into your
  assigned artifact directory; otherwise request coordinator verification in
  the report.
- Never commit.

## Evidence threshold

Report a defect only when it has:

- a reproducible observation;
- a clear violated contract or established product precedent;
- concrete user, correctness, operability, or maintenance impact;
- enough evidence for another agent to verify it.

Prefer focused test output, exact code paths, route and interaction steps,
console/network output, snapshots, screenshots, or traces. Subjective
preferences without a defensible contract are recommendations, not defects.
Pre-existing issues outside the assigned scope must not become automatic fix
work.

## Severity

- **Blocker**: the primary flow cannot complete, data is corrupted, or the
  application cannot operate safely.
- **High**: important behavior is wrong or inaccessible without a reasonable
  workaround.
- **Medium**: behavior is materially confusing, fragile, inconsistent, or
  incomplete while the main flow remains usable.
- **Low**: an objective minor defect with limited impact.

## Required output

```markdown
## Review
- Perspective:
- Scope:
- Evidence examined:

## Findings
### F-01: [short title]
- Severity: Blocker | High | Medium | Low
- Confidence: High | Medium | Low
- Scope: [file, route, component, or contract]
- Reproduction:
  1. ...
- Expected:
- Actual:
- Impact:
- Evidence:
- Suggested regression level: Unit | Snapshot | E2E | None
- Recommendation:

## No-finding checks
- [important checks that passed]

## Unverified risks
- [risk and why it could not be verified]
```

Use stable finding IDs within the report. If there are no findings, say so
explicitly and list the important checks that passed. Save the report as
`report.md` in the assigned artifact directory and return the same content to
the coordinator.
