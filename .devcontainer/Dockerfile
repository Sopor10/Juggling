# The base image and Dev Container Features provide the required BuildKit support.
FROM mcr.microsoft.com/devcontainers/dotnet:10.0

USER root

# dotnetup is the repository's canonical SDK provisioner. Keep only the
# version selector in the image build context; never copy the source tree into
# the image.
RUN apt-get update \
    && apt-get install --no-install-recommends -y ca-certificates curl \
    && rm -rf /var/lib/apt/lists/*

COPY --chown=vscode:vscode global.json /tmp/dotnetup/global.json

USER vscode

ENV HOME=/home/vscode \
    DOTNET_ROOT=/home/vscode/.local/share/dotnetup/dotnet \
    PATH=/home/vscode/.dotnetup:/home/vscode/.local/share/dotnetup/dotnet:/home/vscode/.dotnet/tools:${PATH}

RUN curl --proto '=https' --tlsv1.2 -fsSL --retry 3 \
      https://aka.ms/dotnet/dotnetup/preview/get-dotnetup.sh \
      | bash -s -- --install-dir /home/vscode/.dotnetup \
    && cd /tmp/dotnetup \
    && dotnetup install --interactive false --no-progress \
    && dotnet tool install --global Aspire.Cli --version 13.5.2
