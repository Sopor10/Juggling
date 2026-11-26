---
name: merge-pr
description: Take an explicitly requested pull request through automated checks to merge, fixing implementation and test issues that are technical and within the requested scope.
---

# Merge a pull request

Use this skill when the user asks to merge a PR, turn on auto-merge, or otherwise take a pull request through its checks and into the target branch. A short request such as “merge the PR” authorizes this workflow for the PR under discussion.

## Workflow

1. Identify the intended PR from the conversation, current branch, and repository. Confirm that it is open and targets the expected base branch. Read its description, changed-file summary, review comments, and current check results.
2. Review the complete PR diff against the user's request. Report unexpected changes. Stop before merging if they materially expand scope or cannot be justified by the request; explain what needs the user's decision. Report functional mistakes, unmet requirements, or conflicting product behavior rather than silently redefining them.
3. After the diff review is clear, enable auto-merge immediately, before waiting for checks. If the PR is a draft, mark it ready after confirming there are no unresolved scope or safety blockers. Run the deterministic helper from this skill directory:

   ```bash
   ./.agents/skills/merge-pr/enable-auto-merge.sh <PR_NUMBER> <EXPECTED_BASE> <METHOD>
   ```

   Choose the repository's configured normal merge method; if no default is configured, use `squash` when supported. The helper verifies the PR is open, targets the expected base, and is not a draft, then requests GitHub auto-merge. Auto-merge must wait for required reviews, checks, and branch protections; never bypass them. If auto-merge cannot be enabled, report the concrete blocker and continue resolving issues that are in scope.
4. Follow the required and relevant PR checks to completion. Inspect failure logs and classify each failure:
   - Fix code, build, formatting, infrastructure configuration, and other technical failures when they are within the request.
   - Update tests when implementation changes make an assertion stale, or when the request adds behavior that existing tests should cover. Add focused coverage for new behavior when appropriate.
   - Do not alter product behavior or weaken/remove meaningful tests merely to make a check pass. If a failure exposes a functional ambiguity or conflicting requirement, report it and pause that dependent change for the user's direction.
   - For clearly transient service failures, retry using the repository's normal mechanism and verify the new run. Do not repeatedly retry an unchanged deterministic failure.
5. Before each push, inspect the diff and working tree so unrelated or unexpected edits are not included. Keep fixes on the PR branch, push them, and let the new checks run; leave auto-merge enabled so GitHub merges automatically when all requirements are met. Continue until auto-merge completes or a specific external blocker requires user input.
6. Verify the final PR state is merged and report the PR link, merge commit or method when available, checks, and any remaining non-blocking observations.

## Boundaries

- A request to merge authorizes enabling auto-merge and completing the merge for that PR. It does not authorize merging unrelated PRs, deploying separately, bypassing protections, or changing unrelated product behavior.
- Stop and report if the PR contains unexpected changes, an unresolved functional conflict, required approval unavailable from the user, or a repeated/external blocker that cannot be fixed in the repository.
- Keep the user informed about meaningful failures, fixes, and blockers while the pipeline runs. Do not ask for routine confirmation when the PR and requested scope are clear.
