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
# Install into the image's shared SDK directory; the direct feed avoids the
# aka.ms redirect that is blocked in restricted network environments.
DOTNET_ROOT="/usr/share/dotnet"
export DOTNET_ROOT
DOTNET_INSTALL_SCRIPT="$(mktemp)"
trap 'rm -f "${DOTNET_INSTALL_SCRIPT}"' EXIT

curl --proto '=https' --tlsv1.2 -fsSL --retry 3 \
  https://dot.net/v1/dotnet-install.sh \
  -o "${DOTNET_INSTALL_SCRIPT}"
bash "${DOTNET_INSTALL_SCRIPT}" \
  --version 10.0.400 \
  --install-dir "${DOTNET_ROOT}" \
  --azure-feed https://builds.dotnet.microsoft.com/dotnet \
  --skip-non-versioned-files

export PATH="${DOTNET_ROOT}:${HOME}/.dotnet/tools:${PATH}"
dotnet --info
 dotnet workload restore || true
