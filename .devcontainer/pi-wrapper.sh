#!/usr/bin/env bash
set -euo pipefail

node /usr/local/lib/pi-codex-auth.mjs
exec /usr/local/bin/pi-real "$@"
