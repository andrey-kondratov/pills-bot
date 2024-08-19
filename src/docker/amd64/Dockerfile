FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-bookworm-slim AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install --global dotnet-ef --version 8.*

WORKDIR /src
COPY . .

WORKDIR /src/PillsBot/PillsBot.Server
RUN dotnet publish -r linux-x64 --configuration Release -o /app --self-contained
RUN dotnet ef migrations bundle -r linux-x64 --configuration Release -o /app/migrate --self-contained -c PillsBotDbContext

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./bot"]
