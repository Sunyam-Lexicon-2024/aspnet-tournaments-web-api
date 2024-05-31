ARG BUILDKIT_INLINE_CACHE=0

FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0-bookworm

WORKDIR /
RUN \
    git clone https://github.com/BorisWilhelms/create-dotnet-devcert && \
    cd create-dotnet-devcert/scripts && \
    chmod +x *.sh && \
    ./ubuntu-create-dotnet-devcert.sh && \
    cd /workspace && \
    rm -rf create-dotnet-devcert

USER vscode