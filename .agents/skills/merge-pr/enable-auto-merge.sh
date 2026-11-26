#!/usr/bin/env bash
set -euo pipefail

usage() {
  echo "Usage: $0 <PR_NUMBER> <EXPECTED_BASE> <METHOD: merge|squash|rebase>" >&2
  exit 2
}

[[ $# -eq 3 ]] || usage

pr_number=$1
expected_base=$2
method=$3

[[ $pr_number =~ ^[1-9][0-9]*$ ]] || usage
[[ -n $expected_base ]] || usage
case $method in
  merge | squash | rebase) ;;
  *) usage ;;
esac

repo_root=$(git rev-parse --show-toplevel)
cd "$repo_root"

pr=$(gh pr view "$pr_number" --json state,baseRefName,isDraft,url --jq '[.state, .baseRefName, (.isDraft | tostring), .url] | @tsv')
IFS=$'\t' read -r state base_ref is_draft url <<< "$pr"

if [[ $state != OPEN ]]; then
  echo "PR #$pr_number is $state, expected OPEN: $url" >&2
  exit 1
fi
if [[ $base_ref != "$expected_base" ]]; then
  echo "PR #$pr_number targets '$base_ref', expected '$expected_base': $url" >&2
  exit 1
fi
if [[ $is_draft == true ]]; then
  echo "PR #$pr_number is still a draft; review it and mark it ready before enabling auto-merge: $url" >&2
  exit 1
fi

gh pr merge "$pr_number" --auto "--$method"

after=$(gh pr view "$pr_number" --json state,autoMergeRequest,url --jq '[.state, (.autoMergeRequest != null | tostring), .url] | @tsv')
IFS=$'\t' read -r final_state auto_merge_enabled final_url <<< "$after"

if [[ $final_state == MERGED ]]; then
  echo "PR #$pr_number merged using $method: $final_url"
elif [[ $auto_merge_enabled == true ]]; then
  echo "Auto-merge enabled using $method; waiting for GitHub requirements: $final_url"
else
  echo "GitHub did not enable auto-merge for PR #$pr_number: $final_url" >&2
  exit 1
fi
