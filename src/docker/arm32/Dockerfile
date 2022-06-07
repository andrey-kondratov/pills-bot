FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-buster-slim-arm32v7 AS base

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "PillsBot/PillsBot.Server/PillsBot.Server.csproj" \
    --runtime linux-arm --configuration Release \
    --output /app --self-contained true /p:PublishTrimmed=true

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./bot"]
