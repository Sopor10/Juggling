#!/usr/bin/env bash
# Verifies that Aspire can start inside the Dev Container and expose services.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export SSL_CERT_DIR="${SSL_CERT_DIR:-${HOME}/.aspnet/dev-certs/trust:/etc/ssl/certs}"

ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${ROOT}"

APPHOST="./Juggling.AppHost/Juggling.AppHost.csproj"
TIMEOUT_SECS="${ASPIRE_VERIFY_TIMEOUT_SECS:-480}"
PREWARM_TIMEOUT_SECS=60
POLL_SECS=5
prewarm_pid=""

process_tree() {
  local pid="$1"
  local child

  printf '%s\n' "${pid}"
  while IFS= read -r child; do
    process_tree "${child}"
  done < <(pgrep -P "${pid}" 2>/dev/null || true)
}

terminate_process_tree() {
  local root_pid="$1"
  local signal
  local pid
  local end
  local any_alive
  local -a pids

  mapfile -t pids < <(process_tree "${root_pid}")
  for signal in INT TERM KILL; do
    for pid in "${pids[@]}"; do
      kill -"${signal}" "${pid}" 2>/dev/null || true
    done

    if [[ "${signal}" == KILL ]]; then
      break
    fi

    end=$((SECONDS + 5))
    while (( SECONDS < end )); do
      any_alive=false
      for pid in "${pids[@]}"; do
        if kill -0 "${pid}" 2>/dev/null; then
          any_alive=true
          break
        fi
      done
      if [[ "${any_alive}" == false ]]; then
        return 0
      fi
      sleep 1
    done
  done
}

cleanup() {
  if [[ -n "${prewarm_pid}" ]] && kill -0 "${prewarm_pid}" 2>/dev/null; then
    terminate_process_tree "${prewarm_pid}" || true
    wait "${prewarm_pid}" 2>/dev/null || true
  fi
  timeout 30s aspire stop --apphost "${APPHOST}" --non-interactive >/dev/null 2>&1 || true
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

echo "==> Aspire AppHost prewarm"
prewarm_output="$(mktemp)"
ASPIRE_SUPPRESS_CLI_RUN_HOOK=true dotnet run --project "${APPHOST}" --no-launch-profile >"${prewarm_output}" 2>&1 &
prewarm_pid=$!
prewarm_deadline=$((SECONDS + PREWARM_TIMEOUT_SECS))
prewarm_message_found=false
prewarm_timed_out=false
prewarm_exit=0

while true; do
  if grep -Fq 'Distributed application started. Press Ctrl+C to shut down.' "${prewarm_output}"; then
    prewarm_message_found=true
    break
  fi
  if ! kill -0 "${prewarm_pid}" 2>/dev/null; then
    if wait "${prewarm_pid}"; then
      prewarm_exit=$?
    else
      prewarm_exit=$?
    fi
    break
  fi
  if (( SECONDS >= prewarm_deadline )); then
    prewarm_timed_out=true
    prewarm_exit=124
    break
  fi
  sleep 1
done

if [[ -n "${prewarm_pid}" ]] && kill -0 "${prewarm_pid}" 2>/dev/null; then
  terminate_process_tree "${prewarm_pid}" || true
fi
if [[ "${prewarm_timed_out}" == true ]]; then
  prewarm_exit=124
elif kill -0 "${prewarm_pid}" 2>/dev/null; then
  if wait "${prewarm_pid}"; then
    prewarm_exit=$?
  else
    prewarm_exit=$?
  fi
elif [[ "${prewarm_message_found}" == true ]]; then
  if wait "${prewarm_pid}"; then
    prewarm_exit=$?
  else
    prewarm_exit=$?
  fi
fi
prewarm_pid=""

cat "${prewarm_output}"
if [[ "${prewarm_message_found}" != true ]]; then
  echo "Timed out waiting for the Aspire AppHost startup message." >&2
  cat "${prewarm_output}" >&2
  dump_aspire_logs
  rm -f "${prewarm_output}"
  exit 1
fi
case "${prewarm_exit}" in
  0|124|130|143) ;;
  *)
    echo "Aspire AppHost prewarm exited with unexpected status ${prewarm_exit}." >&2
    cat "${prewarm_output}" >&2
    dump_aspire_logs
    rm -f "${prewarm_output}"
    exit 1
    ;;
esac
rm -f "${prewarm_output}"

if ! timeout 30s aspire stop --apphost "${APPHOST}" --non-interactive >/dev/null 2>&1; then
  echo "Failed to stop the prewarmed Aspire AppHost." >&2
  dump_aspire_logs
  exit 1
fi

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
