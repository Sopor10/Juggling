# The lifecycle installs the pinned SDK in the image's shared directory.
export DOTNET_ROOT="/usr/share/dotnet"
export PATH="${DOTNET_ROOT}:${HOME}/.dotnet/tools:${PATH}"
