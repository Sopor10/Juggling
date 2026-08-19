# Shared by Dev Container lifecycle scripts. dotnetup cannot write to
# /usr/share/dotnet (system-managed), so the SDK lives in $HOME/.dotnet.
export DOTNET_ROOT="${HOME}/.dotnet"
export PATH="${DOTNET_ROOT}:${HOME}/.dotnet/tools:${PATH}"
