FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "PillsBot/PillsBot.Server/PillsBot.Server.csproj" \
    --runtime linux-musl-x64 --configuration Release \
    --output /app --self-contained true /p:PublishTrimmed=true

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./bot"]
