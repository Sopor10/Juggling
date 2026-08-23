#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=export-dotnet.sh
source "${SCRIPT_DIR}/export-dotnet.sh"

# Match AppHost Aspire major (see Juggling.AppHost.csproj Aspire.AppHost.Sdk).
if ! command -v aspire >/dev/null 2>&1; then
  dotnet tool install -g Aspire.Cli
else
  dotnet tool update -g Aspire.Cli
fi

dotnet restore
aspire --version
bash .devcontainer/install-pi.sh
