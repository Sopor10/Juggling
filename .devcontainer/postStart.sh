#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=export-dotnet.sh
source "${SCRIPT_DIR}/export-dotnet.sh"
export SSL_CERT_DIR="${SSL_CERT_DIR:-$HOME/.aspnet/dev-certs/trust:/etc/ssl/certs}"

# Prefer trusting the cert for OpenSSL clients; fall back to ensure a cert exists.
if ! dotnet dev-certs https --trust; then
  dotnet dev-certs https
fi
