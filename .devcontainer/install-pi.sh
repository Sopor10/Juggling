#!/usr/bin/env bash
set -euo pipefail

PI_PACKAGE="@earendil-works/pi-coding-agent"
PI_VERSION="0.84.2"
PI_REAL_BIN="/usr/local/bin/pi-real"
PI_AUTH_HELPER="/usr/local/lib/pi-codex-auth.mjs"
PI_WRAPPER="/usr/local/bin/pi"

if ! command -v node >/dev/null 2>&1 || ! command -v npm >/dev/null 2>&1; then
  echo "Pi requires Node.js and npm; the devcontainer Node feature was not installed." >&2
  exit 1
fi

PI_PACKAGE_ROOT="$(npm root --global)/@earendil-works/pi-coding-agent"
npm install --global --ignore-scripts "${PI_PACKAGE}@${PI_VERSION}"
sudo mkdir -p "${HOME}/.pi/agent"
sudo chown -R "$(id -u):$(id -g)" "${HOME}/.pi/agent"

PI_INSTALLED_BIN="${PI_PACKAGE_ROOT}/dist/cli.js"
if [[ ! -x "${PI_INSTALLED_BIN}" ]]; then
  echo "Pi package installed without an executable at ${PI_INSTALLED_BIN}." >&2
  exit 1
fi
sudo ln -sfn "${PI_INSTALLED_BIN}" "${PI_REAL_BIN}"

sudo install -m 0755 .devcontainer/pi-codex-auth.mjs "${PI_AUTH_HELPER}"
sudo install -m 0755 .devcontainer/pi-wrapper.sh "${PI_WRAPPER}"
sudo install -m 0755 .devcontainer/pi-codex-sync.sh /usr/local/bin/pi-codex-sync
sudo ln -sfn "${PI_REAL_BIN}" /usr/local/bin/pi-agent

# Convert the host's Codex login into Pi's auth format. No login prompt is used.
pi-codex-sync

pi --version
