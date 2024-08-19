FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-bookworm-slim-arm64v8 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install --global dotnet-ef --version 8.*

WORKDIR /src
COPY . .

WORKDIR /src/PillsBot/PillsBot.Server
RUN dotnet publish -r linux-arm64 -c Release -o /app --self-contained
RUN dotnet ef migrations bundle -c PillsBotDbContext -r linux-x64 --configuration Release -o /app/migrate --self-contained

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./bot"]
