#!/usr/bin/env bash
set -euo pipefail

if ! mkdir -p "${HOME}/.nuget/NuGet" 2>/dev/null; then
  sudo mkdir -p "${HOME}/.nuget/NuGet"
fi
sudo chown "$(id -u):$(id -g)" "${HOME}/.nuget" "${HOME}/.nuget/NuGet"

for cache_dir in "${NUGET_PACKAGES}" "${NUGET_HTTP_CACHE_PATH}"; do
  if ! mkdir -p "${cache_dir}" 2>/dev/null; then
    sudo mkdir -p "${cache_dir}"
  fi
  if [[ ! -w "${cache_dir}" ]]; then
    sudo chown -R "$(id -u):$(id -g)" "${cache_dir}"
  fi
done

bash .devcontainer/install-pi.sh
