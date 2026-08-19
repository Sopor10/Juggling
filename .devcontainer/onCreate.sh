#!/usr/bin/env bash
set -euo pipefail

# Lifecycle commands run with cwd = workspace folder (VS Code: /workspaces/...,
# JetBrains Gateway: /IdeaProjects/...). Avoid hard-coded paths.
if command -v sudo >/dev/null 2>&1; then
  sudo chown -R "$(id -u):$(id -g)" .
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=export-dotnet.sh
source "${SCRIPT_DIR}/export-dotnet.sh"

# Keep the development container compatible with the SDK pinned in global.json.
# dotnetup refuses /usr/share/dotnet (system-managed); install user-local instead.
DOTNETUP_DIR="${HOME}/.dotnetup"
GET_DOTNETUP="$(mktemp)"
trap 'rm -f "${GET_DOTNETUP}"' EXIT

curl --proto '=https' --tlsv1.2 -fsSL --retry 3 \
  https://aka.ms/dotnetup/get-dotnetup.sh \
  -o "${GET_DOTNETUP}"
bash "${GET_DOTNETUP}" --install-dir "${DOTNETUP_DIR}"
export PATH="${DOTNETUP_DIR}:${PATH}"

"${DOTNETUP_DIR}/dotnetup" install \
  --interactive false \
  --install-path "${DOTNET_ROOT}" \
  --set-default-install false \
  --untracked

dotnet --info
dotnet workload restore || true
