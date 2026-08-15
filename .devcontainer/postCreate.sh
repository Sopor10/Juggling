#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet/tools:$PATH"

# Match AppHost Aspire major (see Juggling.AppHost.csproj Aspire.AppHost.Sdk).
if ! command -v aspire >/dev/null 2>&1; then
  dotnet tool install -g Aspire.Cli
else
  dotnet tool update -g Aspire.Cli
fi

dotnet restore
aspire --version
