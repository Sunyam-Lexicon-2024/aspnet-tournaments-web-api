FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0-bookworm AS development
ARG BUILDKIT_INLINE_CACHE=0

ENV DOTNET_ENVIRONMENT=Development
COPY .devcontainer/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

USER vscode

# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH
WORKDIR /source

COPY Tournaments.API/*.csproj ./Tournaments.API
COPY Tournaments.Core/*.csproj ./Tournaments.Core
COPY Tournaments.Data/*.csproj ./Tournaments.Data
COPY Tournaments.Test/*.csproj ./Tournaments.Test
COPY Tournaments.sln .
RUN dotnet restore -a $TARGETARCH

COPY . .
RUN dotnet publish Tournaments.API -c Release --no-restore -a $TARGETARCH -o /app

# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS publish
ENV DOTNET_ENVIRONMENT=Production
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
# Uncomment to enable non-root user
# USER $APP_UID
ENTRYPOINT ["dotnet", "Tournaments.API.dll"]