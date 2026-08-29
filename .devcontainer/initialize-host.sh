#!/usr/bin/env bash
set -euo pipefail

# Dev Container hooks run on the host. Keep the environment-specific CA out of Git.
container_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
ca_source=/.msb/tls/ca.pem
feature_dir="$container_dir/features/host-ca"
staged_ca="$feature_dir/ca.pem"
runtime_dir="$container_dir/ca-runtime"
runtime_ca="$runtime_dir/ca.pem"

# Always leave deterministic paths behind. An absent, unreadable, or empty CA is
# intentionally represented by no payload and no runtime certificate.
install -d -m 0755 "$feature_dir" "$runtime_dir"
rm -f "$staged_ca"

# In a running container, the mounted runtime path can be the source itself.
# Do not remove that source before checking it.
same_runtime=false
if [[ -e "$ca_source" && -e "$runtime_ca" && "$ca_source" -ef "$runtime_ca" ]]; then
  same_runtime=true
else
  rm -f "$runtime_ca"
fi

if [[ -f "$ca_source" && -r "$ca_source" && -s "$ca_source" ]]; then
  install -m 0644 "$ca_source" "$staged_ca"
  if [[ "$same_runtime" == false ]]; then
    install -m 0644 "$ca_source" "$runtime_ca"
  fi
fi

# Preserve the existing non-secret host-auth setup used by the container.
auth_file="${HOME}/.hermes/auth.json"
if [[ ! -e "$auth_file" ]]; then
  install -D -m 0600 /dev/null "$auth_file"
fi
