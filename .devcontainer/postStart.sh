#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet/tools:$PATH"
export SSL_CERT_DIR="${SSL_CERT_DIR:-$HOME/.aspnet/dev-certs/trust:/etc/ssl/certs}"

# Prefer trusting the cert for OpenSSL clients; fall back to ensure a cert exists.
if ! dotnet dev-certs https --trust; then
  dotnet dev-certs https
fi
