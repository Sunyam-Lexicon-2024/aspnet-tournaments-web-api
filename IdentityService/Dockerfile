ARG BUILDKIT_INLINE_CACHE=0

FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0-bookworm AS development

ENV DOTNET_ENVIRONMENT=Development

COPY .devcontainer/entrypoint.sh /usr/local/bin/entrypoint.sh

RUN chmod +x /usr/local/bin/entrypoint.sh

USER vscode