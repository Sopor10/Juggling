#!/usr/bin/env bash
set -euo pipefail

for cache_dir in "${NUGET_PACKAGES}" "${NUGET_HTTP_CACHE_PATH}"; do
  if ! mkdir -p "${cache_dir}" 2>/dev/null; then
    sudo mkdir -p "${cache_dir}"
  fi
  if [[ ! -w "${cache_dir}" ]]; then
    sudo chown -R "$(id -u):$(id -g)" "${cache_dir}"
  fi
done

dotnet restore
aspire --version
bash .devcontainer/install-pi.sh
