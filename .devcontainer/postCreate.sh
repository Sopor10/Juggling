#!/usr/bin/env bash
set -euo pipefail

eval "$(dotnetup env script --shell bash --dotnet --dotnetup | sed '/^hash -d /d')"

# Match AppHost Aspire major (see Juggling.AppHost.csproj Aspire.AppHost.Sdk).
if ! command -v aspire >/dev/null 2>&1; then
  dotnet tool install -g Aspire.Cli
else
  dotnet tool update -g Aspire.Cli
fi

dotnet restore
aspire --version
bash .devcontainer/install-pi.sh
