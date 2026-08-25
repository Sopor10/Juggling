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

# Aspire extracts its embedded DCP bundle on the first `start`/`run`. Warm it
# up in the image layer so the detached parent and child CLI processes cannot
# race on the bundle lock in a fresh CI or developer container.
WARMUP_DIR="$(mktemp -d)"
cleanup_warmup() {
  aspire stop --apphost "${WARMUP_DIR}/AspireWarmup.csproj" --non-interactive >/dev/null 2>&1 || true
  rm -rf "${WARMUP_DIR}"
}
trap cleanup_warmup EXIT

dotnet new console \
  --name AspireWarmup \
  --framework net10.0 \
  --output "${WARMUP_DIR}" \
  --no-restore \
  >/dev/null
python3 -c 'from pathlib import Path; import sys; d = Path(sys.argv[1]); csproj = d / "AspireWarmup.csproj"; csproj.write_text(csproj.read_text().replace("Microsoft.NET.Sdk", "Aspire.AppHost.Sdk/13.5.2")); (d / "Program.cs").write_text("var builder = DistributedApplication.CreateBuilder(args); builder.Build().Run();\n")' "${WARMUP_DIR}"
aspire start \
  --apphost "${WARMUP_DIR}/AspireWarmup.csproj" \
  --non-interactive \
  --nologo \
  --format Json \
  >/dev/null

REMOTE_USER="${_REMOTE_USER:-vscode}"
REMOTE_GROUP="$(id -gn "${REMOTE_USER}")"
chown -R "${REMOTE_USER}:${REMOTE_GROUP}" "${ASPIRE_TOOL_PATH}"
ln -sfn "${ASPIRE_TOOL_PATH}/aspire" /usr/local/bin/aspire
