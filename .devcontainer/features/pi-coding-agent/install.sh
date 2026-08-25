#!/usr/bin/env bash
set -euo pipefail

PI_PACKAGE="@earendil-works/pi-coding-agent"
PI_VERSION="${VERSION:-0.84.2}"
NVM_DIR="/usr/local/share/nvm"
export NVM_DIR
export PATH="${NVM_DIR}/current/bin:${PATH}"

if ! command -v node >/dev/null 2>&1 || ! command -v npm >/dev/null 2>&1; then
  echo "Pi requires Node.js and npm; install the Node.js feature first." >&2
  exit 1
fi

npm install --global --ignore-scripts "${PI_PACKAGE}@${PI_VERSION}"
PI_BIN="$(npm root --global)/${PI_PACKAGE}/dist/cli.js"
if [[ ! -x "${PI_BIN}" ]]; then
  echo "Pi package installed without an executable at ${PI_BIN}." >&2
  exit 1
fi

ln -sfn "${PI_BIN}" /usr/local/bin/pi
ln -sfn "${PI_BIN}" /usr/local/bin/pi-agent
npm cache clean --force >/dev/null 2>&1 || true
