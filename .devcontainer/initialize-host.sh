#!/usr/bin/env bash
set -euo pipefail

# Dev Container hooks run on the host. Keep the environment-specific CA out of Git.
container_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
ca_source=${MSB_TLS_CA_SOURCE:-/.msb/tls/ca.pem}
build_ca="$container_dir/ca.pem"
runtime_dir="$container_dir/ca-runtime"
runtime_ca="$runtime_dir/ca.pem"

# Always leave a valid build context and a deterministic runtime mount behind.
# An empty context means that the image is built without an extra trust anchor.
install -d -m 0755 "$runtime_dir"
: > "$build_ca"
rm -f "$runtime_ca"

if [[ -f "$ca_source" && -r "$ca_source" && -s "$ca_source" ]]; then
  install -m 0644 "$ca_source" "$build_ca"
  install -m 0644 "$ca_source" "$runtime_ca"
fi

# Preserve the existing non-secret host-auth setup used by the container.
auth_file="${HOME}/.hermes/auth.json"
if [[ ! -e "$auth_file" ]]; then
  install -D -m 0600 /dev/null "$auth_file"
fi
