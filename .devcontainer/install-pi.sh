#!/usr/bin/env bash
set -euo pipefail

PI_PACKAGE="@earendil-works/pi-coding-agent"
PI_VERSION="0.84.2"
PI_AUTH_HELPER="/usr/local/lib/pi-codex-auth.mjs"

if ! command -v node >/dev/null 2>&1 || ! command -v npm >/dev/null 2>&1; then
  echo "Pi requires Node.js and npm; the devcontainer Node feature was not installed." >&2
  exit 1
fi

npm install --global --ignore-scripts "${PI_PACKAGE}@${PI_VERSION}"
PI_BIN="$(npm root --global)/${PI_PACKAGE}/dist/cli.js"
if [[ ! -x "${PI_BIN}" ]]; then
  echo "Pi package installed without an executable at ${PI_BIN}." >&2
  exit 1
fi

sudo install -m 0755 .devcontainer/pi-codex-auth.mjs "${PI_AUTH_HELPER}"
sudo ln -sfn "${PI_BIN}" /usr/local/bin/pi
sudo ln -sfn "${PI_BIN}" /usr/local/bin/pi-agent
sudo mkdir -p "${HOME}/.pi/agent"
sudo chown -R "$(id -u):$(id -g)" "${HOME}/.pi/agent"

# Convert the host's Hermes Codex login into Pi's auth format when one is available.
# CI creates an empty placeholder for the read-only mount but has no login.
if [[ -s /host-hermes-auth.json ]]; then
  node "${PI_AUTH_HELPER}" --force
else
  echo "No host Hermes Codex auth found; skipping Pi auth sync (Pi remains installed)."
fi

pi --version
