#!/usr/bin/env bash
set -euo pipefail

PI_AUTH_HELPER="/usr/local/lib/pi-codex-auth.mjs"

if ! command -v node >/dev/null 2>&1 || ! command -v pi >/dev/null 2>&1; then
  echo "Pi requires the Node.js feature and the baked Pi coding agent." >&2
  exit 1
fi

sudo install -m 0755 .devcontainer/pi-codex-auth.mjs "${PI_AUTH_HELPER}"
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
