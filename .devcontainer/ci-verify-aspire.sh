#!/usr/bin/env bash
# Verifies that Aspire can start inside the Dev Container and expose services.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export SSL_CERT_DIR="${SSL_CERT_DIR:-${HOME}/.aspnet/dev-certs/trust:/etc/ssl/certs}"

ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${ROOT}"

APPHOST="./Juggling.AppHost/Juggling.AppHost.csproj"
TIMEOUT_SECS="${ASPIRE_VERIFY_TIMEOUT_SECS:-480}"
export ASPIRE_CLI_START_TIMEOUT="${ASPIRE_CLI_START_TIMEOUT:-${TIMEOUT_SECS}}"
export features__updateNotificationsEnabled="${features__updateNotificationsEnabled:-false}"
POLL_SECS=5

run_output=""
run_pid=""

cleanup() {
  timeout 30s aspire stop --apphost "${APPHOST}" --non-interactive >/dev/null 2>&1 || true
  if [[ -n "${run_pid}" ]]; then
    kill "${run_pid}" 2>/dev/null || true
    wait "${run_pid}" 2>/dev/null || true
  fi
  [[ -z "${run_output}" ]] || rm -f "${run_output}" || true
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

fail_startup() {
  echo "$1" >&2
  echo "==> aspire run output" >&2
  cat "${run_output}" >&2 || true
  dump_aspire_logs || true
}

echo "==> dotnet build"
if ! dotnet build "${APPHOST}" --no-restore; then
  echo "dotnet build failed." >&2
  exit 1
fi

echo "==> aspire run"
run_output="$(mktemp)"
aspire run --apphost "${APPHOST}" --no-build --non-interactive --nologo >"${run_output}" 2>&1 &
run_pid=$!

echo "==> waiting for Aspire dashboard"
deadline=$((SECONDS + TIMEOUT_SECS))
while true; do
  if grep -Eq 'Dashboard:|Press CTRL\+C to stop the AppHost' "${run_output}"; then
    cat "${run_output}"
    break
  fi
  if ! kill -0 "${run_pid}" 2>/dev/null; then
    aspire_run_status=0
    wait "${run_pid}" || aspire_run_status=$?
    fail_startup "Aspire run failed (status ${aspire_run_status})."
    exit 1
  fi
  if (( SECONDS >= deadline )); then
    fail_startup "Timed out waiting for Aspire dashboard."
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
