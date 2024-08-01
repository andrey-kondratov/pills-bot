FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-bookworm-slim-arm64v8 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "PillsBot/PillsBot.Server/PillsBot.Server.csproj" \
    --runtime linux-arm64 --configuration Release \
    --output /app --self-contained true

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./bot"]
