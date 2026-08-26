#!/usr/bin/env bash
set -euo pipefail

ASPIRE_VERSION="${VERSION:-13.5.2}"
ASPIRE_TOOL_PATH="/usr/local/share/aspire-cli"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Aspire CLI requires the .NET SDK feature to be installed first." >&2
  exit 1
fi

install -d -m 0755 "${ASPIRE_TOOL_PATH}"
dotnet tool install --tool-path "${ASPIRE_TOOL_PATH}" Aspire.Cli --version "${ASPIRE_VERSION}"
chmod 0755 "$(readlink -f "${ASPIRE_TOOL_PATH}/aspire")"
ln -sfn "${ASPIRE_TOOL_PATH}/aspire" /usr/local/bin/aspire

# Materialize the embedded bundle during the serialized feature build. The
# runtime verification keeps its setup call as a defensive idempotent check.
aspire setup --non-interactive --nologo

REMOTE_USER="${_REMOTE_USER:-vscode}"
REMOTE_GROUP="$(id -gn "${REMOTE_USER}")"
chown -R "${REMOTE_USER}:${REMOTE_GROUP}" "${ASPIRE_TOOL_PATH}"
ln -sfn "${ASPIRE_TOOL_PATH}/aspire" /usr/local/bin/aspire
