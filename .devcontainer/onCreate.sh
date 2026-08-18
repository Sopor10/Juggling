#!/usr/bin/env bash
set -euo pipefail

# Lifecycle commands run with cwd = workspace folder (VS Code: /workspaces/...,
# JetBrains Gateway: /IdeaProjects/...). Avoid hard-coded paths.
if command -v sudo >/dev/null 2>&1; then
  sudo chown -R "$(id -u):$(id -g)" .
fi

# Keep the development container compatible with the SDK pinned in global.json.
curl -fsSL https://dot.net/v1/dotnet-install.sh | sudo bash -s -- \
  --version 10.0.400 \
  --install-dir /usr/share/dotnet \
  --skip-non-versioned-files

dotnet --info
dotnet workload restore || true
