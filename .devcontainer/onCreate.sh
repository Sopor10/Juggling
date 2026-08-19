#!/usr/bin/env bash
set -euo pipefail

# Lifecycle commands run with cwd = workspace folder (VS Code: /workspaces/...,
# JetBrains Gateway: /IdeaProjects/...). Avoid hard-coded paths.
if command -v sudo >/dev/null 2>&1; then
  sudo chown -R "$(id -u):$(id -g)" .
fi

# Keep the development container compatible with the SDK pinned in global.json.
DOTNETUP_DIR="${HOME}/.dotnetup"
GET_DOTNETUP="$(mktemp)"
trap 'rm -f "${GET_DOTNETUP}"' EXIT

curl --proto '=https' --tlsv1.2 -fsSL --retry 3 \
  https://aka.ms/dotnetup/get-dotnetup.sh \
  -o "${GET_DOTNETUP}"
bash "${GET_DOTNETUP}" --install-dir "${DOTNETUP_DIR}"
export PATH="${DOTNETUP_DIR}:${PATH}"

run_dotnetup_install() {
  "$@" install \
    --interactive false \
    --install-path /usr/share/dotnet \
    --set-default-install false \
    --untracked
}

if command -v sudo >/dev/null 2>&1 && [[ ! -w /usr/share/dotnet ]]; then
  run_dotnetup_install sudo env "PATH=${PATH}" "${DOTNETUP_DIR}/dotnetup"
else
  run_dotnetup_install "${DOTNETUP_DIR}/dotnetup"
fi

dotnet --info
dotnet workload restore || true
