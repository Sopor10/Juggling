#!/usr/bin/env bash
set -euo pipefail

ca_source="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)/ca.pem"
ca_target=/usr/local/share/ca-certificates/host-ca.crt

# The payload is prepared by initialize-host.sh and is intentionally optional.
if [[ ! -f "$ca_source" || ! -r "$ca_source" || ! -s "$ca_source" ]]; then
  exit 0
fi

# Do not alter the trust store unless the staged payload is an X.509 certificate.
if ! openssl x509 -in "$ca_source" -noout >/dev/null 2>&1; then
  exit 0
fi

install -D -m 0644 "$ca_source" "$ca_target"
update-ca-certificates
