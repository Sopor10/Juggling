#!/usr/bin/env bash
# Verifies that Aspire can start inside the Dev Container and expose services.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export SSL_CERT_DIR="${SSL_CERT_DIR:-${HOME}/.aspnet/dev-certs/trust:/etc/ssl/certs}"

ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${ROOT}"

APPHOST="./Juggling.AppHost/Juggling.AppHost.csproj"
TIMEOUT_SECS="${ASPIRE_VERIFY_TIMEOUT_SECS:-480}"
POLL_SECS=5

cleanup() {
  aspire stop --apphost "${APPHOST}" --non-interactive >/dev/null 2>&1 || true
}
trap cleanup EXIT

dump_aspire_logs() {
  echo "==> Aspire CLI logs" >&2
  find "${HOME}/.aspire/logs" -maxdepth 1 -type f -printf "%T@ %p\n" 2>/dev/null \
    | sort -nr \
    | cut -d' ' -f2- \
    | while IFS= read -r log_file; do
        echo "--- ${log_file} (last 200 lines)" >&2
        tail -200 "${log_file}" >&2 || true
      done
}

echo "==> prewarm Aspire CLI bundle"
prewarm_output="$(mktemp)"
if ! aspire run --apphost "${APPHOST}" --non-interactive --nologo --detach >"${prewarm_output}" 2>&1; then
  cat "${prewarm_output}" >&2
  dump_aspire_logs
  exit 1
fi
if ! aspire stop --apphost "${APPHOST}" --non-interactive >>"${prewarm_output}" 2>&1; then
  cat "${prewarm_output}" >&2
  dump_aspire_logs
  exit 1
fi
cat "${prewarm_output}"
rm -f "${prewarm_output}"

echo "==> aspire start"
# Keep a supervising shell process for the duration of this script so orphan
# detection does not race the short-lived `aspire start` parent.
start_output="$(mktemp)"
if ! aspire start --apphost "${APPHOST}" --non-interactive --nologo --format Json 2>&1 | tee "${start_output}"; then
  echo "Failed to start the Aspire AppHost." >&2
  cat "${start_output}" >&2
  dump_aspire_logs
  exit 1
fi
rm -f "${start_output}"

echo "==> waiting for AppHost (timeout ${TIMEOUT_SECS}s)"
deadline=$((SECONDS + TIMEOUT_SECS))
while true; do
  if aspire ps --format Json 2>/dev/null | grep -q '"status": "running"'; then
    echo "AppHost is running"
    break
  fi
  if (( SECONDS >= deadline )); then
    echo "Timed out waiting for AppHost" >&2
    aspire ps --format Json || true
    ls -lt "${HOME}/.aspire/logs" 2>/dev/null | head -20 || true
    exit 1
  fi
  sleep "${POLL_SECS}"
done

echo "==> waiting for Healthy resources"
deadline=$((SECONDS + TIMEOUT_SECS))
while true; do
  describe="$(aspire describe --apphost "${APPHOST}" --format Json 2>/dev/null || true)"
  web_ok="$(echo "${describe}" | grep -c '"displayName": "Webassembly"' || true)"
  mcp_ok="$(echo "${describe}" | grep -c '"displayName": "McpServer"' || true)"
  healthy="$(echo "${describe}" | grep -c '"healthStatus": "Healthy"' || true)"
  if [[ "${web_ok}" -ge 1 && "${mcp_ok}" -ge 1 && "${healthy}" -ge 2 ]]; then
    echo "Webassembly and McpServer are Healthy"
    break
  fi
  if (( SECONDS >= deadline )); then
    echo "Timed out waiting for Healthy resources" >&2
    echo "${describe}" >&2
    exit 1
  fi
  sleep "${POLL_SECS}"
done

echo "==> HTTP checks"
curl -fsS -o /dev/null -w "wizard:%{http_code}\n" http://localhost:7021/wizard
curl -fsS -o /dev/null -w "cardstack:%{http_code}\n" http://localhost:7021/cardstack

echo "==> Dev Container Aspire verification succeeded"
