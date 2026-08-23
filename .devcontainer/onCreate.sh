#!/usr/bin/env bash
set -euo pipefail

if command -v sudo >/dev/null 2>&1; then
  sudo chown -R "$(id -u):$(id -g)" .
fi

curl --proto '=https' --tlsv1.2 -fsSL --retry 3 \
  https://aka.ms/dotnet/dotnetup/preview/get-dotnetup.sh | bash
sudo ln -sfn "${HOME}/.dotnetup/dotnetup" /usr/local/bin/dotnetup
dotnetup install --interactive false --no-progress
eval "$(dotnetup env script --shell bash --dotnet --dotnetup | sed '/^hash -d /d')"
