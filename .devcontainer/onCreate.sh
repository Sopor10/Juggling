#!/usr/bin/env bash
set -euo pipefail

# Lifecycle commands run with cwd = workspace folder (VS Code: /workspaces/...,
# JetBrains Gateway: /IdeaProjects/...). Avoid hard-coded paths.
if command -v sudo >/dev/null 2>&1; then
  sudo chown -R "$(id -u):$(id -g)" .
fi

dotnet --info
dotnet workload restore || true
